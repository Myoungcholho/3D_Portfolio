using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// AIController Ŭ����: �߻� Ŭ����, AI�� ���� ��ȯ �� ������ ����
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PerceptionComponent))]
public abstract class AIController : MonoBehaviour
{
    // AI ���� Ÿ�� ����
    public enum Type
    {
        Wait = 0, Patrol = 1, Approach = 2, Equip = 3, Action = 4, Damaged = 5, Fallback = 6, CirclePatrol = 7, Dead = 8
    }

    public event Action<Type, Type> OnAIStateTypeChanged; // ���� ��ȭ �̺�Ʈ

    [SerializeField]
    protected float attackRange = 1.5f;           // ���� ����
    [SerializeField]
    private float damageDelay = 1.5f;           // ������ ���� �� ������ �ð�
    [SerializeField]
    private float damageDelayRandom = 0.5f;     // ���� ������ �߰�
    [SerializeField]
    protected float currentCoolTime = 0.0f;       // ���� ��Ÿ�� ����
    public float CurrentCoolTime
    {
        get => currentCoolTime;
    }

    protected bool bRetreat;  // ���� ���� �Ǵ�
    protected float fallbackTime = 0f;  // ���� �ð�
    protected float circlePatrolTime = 0f;  // ���� ��� �ð�
    protected Vector3 fallbackDirection;  // ���� ����
    protected int randomDirection = 0;  // ���� ��� ����

    // AI ���� üũ�� ���� ������Ƽ
    public bool WaitMode { get => type == Type.Wait; }
    public bool PatrolMode { get => type == Type.Patrol; }
    public bool ApproachMode { get => type == Type.Approach; }
    public bool EquipMode { get => type == Type.Equip; }
    public bool ActionMode { get => type == Type.Action; }
    public bool DamagedMode { get => type == Type.Damaged; }
    public bool FallBackMode { get => type == Type.Fallback; }
    public bool CirclePatrolMode { get => type == Type.CirclePatrol; }
    public bool DeadMode { get => type == Type.Dead; }

    // �ֿ� ������Ʈ
    protected PerceptionComponent perception;
    protected WeaponComponent weapon;
    protected NavMeshAgent nav;
    protected StateComponent state;

    private Type type = Type.Wait;
    private Animator animator;

    protected virtual void Awake()
    {
        perception = GetComponent<PerceptionComponent>();
        weapon = GetComponent<WeaponComponent>();
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();

        weapon.OnEndEquip += OnEndEquip;  // ���� ���� �Ϸ� �̺�Ʈ
        weapon.OnEndDoAction += OnEndDoAction;  // �׼� ���� �̺�Ʈ
    }

    protected virtual void Start()
    {
        SetWaitMode();  // ��� ���� ����
    }

    protected virtual void Update()
    {
        // ������Ʈ ������ ��ӹ��� Ŭ�������� ����
    }

    protected abstract void FixedUpdate();  // ���� ������Ʈ�� ��ӹ��� Ŭ�������� ����

    private void LateUpdate()
    {
        LateUpdate_Approach();  // ���� ���̶�� ��ǥ�� ����
        LateUpdate_SetSpeed();  // ���¿� ���� �ִϸ��̼� �ӵ� ����
    }

    // ���� ����� �� �÷��̾ ����
    private void LateUpdate_Approach()
    {
        if (!ApproachMode)
            return;

        GameObject player = perception.GetPercievedPlayer();
        if (player != null)
        {
            nav.SetDestination(player.transform.position);
        }
    }

    // �ִϸ��̼� �ӵ� ����
    private void LateUpdate_SetSpeed()
    {
        switch (type)
        {
            case Type.Action:
                if (!bRetreat)
                {
                    animator.SetFloat("SpeedX", 0.0f);
                    animator.SetFloat("SpeedY", 0.0f);
                }
                break;
            case Type.Wait:
                if (bRetreat)
                {
                    animator.SetFloat("SpeedX", 0.0f);
                    animator.SetFloat("SpeedY", -nav.velocity.magnitude);
                }
                else
                {
                    animator.SetFloat("SpeedX", 0.0f);
                    animator.SetFloat("SpeedY", 0.0f);
                }
                break;
            case Type.Patrol:
            case Type.Approach:
                animator.SetFloat("SpeedX", 0.0f);
                animator.SetFloat("SpeedY", nav.velocity.magnitude);
                break;
            case Type.Fallback:
                animator.SetFloat("SpeedX", 0.0f);
                animator.SetFloat("SpeedY", -2);
                break;
            case Type.CirclePatrol:
                float speed = randomDirection == 1 ? nav.velocity.magnitude : -nav.velocity.magnitude;
                animator.SetFloat("SpeedX", speed);
                animator.SetFloat("SpeedY", 0.0f);
                break;
        }
    }

