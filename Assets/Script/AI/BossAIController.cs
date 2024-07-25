using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
public enum BossType
{
    Wait = 0, Move =1, Action = 2,Stun=3, Damage = 4,Equip=5,
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(BossWeaponComponent))]
public class BossAIController : MonoBehaviour
{
    public GameObject target;
    public Transform centerPosition;            // 외곽으로 나가는 경우 가운데로 그냥 텔포해버리게

    private BossType type = BossType.Wait;
    public float closeRange = 5.0f;             // 근거리 반경

    // State Call delegate
    public event Action<BossType, BossType> OnAIStateTypeChanged;

    [SerializeField]
    private float moveSpeed = 2.0f;
    [SerializeField]
    protected float attackRange = 1.5f;         //무기 마다 다르므로 위치 고려
    [SerializeField]
    private float attackDelay = 5.0f;           //공격 후 딜레이?
    [SerializeField]
    private float attackRandomDelay = 1.0f;     //공격 딜레이에 추가할 시간 -1.0~+1.0

    [SerializeField]
    private float currentCoolTime = 0.0f;       //내부 CoolTime 상태 관리 용
    public float CurrentCoolTime
    {
        get => currentCoolTime;
    }
    [SerializeField]
    private int longRangePatternCount = 3;
    [SerializeField]
    private int shortRangePatternCount = 2;

    public float moveEnemyDistance = 1f;    // 적이 매 프레임 위치 연산할 지점
    private float speed; // 원을 따라 움직이는 속도



    // Type 체크용 프로퍼티
    public bool WaitMode { get => type == BossType.Wait; }
    public bool MoveMode { get=>type == BossType.Move; }
    public bool ActionMode { get => type == BossType.Action; }
    public bool StunMode { get => type == BossType.Stun; }
    public bool DamageMode {  get => type == BossType.Damage; }

    
    private BossWeaponComponent weapon;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private HealthPointComponent healthPointComponent;

    private float stunTime;         // stunTime
    private Coroutine stopMoveCoroutine;
    


    private void Awake()
    {
        weapon = GetComponent<BossWeaponComponent>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        healthPointComponent = GetComponent<HealthPointComponent>();
        

        weapon.OnEndDoAction += OnEndDoAction;          // AI 상태를 바꾸기 위해 연결
    }

    private void Start()
    {
        navMeshAgent.updateRotation = false;
        speed = navMeshAgent.speed;
        SetWaitMode();
    }

    private void Update()
    {
        if(healthPointComponent.Dead)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = true;
            return;
        }

