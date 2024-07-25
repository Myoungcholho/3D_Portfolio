using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PerceptionComponent))]
public abstract class AIController : MonoBehaviour
{
    public enum Type
    {
        Wait = 0, Patrol =1, Approach =2, Equip =3, Action =4, Damaged =5,
    }

    public event Action<Type, Type> OnAIStateTypeChanged;

    [SerializeField]
    protected float attackRange = 1.5f;           //���� ���� �ٸ��Ƿ� ��ġ ���
    [SerializeField]
    private float attackDelay = 5.0f;           //���� �� ������?
    [SerializeField]
    private float damageDelay = 1.5f;           //������ ���� �� ������ �ð�
    [SerializeField]
    private float damageDelayRandom = 0.5f;     //���� �������� ���� �� �߰���
    [SerializeField]
    protected float currentCoolTime = 0.0f;       //���� CoolTime ���� ���� ��
    public float CurrentCoolTime
    {
        get => currentCoolTime;
    }

    protected bool bRetreat;                    // ���������� �Ǵ�

    // Type üũ�� ������Ƽ
    public bool WaitMode { get => type == Type.Wait; }
    public bool PatrolMode { get => type == Type.Patrol; }
    public bool ApproachMode { get => type == Type.Approach; }
    public bool EquipMode { get => type == Type.Equip; }
    public bool ActionMode { get => type == Type.Action; }
    public bool DamagedMode { get => type == Type.Damaged; }

    protected PerceptionComponent perception;
    protected WeaponComponent weapon;
    protected NavMeshAgent nav;

    private Type type = Type.Wait;
    private Animator animator;

    protected virtual void Awake()
    {
        perception = GetComponent<PerceptionComponent>();
        weapon = GetComponent<WeaponComponent>();
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        weapon.OnEndEquip += OnEndEquip;
        weapon.OnEndDoAction += OnEndDoAction;
    }

    protected virtual void Start()
    {
        SetWaitMode();
    }

    protected virtual void Update()
    {

    }

    protected abstract void FixedUpdate();

    private void LateUpdate()
    {
        LateUpdate_Approach();
        LateUpdate_SetSpeed();
    }

    // ���� �� �̶�� Player�� ��ġ�� ��� navMeshDest ����
    private void LateUpdate_Approach()
    {
        if (ApproachMode == false)
            return;

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            return;

        nav.SetDestination(player.transform.position);
    }

    //private void LateUpdate_


    // Animation Speed Setting.
    private void LateUpdate_SetSpeed()
    {
        switch (type)
        {
            case Type.Action:
                {
                    if (bRetreat)
                        break;
                    animator.SetFloat("SpeedY", 0.0f);
                }
                break;
            case Type.Wait:
                {
                    if (bRetreat)
                    {
                        animator.SetFloat("SpeedY", -nav.velocity.magnitude);
                    }
                }
                break;
            case Type.Damaged:
                {
                    animator.SetFloat("SpeedY", 0.0f);
                }
                break;
            case Type.Patrol:
            case Type.Approach:
                {
                    animator.SetFloat("SpeedY", nav.velocity.magnitude);
                }
                break;

        }
    }


    protected void SetWaitMode()
    {
        if (WaitMode == true)
            return;

        ChangeType(Type.Wait);
        nav.isStopped = true;
    }

    protected void SetApproachMode()
    {
        if(ApproachMode == true) 
            return;

        ChangeType(Type.Approach);
        nav.isStopped = false;
    }

    protected void SetEquipMode(WeaponType type)
    {
        if (EquipMode == true)
            return;

        ChangeType(Type.Equip);

        // WeaponComponent Call
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

    protected virtual void SetActionMode()
    {
        if (ActionMode == true)
            return;

        nav.isStopped = true;
        ChangeType(Type.Action);

        GameObject player = perception.GetPercievedPlayer();
        if (player != null)
            transform.LookAt(player.transform.position);

        weapon.DoAction();
    }

    public void SetDamageMode()
    {
        // ���� �� �ǰ� �� �߰� ����
        if(EquipMode == true)
        {
            if (!AnimatorHelper.DoesStateExistInLayer(animator, "Unarmed_None", 1))
                return;
            animator.Play("Unarmed_None", 1);

            if (weapon.IsEquippingMode() == false)
                weapon.Begin_Equip();

            weapon.End_Equip();
        }
        // ���� �� �ǰ� �� �߰� ����
        if(ActionMode == true)
        {
            /*if (!AnimatorHelper.DoesStateExistInLayer(animator, $"{weapon.Type}.Blend Tree", 0))
                return;*/

            animator.Play($"{weapon.Type}.Blend Tree", 0);

            /*if (!AnimatorHelper.DoesParameterExist(animator, "IsAction"))
                return;*/

            if (animator.GetBool("IsAction") == true)
                weapon.End_DoAction();
        }


        bool bCancledCoolTime = false;
        bCancledCoolTime |= EquipMode;
        bCancledCoolTime |= ApproachMode;
        bCancledCoolTime |= (perception.GetPercievedPlayer() == null);

        if (bCancledCoolTime == true)
            currentCoolTime = -1.0f;

        if (DamagedMode == true)
            return;

        nav.isStopped = true;
        ChangeType(Type.Damaged);
    }

    private void OnEndEquip()
    {
        SetWaitMode();
    }

    private void OnEndDoAction()
    {
        SetCoolTime(damageDelay, damageDelayRandom);
        SetWaitMode();
    }

    // Change Type
    protected void ChangeType(Type type)
    {
        Type prevType = this.type;
        this.type = type;

        OnAIStateTypeChanged?.Invoke(prevType, type);
    }

    // �ִϸ��̼��� End_Damaged ȣ�� �� ���� ȣ�� �ޱ� ����
    // �ǰ� �� ��Ÿ�� ���ð� Wait���� ������.
    public void End_Damge()
    {
        SetCoolTime(damageDelay, damageDelayRandom);
        SetWaitMode();
    }

    // CoolTime Setting
    private void SetCoolTime(float delayTime, float randomTime)
    {
        if(currentCoolTime <0f)
        {
            currentCoolTime = 0f;
            return;
        }

        float time = 0.0f;
        time += delayTime;
        time += UnityEngine.Random.Range(-randomTime, +randomTime);

        currentCoolTime = time;
    }

    // CoolTime Check, WaitMode�� ��쿡�� CoolTime�� �پ��.
    protected virtual bool CheckCoolTime()
    {
        if (WaitMode == false)
            return false;
        if (currentCoolTime <= 0.0f)
            return false;

        currentCoolTime -= Time.fixedDeltaTime;

        bool bCheckCoolTimeZero = false;
        bCheckCoolTimeZero |= (currentCoolTime <= 0.0f);
        bCheckCoolTimeZero |= (perception.GetPercievedPlayer() == null);

        if (bCheckCoolTimeZero)
        {
            currentCoolTime = 0f;
            return false;
        }
        return true;
    }
}
