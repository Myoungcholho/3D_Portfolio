using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using StateType = StateComponent.StateType;

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

    // IK용 변수
    public float checkSphereRadius = 0.1f; // CheckSphere의 반경
    public LayerMask groundLayers;
    public float jumpForce = 5.0f;


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
        Vector3 teleportPos = transform.position;

        switch (direction)
        {
            case EvadeDirection.Forward:
                {
                  //  transform.Translate(new Vector3(0, 0, 5f));
                    Vector3 distance = transform.position + new Vector3(0, 0, teleportDistance);
                    StartCoroutine(MoveToPosition(distance));
                }
            break;
            case EvadeDirection.Backward:
                {
                    //transform.Translate(new Vector3(0, 0, -5f));
                    Vector3 distance = transform.position + new Vector3(0, 0, -teleportDistance);
                    StartCoroutine(MoveToPosition(distance));
                }
            break;
            case EvadeDirection.Left:
                {
                   // transform.Translate(new Vector3(-5f, 0, 0));
                    Vector3 distance = transform.position + new Vector3(-teleportDistance, 0, 0);
                    StartCoroutine(MoveToPosition(distance));

                }
            break;
            case EvadeDirection.Right:
                {
                    //transform.Translate(new Vector3(5f, 0, 0));
                    Vector3 distance = transform.position + new Vector3(teleportDistance, 0, 0);
                    StartCoroutine(MoveToPosition(distance));
                }
                break;
            case EvadeDirection.ForwardLeft:
                {
                    //transform.Translate(new Vector3(5f, 0, 0));
                    Vector3 distance = transform.position + new Vector3(-teleportDistance, 0, teleportDistance);
                    StartCoroutine(MoveToPosition(distance));
                }
                break;
            case EvadeDirection.ForwardRight:
                {
                    //transform.Translate(new Vector3(5f, 0, 0));
                    Vector3 distance = transform.position + new Vector3(teleportDistance, 0, teleportDistance);
                    StartCoroutine(MoveToPosition(distance));
                }
                break;
            case EvadeDirection.BackwardLeft:
                {
                    //transform.Translate(new Vector3(5f, 0, 0));
                    Vector3 distance = transform.position + new Vector3(-teleportDistance, 0, -teleportDistance);
                    StartCoroutine(MoveToPosition(distance));
                }
                break;
            case EvadeDirection.BackwardRight:
                {
                    //transform.Translate(new Vector3(5f, 0, 0));
                    Vector3 distance = transform.position + new Vector3(teleportDistance, 0, -teleportDistance);
                    StartCoroutine(MoveToPosition(distance));
                }
                break;
        }

        Debug.Assert(StaffParticlePrefab != null);
        if(StaffParticlePrefab != null)
        {
            Instantiate<GameObject>(StaffParticlePrefab, teleportPos,transform.rotation);
        }
    }
    private IEnumerator MoveToPosition(Vector3 target)
    {
        renderer = GetComponentsInChildren<Renderer>();
        foreach (Renderer rd in renderer)
        {
            rd.enabled = false;
        }

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            // 현재 위치에서 목표 위치로 매 프레임마다 이동
            transform.position = Vector3.MoveTowards(transform.position, target, 10f * Time.deltaTime);
            yield return null; // 다음 프레임까지 대기
        }

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

    bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position, checkSphereRadius, groundLayers);
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

    // Gizmos를 사용하여 CheckSphere를 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkSphereRadius);
    }
}