    // ��� ��� ����
    protected void SetWaitMode()
    {
        if (WaitMode)
            return;

        ChangeType(Type.Wait);
        nav.isStopped = true;
    }

    // ���� ��� ����
    protected void SetApproachMode()
    {
        if (ApproachMode)
            return;

        ChangeType(Type.Approach);
        nav.updateRotation = true;
        nav.isStopped = false;
    }

    // ���� ��� ����
    protected void SetFallBackMode()
    {
        if (FallBackMode || state.DeadMode)
            return;

        fallbackTime = 0f;
        nav.updateRotation = false;
        ChangeType(Type.Fallback);
    }

    // ���� ��� ���� ��� ����
    protected void SetCirclePatrolMode()
    {
        if (CirclePatrolMode || state.DeadMode)
            return;

        nav.updateRotation = false;
        nav.isStopped = false;
        circlePatrolTime = 0f;
        randomDirection = UnityEngine.Random.Range(1, 3);  // ���� ���� ����
        ChangeType(Type.CirclePatrol);
    }

    // ���� ���� ��� ����
    protected void SetEquipMode(WeaponType type)
    {
        if (EquipMode)
            return;

        ChangeType(Type.Equip);

        switch (type)
        {
            case WeaponType.Sword:
                weapon.SetSwordMode();
                break;
            case WeaponType.FireBall:
                weapon.SetFireBallMode();
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }

    // �׼� ��� ����
    protected virtual void SetActionMode()
    {
        if (ActionMode)
            return;

        nav.isStopped = true;
        ChangeType(Type.Action);

        GameObject player = perception.GetPercievedPlayer();
        if (player != null)
            transform.LookAt(player.transform.position);

        weapon.DoAction();
    }

    // ������ ��� ����
    public void SetDamageMode()
    {
        //if (EquipMode && !AnimatorHelper.DoesStateExistInLayer(animator, "Unarmed_None", 1))
        if (EquipMode)
        {
            animator.Play("Unarmed_None", 1);
            if (!weapon.IsEquippingMode())
                weapon.Begin_Equip();
            weapon.End_Equip();
        }

        if (ActionMode && animator.GetBool("IsAction"))
        {
            animator.Play($"{weapon.Type}.Blend Tree", 0);
            weapon.End_DoAction();
        }

        bool cancelCoolTime = EquipMode || ApproachMode || perception.GetPercievedPlayer() == null;

        if (cancelCoolTime)
            currentCoolTime = -1.0f;

        if (DamagedMode)
            return;

        nav.isStopped = true;
        ChangeType(Type.Damaged);
    }

    // ���� ���� �Ϸ� �� ��� ���� ��ȯ
    private void OnEndEquip()
    {
        SetWaitMode();
    }

    // �׼� �Ϸ� �� ��� ���� ��ȯ
    protected virtual void OnEndDoAction()
    {
        SetCoolTime(damageDelay, damageDelayRandom);
        SetWaitMode();
    }

    // ���� Ÿ�� ��ȯ ó��
    protected void ChangeType(Type type)
    {
        Type prevType = this.type;
        this.type = type;

        OnAIStateTypeChanged?.Invoke(prevType, type);  // ���� ���� �̺�Ʈ ȣ��
    }

    // �ǰ� ���� ó��
    public void End_Damge()
    {
        SetCoolTime(damageDelay, damageDelayRandom);
        SetWaitMode();
    }

    // ��Ÿ�� ����
    private void SetCoolTime(float delayTime, float randomTime)
    {
        if (currentCoolTime < 0f)
        {
            currentCoolTime = 0f;
            return;
        }

        currentCoolTime = delayTime + UnityEngine.Random.Range(-randomTime, +randomTime);
    }

    // ��Ÿ�� üũ
    protected virtual bool CheckCoolTime()
    {
        if (!WaitMode || currentCoolTime <= 0.0f)
            return false;

        currentCoolTime -= Time.fixedDeltaTime;

        if (currentCoolTime <= 0.0f || perception.GetPercievedPlayer() == null)
        {
            currentCoolTime = 0f;
            return false;
        }
        return true;
    }
}