        // 바라볼 대상이 있다면 바라보기
        if (target != null)
        {
            if(ActionMode == true)
                return;

            lookPlayerUpdate();
        }


    }

    private void lookPlayerUpdate()
    {
        Vector3 directionToLookAtTarget = target.transform.position - transform.position;
        directionToLookAtTarget.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToLookAtTarget);
        //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
        transform.rotation = lookRotation;
    }

    private void FixedUpdate()
    {
        if (weapon.Type != WeaponType.BossHammer)
            weapon.SetBossHammerMode();

        // Action Mode 라면 return
        if (CheckMode())
            return;

        if (CheckStunTime())
        {
            SetStunMode();
            return;
        }

        if (CheckCoolTime())
        {
            SetMoveMode();
            return;
        }


        if (target == null)
        {
            Debug.Assert(target == null);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if(distance > closeRange)
        {
            if (ActionMode == true)
                return;

            int longAattern = UnityEngine.Random.Range(0, longRangePatternCount);
            SetActionMode(longAattern);
            return;
        }

        int maxPatternCount = longRangePatternCount + shortRangePatternCount;
        int shortPattern = UnityEngine.Random.Range(longRangePatternCount, maxPatternCount);
        SetActionMode(shortPattern) ;
        return;
    }

    private void LateUpdate()
    {
        LateUpdate_SetSpeed();
    }
    // Animation Speed Setting.
    private void LateUpdate_SetSpeed()
    {
        switch (type)
        {
            case BossType.Wait:
            case BossType.Action:
                {
                    animator.SetFloat("SpeedY", 0.0f);
                }
                break;
            case BossType.Move:                
                {
                    if(random == 0)
                        animator.SetFloat("SpeedX", 0f);
                    else if(random == 1)
                        animator.SetFloat("SpeedX", 2f);
                    else if(random == 2)
                        animator.SetFloat("SpeedX", -2f);

                }
                break;
            default:
                {
                    animator.SetFloat("SpeedY", 0.0f);
                    animator.SetFloat("SpeedX", 0.0f);
                }
                break;

        }
    }

    

    #region SetMethod
    protected void SetWaitMode()
    {
        if (WaitMode == true)
            return;

        ChangeType(BossType.Wait);
    }

    
    protected void SetMoveMode()
    {
        if (MoveMode == true) 
            return;

        ChangeType(BossType.Move);
        navMeshAgent.isStopped = false;

        stopMoveCoroutine = StartCoroutine(MoveInCircle());

        // 경로 계산이 안된다면.. 가운데로 텔포 
        //transform.position = centerPosition.position;
    }

    Vector3 direction1;
    Vector3 direction2;
    int random;
    private IEnumerator MoveInCircle()
    {
        random = UnityEngine.Random.Range(1, 3);

        while (true)
        {
            if (MoveMode == false)
                break;

            // 양 옆 방향벡터 구하기
            direction1 = Quaternion.AngleAxis(75f, Vector3.up) * transform.forward * 2f; 
            direction2 = Quaternion.AngleAxis(-75f, Vector3.up) * transform.forward * 2f;

            NavMeshHit hit;
            Vector3 nextPosition = transform.position;
            // 왼쪽으로
            if(random == 1)
                nextPosition += direction1;
            // 오른쪽으로
            else if(random == 2)
                nextPosition += direction2;

            if (NavMesh.SamplePosition(nextPosition, out hit, 2.0f, NavMesh.AllAreas))
            {
                // 유효한 위치로 설정
                nextPosition = hit.position;
            }

            navMeshAgent.SetDestination(nextPosition);

            yield return new WaitForSeconds(0.5f);
        }
        random = 0;
    }

    protected void SetActionMode(int parttern)
    {
        if (ActionMode == true)
            return;

        ChangeType(BossType.Action);

        navMeshAgent.ResetPath();
        navMeshAgent.isStopped = true;
        if (stopMoveCoroutine != null)
        {
            StopCoroutine(stopMoveCoroutine); // 코루틴 중단
            stopMoveCoroutine = null; // 참조 해제
        }

        weapon.DoAction(parttern);        
    }

    private void SetStunMode()
    {
        if (StunMode == true)
            return;

        ChangeType(BossType.Stun);
    }

    #endregion

    private void OnEndDoAction()
    {
        SetCoolTime(attackDelay, attackRandomDelay);
        SetMoveMode();
        animator.SetTrigger("EndAction");
    }

    // 데미지 피격 시 Enemy.cs OnDamged()에서 호출됨.
    public void OnDamaged()
    {

    }

    public void SetBossTarget(GameObject obj)
    {
        target = obj;
    }

    // Change Type
    protected void ChangeType(BossType type)
    {
        BossType prevType = this.type;
        this.type = type;

        OnAIStateTypeChanged?.Invoke(prevType, type);
    }

    // CoolTime Setting
    private void SetCoolTime(float delayTime, float randomTime)
    {
        float time = 0.0f;
        time += delayTime;
        time += UnityEngine.Random.Range(-randomTime, +randomTime);

        currentCoolTime = time;
    }

    // 쿨타임이 남아 있다면 True, 호출 시 마다 -0.02초
    private bool CheckCoolTime()
    {
        currentCoolTime -= Time.fixedDeltaTime;

        if (currentCoolTime <= 0.0f)
            return false;

        bool bCheckCoolTimeZero = false;
        bCheckCoolTimeZero |= (currentCoolTime <= 0.0f);

        if (bCheckCoolTimeZero)
        {
            currentCoolTime = 0f;
            return false;
        }
        return true;
    }

    // 스턴 시간이 남아 있다면 True, 호출 시 마다 -0.02초
    private bool CheckStunTime()
    {
        stunTime -= Time.fixedDeltaTime;

        if (stunTime <= 0.0f)
            return false;

        bool bCheckStunTimeZero = false;
        bCheckStunTimeZero |= (stunTime <= 0.0f);
        if(bCheckStunTimeZero)
        {
            stunTime = 0f;
            return false;
        }
        return true;
    }

    private bool CheckMode()
    {
        bool bCheck = false;
        bCheck |= ActionMode;
        bCheck |= healthPointComponent.Dead;
        bCheck |= (target == null);

        return bCheck;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, closeRange);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + direction1 );
        Gizmos.DrawLine(transform.position, transform.position + direction2 );
    }
}
