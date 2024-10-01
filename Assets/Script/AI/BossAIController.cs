using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
public enum BossType
{
    Wait = 0, Move =1, Action = 2,Stun=3, Damage = 4,Equip=5, Rush=6,
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

    [Header("공격 후 딜레이 설정")]
    [SerializeField]
    private float attackDelay = 3.0f;           //공격 후 딜레이
    [SerializeField]
    private float attackRandomDelay = 1.0f;     //공격 딜레이에 추가할 시간 -1.0~+1.0

    [Header("내부 CoolTime Check용")]
    [SerializeField]
    private float currentCoolTime = 0.0f;       //내부 CoolTime 상태 관리 용
    public float CurrentCoolTime
    {
        get => currentCoolTime;
    }

    [Header("Pattern 수 명시")]
    [SerializeField]
    private int longRangePatternCount = 3;
    [SerializeField]
    private int shortRangePatternCount = 2;


    // Type 체크용 프로퍼티
    public bool WaitMode { get => type == BossType.Wait; }
    public bool MoveMode { get=>type == BossType.Move; }
    public bool ActionMode { get => type == BossType.Action; }
    public bool StunMode { get => type == BossType.Stun; }
    public bool DamageMode {  get => type == BossType.Damage; }
    public bool RushMode { get=>type == BossType.Rush; }

    
    private BossWeaponComponent weapon;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private HealthPointComponent healthPointComponent;
    private Rigidbody rb;

    private float stunTime;         // stunTime
    private Coroutine stopMoveCoroutine;

    [Header("JUMP")]
    // 점프 관련 변수
    public float jumpHeight = 5f;           // 점프 높이
    public float jumpDuration = 3f;         // 체공 시간
    private float jumpStartTime;            // 점프 시작 시간
    private Vector3 jumpVelocity;           // 점프 속도
    public float distanceInFrontOfPlayer = 2f; // 플레이어 앞에 도착할 거리
    [Header("착지 체크 Ray 정보")]
    [SerializeField]
    private float rayDistance = 2.0f;
    [SerializeField]
    private LayerMask groundLayer;
    private bool isJumping = false;
    [Header("Boss Rotation Speed")]
    [SerializeField]
    private float rotationSpeed = 10f;          // 보스 회전 속도


    private void Awake()
    {
        weapon = GetComponent<BossWeaponComponent>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        healthPointComponent = GetComponent<HealthPointComponent>();
        rb = GetComponent<Rigidbody>();

        weapon.OnEndDoAction += OnEndDoAction;          // AI 상태를 바꾸기 위해 연결
    }

    private void Start()
    {
        navMeshAgent.updateRotation = false;            // navMesh로 인한 회전 사용 안함.
        SetWaitMode();
    }

    private void Update()
    {
        if (weapon.Type != WeaponType.BossHammer)
            weapon.SetBossHammerMode();

        if (healthPointComponent.Dead)
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
        /* Vector3 directionToLookAtTarget = target.transform.position - transform.position;
         directionToLookAtTarget.y = 0;
         Quaternion lookRotation = Quaternion.LookRotation(directionToLookAtTarget);
         transform.rotation = lookRotation;*/

        Vector3 directionToLookAtTarget = target.transform.position - transform.position;
        directionToLookAtTarget.y = 0; // Y축 회전만 처리하도록 설정

        Quaternion lookRotation = Quaternion.LookRotation(directionToLookAtTarget);

        // 천천히 회전하기 위해 Slerp 사용
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }


    
    public float dashSpeed = 10f;
    private void FixedUpdate()
    {
        // 보스가 뛰는 상태라면
        if (isJumping)
        {
            // Ray를 바닥 아래로 쏴서 바닥 Layer가 걸리면 EndJump() 실행
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayDistance, groundLayer))
            {
                EndJump();
                return;
            }
            return;
        }


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


        float distance = Vector3.Distance(transform.position, target.transform.position);
        if(distance > closeRange)
        {
            if (ActionMode == true)
                return;

            int longAattern = UnityEngine.Random.Range(0, longRangePatternCount);
            if (longAattern == 1)
            {
                SetRushMode();
                return;
            }

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
    
    // 애니메이션 event 호출용
    private void Begin_SetMoveMode()
    {
        SetMoveMode();
    }

    Vector3 direction1;
    Vector3 direction2;
    int random;
    private IEnumerator MoveInCircle()
    {
        random = UnityEngine.Random.Range(1, 4);

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
            if (random == 1)
                nextPosition += direction1;
            // 오른쪽으로
            else if (random == 2)
                nextPosition += direction2;
            // 잠시 대기
            else if (random == 3)
                break;

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

        EnableRootMotion();
        weapon.DoAction(parttern);
    }

    private void SetRushMode()
    {
        if (RushMode == true)
            return;

        ChangeType(BossType.Rush);

        // Rush 
        animator.SetTrigger("Jump");
    }

    private void Begin_Jump()
    {
        Vector3 directionToPlayer = (target.transform.position - transform.position).normalized;
        Vector3 jumpTarget = target.transform.position - directionToPlayer * distanceInFrontOfPlayer;

        // 2. navMesh 잠깐 끄기
        navMeshAgent.enabled = false;

        // 3. 초기 속도 계산
        jumpVelocity = CalculateJumpVelocity(transform.position, jumpTarget, jumpHeight);

        // 4. Rigidbody를 사용한 이동
        rb.isKinematic = false;
        rb.velocity = jumpVelocity;

        jumpStartTime = Time.time;
    }

    private void Begin_IsJumping()
    {
        isJumping = true;
    }

    private void EndJump()
    {
        // 0. Ray 쏘기 멈추기
        isJumping = false;

        // 1. 점프를 마친 후, 이동처리 및 보간
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;

        // 2. navMesh 재활성
        navMeshAgent.enabled = true;

        // 3. Animation 설정
        animator.SetTrigger("EndAction");

        // 4. Mode 변경
        SetMoveMode();
    }

    Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float height)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0, endPoint.z - startPoint.z);

        float time = Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);

        Vector3 velocityXZ = displacementXZ / time;
        /*float drag = rb.drag;
        float dragFactor = Mathf.Exp(-drag * time);
        Vector3 velocityXZ = displacementXZ / (time * dragFactor);*/

        return velocityXZ + velocityY * -Mathf.Sign(gravity);
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
        DisableRootMotion();
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

    // 루트 모션 활성화 메서드
    public void EnableRootMotion()
    {
        animator.applyRootMotion = true;
    }

    // 루트 모션 적용 비활성화 메서드
    public void DisableRootMotion()
    {
        animator.applyRootMotion = false;
    }

    private bool CheckMode()
    {
        bool bCheck = false;
        bCheck |= ActionMode;
        bCheck |= RushMode;
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

        // 보스 착지 Ray
        // 레이가 그려질 시작점 (보스의 위치)에서 아래로 레이의 길이만큼 라인을 그림
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
    }
}
