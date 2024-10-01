using System.Collections;
using Tiny;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using StateType = StateComponent.StateType;
using DamageStateType = StateComponent.DamageStateType;

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
    [SerializeField]
    private float sensitivity = 100.0f;
    [SerializeField]
    private float deadZone = 0.001f;
    [SerializeField]
    private float teleportDistance = 5f;
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
        float speed = bRun ? runSpeed : walkSpeed;
        if (currInputMove.magnitude > deadZone)
        {
            direction = (Vector3.right * currInputMove.x) + (Vector3.forward * currInputMove.y);
            direction = direction.normalized * speed;
        }

        if (direction.z >= 0.0f)
        {
            if (direction.z >= 1.0f * runSpeed)
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

        animator.SetFloat("SpeedX", currInputMove.x * speed);
        animator.SetFloat("SpeedY", currInputMove.y * speed + yIncreaseTime);
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
        EvadeDirection direction = EvadeDirection.Forward;
        EvadeDirection staffDirection = EvadeDirection.Forward;

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
        EnableEvadeCollider(direction);

        if (animator.GetInteger("WeaponType") == (int)WeaponType.FireBall)
        {
            StaffEvade(staffDirection);
        }
    }

    // 스태프 Evade
    private void StaffEvade(EvadeDirection direction)
    {
        Vector3 moveDirection = Vector3.zero;
        Vector3 position = transform.position;

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
        Debug.DrawRay(position, moveDirection * teleportDistance, Color.green, 2f);

        if (Physics.Raycast(position, moveDirection, out hit, teleportDistance))
        {
            // 벽에 부딪힌 경우, 충돌 지점까지만 이동
            destination = hit.point;

            // 충돌 지점까지 Ray를 다른 색상으로 그리기
            Debug.DrawLine(position, hit.point, Color.red, 2f);
        }
        else
        {
            // 벽에 부딪히지 않는 경우, 지정된 거리만큼 이동
            destination = transform.position + moveDirection * teleportDistance;
        }

        // 목적지로 이동
        StartCoroutine(MoveToPosition(destination));

        // 텔포 시 파티클 생성
        Debug.Assert(StaffParticlePrefab != null);
        if (StaffParticlePrefab != null)
        {
            Instantiate<GameObject>(StaffParticlePrefab, transform.position, transform.rotation);
        }
    }
    private IEnumerator MoveToPosition(Vector3 target)
    {
        renderer = GetComponentsInChildren<Renderer>();
        foreach (Renderer rd in renderer)
        {
            rd.enabled = false;
        }

        float duration = 0.5f; // 0.5초 동안 이동
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        // Lerp를 사용해 1초 동안 이동
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 정확한 위치로 이동 (Lerp가 끝났을 때)
        transform.position = target;

        foreach (Renderer rd in renderer)
            rd.enabled = true;
        
        animator.SetTrigger("TestExit");
        Move();
        End_Evade();
    }
    // 스태프 Evade

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
