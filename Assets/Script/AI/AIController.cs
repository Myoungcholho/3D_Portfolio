using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// AIController 클래스: 추상 클래스, AI의 상태 전환 및 동작을 관리
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PerceptionComponent))]
public abstract class AIController : MonoBehaviour
{
    // AI 상태 타입 정의
    public enum Type
    {
        Wait = 0, Patrol = 1, Approach = 2, Equip = 3, Action = 4, Damaged = 5, Fallback = 6, CirclePatrol = 7, Dead = 8
    }

    public event Action<Type, Type> OnAIStateTypeChanged; // 상태 변화 이벤트

    [SerializeField]
    protected float attackRange = 1.5f;           // 공격 범위
    [SerializeField]
    private float damageDelay = 1.5f;           // 데미지 받은 후 딜레이 시간
    [SerializeField]
    private float damageDelayRandom = 0.5f;     // 랜덤 딜레이 추가
    [SerializeField]
    protected float currentCoolTime = 0.0f;       // 내부 쿨타임 관리
    public float CurrentCoolTime
    {
        get => currentCoolTime;
    }

    protected bool bRetreat;  // 후퇴 여부 판단
    protected float fallbackTime = 0f;  // 후퇴 시간
    protected float circlePatrolTime = 0f;  // 원형 경로 시간
    protected Vector3 fallbackDirection;  // 후퇴 방향
    protected int randomDirection = 0;  // 원형 경로 방향

    // AI 상태 체크를 위한 프로퍼티
    public bool WaitMode { get => type == Type.Wait; }
    public bool PatrolMode { get => type == Type.Patrol; }
    public bool ApproachMode { get => type == Type.Approach; }
    public bool EquipMode { get => type == Type.Equip; }
    public bool ActionMode { get => type == Type.Action; }
    public bool DamagedMode { get => type == Type.Damaged; }
    public bool FallBackMode { get => type == Type.Fallback; }
    public bool CirclePatrolMode { get => type == Type.CirclePatrol; }
    public bool DeadMode { get => type == Type.Dead; }

    // 주요 컴포넌트
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

        weapon.OnEndEquip += OnEndEquip;  // 무기 장착 완료 이벤트
        weapon.OnEndDoAction += OnEndDoAction;  // 액션 종료 이벤트
    }

    protected virtual void Start()
    {
        SetWaitMode();  // 대기 모드로 시작
    }

    protected virtual void Update()
    {
        // 업데이트 로직은 상속받은 클래스에서 구현
    }

    protected abstract void FixedUpdate();  // 물리 업데이트는 상속받은 클래스에서 구현

    private void LateUpdate()
    {
        LateUpdate_Approach();  // 추적 중이라면 목표를 따라감
        LateUpdate_SetSpeed();  // 상태에 따른 애니메이션 속도 설정
    }

    // 추적 모드일 때 플레이어를 추적
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

    // 애니메이션 속도 설정
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

    // 대기 모드 설정
    protected void SetWaitMode()
    {
        if (WaitMode)
            return;

        ChangeType(Type.Wait);
        nav.isStopped = true;
    }

    // 추적 모드 설정
    protected void SetApproachMode()
    {
        if (ApproachMode)
            return;

        ChangeType(Type.Approach);
        nav.updateRotation = true;
        nav.isStopped = false;
    }

    // 후퇴 모드 설정
    protected void SetFallBackMode()
    {
        if (FallBackMode || state.DeadMode)
            return;

        fallbackTime = 0f;
        nav.updateRotation = false;
        ChangeType(Type.Fallback);
    }

    // 원형 경로 순찰 모드 설정
    protected void SetCirclePatrolMode()
    {
        if (CirclePatrolMode || state.DeadMode)
            return;

        nav.updateRotation = false;
        nav.isStopped = false;
        circlePatrolTime = 0f;
        randomDirection = UnityEngine.Random.Range(1, 3);  // 랜덤 방향 설정
        ChangeType(Type.CirclePatrol);
    }

    // 무기 장착 모드 설정
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

    // 액션 모드 설정
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

    // 데미지 모드 설정
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

    // 무기 장착 완료 시 대기 모드로 전환
    private void OnEndEquip()
    {
        SetWaitMode();
    }

    // 액션 완료 시 대기 모드로 전환
    protected virtual void OnEndDoAction()
    {
        SetCoolTime(damageDelay, damageDelayRandom);
        SetWaitMode();
    }

    // 상태 타입 전환 처리
    protected void ChangeType(Type type)
    {
        Type prevType = this.type;
        this.type = type;

        OnAIStateTypeChanged?.Invoke(prevType, type);  // 상태 변경 이벤트 호출
    }

    // 피격 종료 처리
    public void End_Damge()
    {
        SetCoolTime(damageDelay, damageDelayRandom);
        SetWaitMode();
    }

    // 쿨타임 설정
    private void SetCoolTime(float delayTime, float randomTime)
    {
        if (currentCoolTime < 0f)
        {
            currentCoolTime = 0f;
            return;
        }

        currentCoolTime = delayTime + UnityEngine.Random.Range(-randomTime, +randomTime);
    }

    // 쿨타임 체크
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