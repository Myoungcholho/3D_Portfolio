using System.Collections;
using Tiny;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using StateType = StateComponent.StateType;
using DamageStateType = StateComponent.DamageStateType;
using Unity.VisualScripting;

public enum EvadeDirection
{
    Forward, Backward, Left, Right, ForwardLeft, ForwardRight, BackwardLeft, BackwardRight
}

public class PlayerMovingComponent : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 2.0f;
    [SerializeField]
    private float runSpeed = 4.0f;
    [SerializeField]
    private float fastSpeed = 2f;
    [Header("움직임 보간 수치 높을 수록 전환 빠름")]
    [SerializeField]
    private float sensitivity = 5f;
    [Header("잠깐 눌렀다 땐 경우 움직임 제한하기 위함")]
    [SerializeField]
    private float deadZone = 0.2f;

    [Header("Evade Distance")]
    [SerializeField]
    private float fistEvadeDistance = 5f;
    [SerializeField]
    private float swordEvadeDistance = 5f;
    [SerializeField]
    private float hammerEvadeDistance = 5f;
    [SerializeField]
    private float staffEvadeDistance = 5f;
    [SerializeField]
    private float dualEvadeDistance = 5f;

    [SerializeField]
    private GameObject StaffParticlePrefab;

    private bool bCanMove = true;

    private Animator animator;
    private Rigidbody rb;
    private WeaponComponent weapon;
    private StateComponent state;
    private new Renderer[] renderer;

    private Vector2 inputMove;
    private Vector2 currInputMove;
    private bool bRun;
    private Vector2 velocity;
    
    private float yIncreaseTime = 0.0f; // Y축 값이 4일 때의 지속 시간을 추적


    // Evade시 활성화 할 Collider
    public BoxCollider forwardCollider;
    public BoxCollider backwardCollider;
    public BoxCollider leftCollider;
    public BoxCollider rightCollider;

    // 반격 시 진행 중인 코루틴 정지 위함
    private Coroutine moveCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        weapon = GetComponent<WeaponComponent>();
        state = GetComponent<StateComponent>();

        Awake_BindPlayerInput();

        //Evade용 등록함수
        state.OnStateTypeChanged += OnStateTypeChanged;
        state.OnDamageStateChanged += OnDamageStateTypeChanged;

    }

    private void Start()
    {
        DeactivateAllColliders();
    }

    private void Awake_BindPlayerInput()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction moveAction = actionMap.FindAction("Move");
        moveAction.performed += Input_Move_Performed;
        moveAction.canceled += Input_Move_Cancled;

        InputAction runAction = actionMap.FindAction("Run");
        runAction.started += Input_Run_Started;
        runAction.canceled += Input_Run_Cancled;
    }

    [HideInInspector]
    public float currentSpeed;
    private float smoothSpeed;      // Shift 보간
    private void Update()
    {
        currInputMove = Vector2.SmoothDamp(currInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        if (bCanMove == false)
            return;

        bool bCheck = false;
        bCheck |= state.DamagedMode == true;
        bCheck |= state.EvadeMode == true;
        bCheck |= state.DodgedMode == true;
        bCheck |= state.InstantKillMode == true;
        bCheck |= state.UsingSkillMode == true;

        if(bCheck)
            return;

        Vector3 direction = Vector3.zero;
        float targetSpeed = bRun ? runSpeed : walkSpeed;
        smoothSpeed = Mathf.Lerp(smoothSpeed, targetSpeed, Time.deltaTime * sensitivity);

        // 데드존 값을 올려 찔끔찔끔 가는걸 방지
        if (currInputMove.magnitude > deadZone)
        {
            direction = (Vector3.right * currInputMove.x) + (Vector3.forward * currInputMove.y);
            direction = direction.normalized * smoothSpeed;
        }


        if (direction.z >= 0.0f)
        {
            if (direction.z >= 1.0f * runSpeed-0.1f)
            {
                yIncreaseTime += Time.deltaTime;
            }
            else
            {
                yIncreaseTime -= Time.deltaTime;
            }

            yIncreaseTime = Mathf.Clamp(yIncreaseTime, 0.0f, 2.0f);
            direction.z += (yIncreaseTime * fastSpeed);
        }
        else
        {
            yIncreaseTime = 0f;
        }


        currentSpeed = direction.magnitude;
        transform.Translate(direction * Time.deltaTime);
        
        if (weapon.UnarmedMode)
        {
            animator.SetFloat("SpeedY", direction.magnitude);

            return;
        }

        animator.SetFloat("SpeedX", currInputMove.x * smoothSpeed);
        animator.SetFloat("SpeedY", currInputMove.y * smoothSpeed + yIncreaseTime);
    }


    //플레이어 상태가 변경되면 호출됨.
    private void OnStateTypeChanged(StateType prevType, StateType newType)
    {
        switch (newType)
        {
            case StateType.Evade:
                {
                    ExecuteEvade();
                }
                break;
        }
    }

    private void OnDamageStateTypeChanged(DamageStateType prevType, DamageStateType newType)
    {
        switch (newType)
        {
            case DamageStateType.Normal:
                {
                    rb.isKinematic = false;
                }
                break;
            case DamageStateType.SuperArmor:
                {
                    rb.isKinematic = true;
                }
                break;
        
        }

    }

    private void ExecuteEvade()
    {
        Vector2 value = inputMove;
        EvadeDirection direction = EvadeDirection.Forward;          // 4방향 저장
        EvadeDirection staffDirection = EvadeDirection.Forward;     // 8방향 저장

        // 키 입력이 없는 경우
        if (value.y == 0.0f)
        {
            direction = EvadeDirection.Forward;
            staffDirection = direction;

            if (value.x < 0.0f)
            {
                direction = EvadeDirection.Left;
                staffDirection = direction;
            }
            else if (value.x > 0.0f)
            {
                direction = EvadeDirection.Right;
                staffDirection = direction;
            }
        }

        // 앞 입력 시
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;
            staffDirection = direction;
            // 왼쪽 대각선
            if (value.x < 0.0f)
            {
                staffDirection = EvadeDirection.ForwardLeft;
                
            }
            // 오른쪽 대각선
            else if (value.x > 0.0f)
            {
                staffDirection = EvadeDirection.ForwardRight;
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
            staffDirection = direction;
            // 왼쪽 뒤 대각선
            if (value.x < 0.0f)
            {
                staffDirection = EvadeDirection.BackwardLeft;                
            }
            // 오른쪽 뒤 대각선
            else if (value.x > 0.0f)
            {
                staffDirection = EvadeDirection.BackwardRight;
            }
        }
        animator.SetInteger("Direction", (int)direction);
        animator.SetTrigger("Evade");
        EnableEvadeCollider(direction);                                 // 회피 판정용 Collider 활성

        if (animator.GetInteger("WeaponType") == (int)WeaponType.Fist)
        {
            FistEvade(direction);
        }

        if (animator.GetInteger("WeaponType") == (int)WeaponType.Sword)
        {
            SwordEvade(direction);
        }

        if (animator.GetInteger("WeaponType") == (int)WeaponType.Hammer)
        {
            HammerEvade(direction);
        }

        if (animator.GetInteger("WeaponType") == (int)WeaponType.FireBall)
        {
            StaffEvade(staffDirection);
        }

        if (animator.GetInteger("WeaponType") == (int)WeaponType.DualSword)
        {
            DualEvade(direction);
        }
    }


    // Fist Evade
    private void FistEvade(EvadeDirection direction)
    {
        CheckCollisionAndMove(direction,fistEvadeDistance, 0.5f);
    }

    // Sword Evade
    private void SwordEvade(EvadeDirection direction)
    {
        CheckCollisionAndMove(direction, swordEvadeDistance, 0.5f);
    }

    // Hammer Evade
    private void HammerEvade(EvadeDirection direction)
    {
        CheckCollisionAndMove(direction,hammerEvadeDistance,0.5f);
    }

    // Staff Evade
    private void StaffEvade(EvadeDirection direction)
    {
        CheckCollisionAndMove(direction, staffEvadeDistance,0.25f, true);

        // 텔포 시 파티클 생성
        Debug.Assert(StaffParticlePrefab != null);
        if (StaffParticlePrefab != null)
        {
            Instantiate<GameObject>(StaffParticlePrefab, transform.position, transform.rotation);
        }
    }

    [SerializeField]
    private GameObject dualEvadeTrailPrefab;                 // evade Trail 프리팹
    private GameObject dualEvadeTrailObject;                 // Instance Object;
    // Dual Evade
    private void DualEvade(EvadeDirection direction)
    {
        CheckCollisionAndMove(direction,dualEvadeDistance,0.1f,true);

        // trail 생성
        if(dualEvadeTrailPrefab != null)
        {
            Vector3 pos = transform.position;
            pos += new Vector3(0, 0.25f, 0);

            Quaternion quaternion = transform.rotation;

            dualEvadeTrailObject = Instantiate<GameObject>(dualEvadeTrailPrefab, pos, quaternion,transform);
            Destroy(dualEvadeTrailObject, 0.4f);
        }

    }

    // 이동 거리 가능 판단 및 이동 toggleRenderer True면 렌더링 비활성
    private void CheckCollisionAndMove(EvadeDirection direction, float distance,float duration, bool toggleRenderer = false)
    {
        Vector3 moveDirection = Vector3.zero;
        Vector3 position = transform.position;
        position += new Vector3(0, 0.9f, 0);

        switch (direction)
        {
            case EvadeDirection.Forward:
                moveDirection = transform.forward;
                break;
            case EvadeDirection.Backward:
                moveDirection = -transform.forward;
                break;
            case EvadeDirection.Left:
                moveDirection = -transform.right;
                break;
            case EvadeDirection.Right:
                moveDirection = transform.right;
                break;
            case EvadeDirection.ForwardLeft:
                moveDirection = (transform.forward - transform.right).normalized;
                break;
            case EvadeDirection.ForwardRight:
                moveDirection = (transform.forward + transform.right).normalized;
                break;
            case EvadeDirection.BackwardLeft:
                moveDirection = (-transform.forward - transform.right).normalized;
                break;
            case EvadeDirection.BackwardRight:
                moveDirection = (-transform.forward + transform.right).normalized;
                break;
        }

        // Ray를 쏴서 충돌 여부 체크
        RaycastHit hit;
        Vector3 destination;

        // 확인용 Ray
        Debug.DrawRay(position, moveDirection * distance, Color.green, 2f);

        if (Physics.Raycast(position, moveDirection, out hit, distance))
        {
            // 벽에 부딪힌 경우, 충돌 지점까지만 이동
            destination = hit.point;

            // 충돌 지점까지 Ray를 다른 색상으로 그리기
            Debug.DrawLine(position, hit.point, Color.red, 2f);
        }
        else
        {
            // 벽에 부딪히지 않는 경우, 지정된 거리만큼 이동
            destination = transform.position + moveDirection * distance;
        }

        // 목적지로 이동
        // 예외1) - Sword의 반격이 들어오면 이동을 멈추어야 함 
        moveCoroutine = StartCoroutine(MoveToPosition(destination, duration,toggleRenderer));
    }

    // 목표 지점으로 움직이는 코루틴
    private IEnumerator MoveToPosition(Vector3 target,float duration, bool toggleRenderer = false)
    {
        // 랜더 끄기
        if (toggleRenderer)
        {
            ToggleRenderers(false);
        }

        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        float actualDuration = duration; // 애니메이션 속도에 따른 실제 duration

        // Lerp를 사용해 1초 동안 이동
        while (elapsedTime < actualDuration)
        {
            //transform.position = Vector3.Lerp(startPos, target, elapsedTime / duration);
            
            // 애니메이션 속도 가져오기
            float animSpeed = animator.speed;

            // 애니메이션 속도에 따라 실제 지속 시간을 증가시킴
            actualDuration = duration / animSpeed;  // 애니메이션 속도가 느려지면 더 긴 duration으로 변경

            // 이동 시간 비율 계산 (애니메이션 속도에 맞춰)
            float t = elapsedTime / actualDuration;

            // Lerp를 사용하여 캐릭터 이동
            transform.position = Vector3.Lerp(startPos, target, Mathf.Clamp01(t));

            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 정확한 위치로 이동 (Lerp가 끝났을 때)
        transform.position = target;

        // 랜더 키기
        if (toggleRenderer)
        {
            ToggleRenderers(true);
        }

        animator.SetTrigger("TestExit");
        Move();
        End_Evade();                                // 코루틴을 중간에 끊었는데 너가 왜해?
    }

    // 목표 지점 이동 캔슬
    public void CancelMove()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null; // 코루틴 참조를 초기화
        }
    }

    // 렌더러를 켜고 끄는 메서드
    private void ToggleRenderers(bool enabled)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rd in renderers)
        {
            rd.enabled = enabled;
        }
    }


    private void EnableEvadeCollider(EvadeDirection direction)
    {
        switch(direction)
        {
            case EvadeDirection.Forward:
                forwardCollider.enabled = true;
                break;
            case EvadeDirection.Backward:
                backwardCollider.enabled = true;
                break;
            case EvadeDirection.Left:
                leftCollider.enabled = true;
                break;
            case EvadeDirection.Right:
                rightCollider.enabled = true;
                break;
        }
    }

    public void Move()
    {
        bCanMove = true;
    }
    public void Stop()
    {
        bCanMove = false;
    }

    private void Input_Move_Performed(InputAction.CallbackContext context)
    {
         inputMove = context.ReadValue<Vector2>();
    }

    private void Input_Move_Cancled(InputAction.CallbackContext context)
    {
        inputMove = Vector2.zero;
    }

    private void Input_Run_Started(InputAction.CallbackContext context)
    {
        bRun = true;
    }

    private void Input_Run_Cancled(InputAction.CallbackContext context)
    {
        bRun = false;
    }

    /// <summary>
    /// 구르기 끝나면 Idle모드로 변경
    /// </summary>
    public void End_Evade()
    {
        state.SetIdleMode();
        DeactivateAllColliders();
    }

    private void DeactivateAllColliders()
    {
        forwardCollider.enabled = false;
        backwardCollider.enabled = false;
        leftCollider.enabled = false;
        rightCollider.enabled = false;
    }


    // 대상의 앞으로 위치하기
    private float moveSpeed = 5f;
    // target : 목표한 타겟으로 이동
    // stoppingDistacne : 정확한 지점이 아닌 앞까지 이동하기 위한 차이값
    public IEnumerator MoveToTarget(Transform target, float stoppingDistance)
    {
        // 목표 위치에 도달할 때까지 반복
        while (Vector3.Distance(transform.position, target.position) > stoppingDistance)
        {
            // 대상의 위치까지의 방향 벡터 계산
            Vector3 direction = (target.position - transform.position).normalized;

            // 현재 위치에서 대상 위치로 이동 (MoveTowards 사용)
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            // 프레임이 끝날 때까지 대기
            yield return null;
        }
    }

    #region FistQSkillDash

    // Fist Q 스킬 시 앞으로 살짝 뛰게 하기 위한 이벤트 메서드
    // 애니메이션 이벤트에 의해 호출
    private void Begin_MoveForward()
    {
        StartCoroutine(MoveForward());
    }

    private float forwardMoveDistance = 5.0f;  // 앞으로 이동할 거리
    private float forwardMoveDuration = 0.5f;  // 이동하는 데 걸릴 시간
    private float bufferDistance = 0.5f; // 벽에서 살짝 떨어진 위치로 이동하기 위한 버퍼 거리

    private Vector3 playeroffset = new Vector3(0, 0.9f, 0);
    private IEnumerator MoveForward()
    {
        float elapsedTime = 0;
        Vector3 movementDirection = transform.forward.normalized;  // 이동 방향

        // Ray를 쏴서 충돌을 감지
        RaycastHit hit;
        float moveDistance = forwardMoveDistance; // 실제로 이동할 거리

        // 캐릭터 앞으로 Raycast를 쏴서 충돌 여부 확인
        if (Physics.Raycast(transform.position + playeroffset, movementDirection, out hit, forwardMoveDistance, 1 << 15))
        {
            // 충돌이 발생한 경우, 충돌 지점까지의 거리만큼 이동
            moveDistance = hit.distance - bufferDistance;
            if (moveDistance < 0)
            {
                moveDistance = 0;
            }
        }

        Vector3 startingPosition = rb.position;
        Vector3 targetPosition = rb.position + movementDirection * moveDistance;

        // 일정 시간 동안 부드럽게 이동
        while (elapsedTime < forwardMoveDuration)
        {
            Vector3 newPosition = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / forwardMoveDuration);
            transform.position = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;  // 다음 프레임으로 넘어감
        }

        // 이동이 끝난 후 마지막 위치 보정
        transform.position = targetPosition;
    }

    #endregion


    // Gizmos를 사용하여 CheckSphere를 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 rayStart = transform.position + playeroffset;
        Vector3 rayDirection = transform.forward.normalized * forwardMoveDistance;

        // Ray를 Gizmos로 그리기
        Gizmos.DrawRay(rayStart, rayDirection);

        // Ray의 끝점을 표시하기 위해 구를 그림
        Gizmos.DrawSphere(rayStart + rayDirection, 0.1f);
    }
}
