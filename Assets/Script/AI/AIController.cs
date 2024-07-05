using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PerceptionComponent))]
public class AIController : MonoBehaviour
{
    [SerializeField]
    private float attackRange = 1.5f;
    [SerializeField]
    private float attackDelay = 5.0f;
    [SerializeField]
    private float attackDelayRandom = 0.5f;

    public enum Type
    {
        Wait =0, Patrol, Approach, Equip, Action, Damaged,
    }
    private Type type = Type.Wait;
    public bool WaitMode { get => type == Type.Wait; }
    public bool PatrolMode { get => type == Type.Patrol; }
    public bool ApproachMode { get => type == Type.Approach; }
    public bool EquipMode { get => type == Type.Equip; }
    public bool ActionMode { get => type == Type.Action; }
    public bool DamagedMode { get => type == Type.Damaged; }


    public event Action<Type, Type> OnAIStateTypeChanged;

    private NavMeshAgent navMeshAgent;
    private PerceptionComponent perception;     // 추적 컴포넌트
    private Animator animator;
    private PatrolComponent patrol;             // 순찰 컴포넌트
    private WeaponComponent weapon;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        perception = GetComponent<PerceptionComponent>();
        patrol = GetComponent<PatrolComponent>();
        weapon = GetComponent<WeaponComponent>();
        weapon.OnEndEquip += OnEndEquip;
        weapon.OnEndDoAction += OnEndDoAction;
    }


    /// <summary>
    /// 대기, 순찰, 감지 모드 업데이트
    /// </summary>
    private void FixedUpdate()
    {
        bool bCheck = false;
        bCheck |= (EquipMode == true);
        bCheck |= (ActionMode == true);

        if (bCheck) return;

        GameObject player = perception.GetPercievedPlayer();
        if(player ==null)
        {
            if(patrol == null)
            {
                SetWaitMode();  // 대기 모드
                return;
            }
            SetPatrolMode();    // 순찰 모드
            return;
        }

        // 추적 대상이 있다면 
        // 무기를 뽑는 상태로 변경하고, 해당 무기의 Type으로 설정
        if (weapon.UnarmedMode == true)
        {
            SetEquipMode(WeaponType.Sword);
            return;
        }

        // 대상으로부터의 거리가 가까워졌다면 공격
        // SetActionMode() ->  weapon.DoAction();
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp < attackRange)
        {
            if (weapon.SwordMode == true)
                SetActionMode();

            return;
        }

        SetApproachMode();  // 감지 모드
    }

    private void LateUpdate()
    {
        LateUpdate_Approach();      // 대상 player가 있다면 Dest 설정
        LateUpdate_SetSpeed();      // Animator Paramter 설정
    }

    private void LateUpdate_Approach()
    {
        if (ApproachMode == false)
            return;

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            return;

        navMeshAgent.SetDestination(player.transform.position);
    }

    private void LateUpdate_SetSpeed()
    {
        switch (type)
        {
            case Type.Wait:
            case Type.Action:
                {
                    animator.SetFloat("SpeedY", 0.0f);
                }
                break;
            case Type.Patrol:
            case Type.Approach:
                {
                    animator.SetFloat("SpeedY", navMeshAgent.velocity.magnitude);
                }
                break;

        }
    }

    /// <summary>
    /// 무기가 뽑혔다면 상태를 변경합니다.
    /// </summary>
    private void OnEndEquip()
    {
        SetWaitMode();
    }

    /// <summary>
    /// 공격이 끝나면 정지합니다.
    /// </summary>
    private void OnEndDoAction()
    {
        StartCoroutine(Wait_EndDoAction_Random());
    }

    /// <summary>
    /// 공격 후 딜레이 합니다.
    /// </summary>
    private IEnumerator Wait_EndDoAction_Random()
    {
        float time = 0.0f;
        time += attackDelay;
        time += UnityEngine.Random.Range(-attackDelayRandom, +attackDelayRandom);

        yield return new WaitForSeconds(time);

        SetWaitMode();
    }

    #region SetMode
    private void SetWaitMode()
    {
        if (WaitMode == true)
            return;

        ChangeType(Type.Wait);
        navMeshAgent.isStopped = true;
    }

    private void SetApproachMode()
    {
        if (ApproachMode == true)
            return;

        ChangeType(Type.Approach);
        navMeshAgent.isStopped = false;
    }

    private void SetPatrolMode()
    {
        if (PatrolMode == true)
            return;

        ChangeType(Type.Patrol);
        navMeshAgent.isStopped = false;
        patrol.StartMove();
    }

    private void SetEquipMode(WeaponType type)
    {
        if (EquipMode == true)
            return;

        ChangeType(Type.Equip);
        //navMeshAgent.isStopped = true;

        switch (type)
        {
            case WeaponType.Sword:
                weapon.SetSwordMode();
                break;

            default:
                Debug.Assert(false);
                break;
        }
    }

    private void SetActionMode()
    {
        if (ActionMode == true)
            return;

        navMeshAgent.isStopped = true;
        ChangeType(Type.Action);

        GameObject player = perception.GetPercievedPlayer();
        if (player != null)
            transform.LookAt(player.transform.position);

        weapon.DoAction();
    }

    #endregion
    private void ChangeType(Type type)
    {
        Type prevType = this.type;
        this.type = type;

        OnAIStateTypeChanged?.Invoke(prevType, type);
    }
}
