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
    
    private float yIncreaseTime = 0.0f; // Y�� ���� 4�� ���� ���� �ð��� ����


    // Evade�� Ȱ��ȭ �� Collider
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

        //Evade�� ����Լ�
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


    //�÷��̾� ���°� ����Ǹ� ȣ���.
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

        // Ű �Է��� ���� ���
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

        // �� �Է� ��
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;
            staffDirection = direction;
            // ���� �밢��
            if (value.x < 0.0f)
            {
                staffDirection = EvadeDirection.ForwardLeft;
                
            }
            // ������ �밢��
            else if (value.x > 0.0f)
            {
                staffDirection = EvadeDirection.ForwardRight;
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
            staffDirection = direction;
            // ���� �� �밢��
            if (value.x < 0.0f)
            {
                staffDirection = EvadeDirection.BackwardLeft;                
            }
            // ������ �� �밢��
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

    // ������ Evade
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

        // Ray�� ���� �浹 ���� üũ
        RaycastHit hit;
        Vector3 destination;

        // Ȯ�ο� Ray
        Debug.DrawRay(position, moveDirection * teleportDistance, Color.green, 2f);

        if (Physics.Raycast(position, moveDirection, out hit, teleportDistance))
        {
            // ���� �ε��� ���, �浹 ���������� �̵�
            destination = hit.point;

            // �浹 �������� Ray�� �ٸ� �������� �׸���
            Debug.DrawLine(position, hit.point, Color.red, 2f);
        }
        else
        {
            // ���� �ε����� �ʴ� ���, ������ �Ÿ���ŭ �̵�
            destination = transform.position + moveDirection * teleportDistance;
        }

        // �������� �̵�
        StartCoroutine(MoveToPosition(destination));

        // ���� �� ��ƼŬ ����
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

        float duration = 0.5f; // 0.5�� ���� �̵�
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        // Lerp�� ����� 1�� ���� �̵�
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // ��Ȯ�� ��ġ�� �̵� (Lerp�� ������ ��)
        transform.position = target;

        foreach (Renderer rd in renderer)
            rd.enabled = true;
        
        animator.SetTrigger("TestExit");
        Move();
        End_Evade();
    }
    // ������ Evade

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
    /// ������ ������ Idle���� ����
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


    // ����� ������ ��ġ�ϱ�
    private float moveSpeed = 5f;
    // target : ��ǥ�� Ÿ������ �̵�
    // stoppingDistacne : ��Ȯ�� ������ �ƴ� �ձ��� �̵��ϱ� ���� ���̰�
    public IEnumerator MoveToTarget(Transform target, float stoppingDistance)
    {
        // ��ǥ ��ġ�� ������ ������ �ݺ�
        while (Vector3.Distance(transform.position, target.position) > stoppingDistance)
        {
            // ����� ��ġ������ ���� ���� ���
            Vector3 direction = (target.position - transform.position).normalized;

            // ���� ��ġ���� ��� ��ġ�� �̵� (MoveTowards ���)
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            // �������� ���� ������ ���
            yield return null;
        }
    }

    #region FistQSkillDash

    // Fist Q ��ų �� ������ ��¦ �ٰ� �ϱ� ���� �̺�Ʈ �޼���
    // �ִϸ��̼� �̺�Ʈ�� ���� ȣ��
    private void Begin_MoveForward()
    {
        StartCoroutine(MoveForward());
    }

    private float forwardMoveDistance = 5.0f;  // ������ �̵��� �Ÿ�
    private float forwardMoveDuration = 0.5f;  // �̵��ϴ� �� �ɸ� �ð�
    private float bufferDistance = 0.5f; // ������ ��¦ ������ ��ġ�� �̵��ϱ� ���� ���� �Ÿ�

    private Vector3 playeroffset = new Vector3(0, 0.9f, 0);
    private IEnumerator MoveForward()
    {
        float elapsedTime = 0;
        Vector3 movementDirection = transform.forward.normalized;  // �̵� ����

        // Ray�� ���� �浹�� ����
        RaycastHit hit;
        float moveDistance = forwardMoveDistance; // ������ �̵��� �Ÿ�

        // ĳ���� ������ Raycast�� ���� �浹 ���� Ȯ��
        if (Physics.Raycast(transform.position + playeroffset, movementDirection, out hit, forwardMoveDistance, 1 << 15))
        {
            // �浹�� �߻��� ���, �浹 ���������� �Ÿ���ŭ �̵�
            moveDistance = hit.distance - bufferDistance;
            if (moveDistance < 0)
            {
                moveDistance = 0;
            }
        }

        Vector3 startingPosition = rb.position;
        Vector3 targetPosition = rb.position + movementDirection * moveDistance;

        // ���� �ð� ���� �ε巴�� �̵�
        while (elapsedTime < forwardMoveDuration)
        {
            Vector3 newPosition = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / forwardMoveDuration);
            transform.position = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;  // ���� ���������� �Ѿ
        }

        // �̵��� ���� �� ������ ��ġ ����
        transform.position = targetPosition;
    }

    #endregion




    // Gizmos�� ����Ͽ� CheckSphere�� �ð�ȭ
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 rayStart = transform.position + playeroffset;
        Vector3 rayDirection = transform.forward.normalized * forwardMoveDistance;

        // Ray�� Gizmos�� �׸���
        Gizmos.DrawRay(rayStart, rayDirection);

        // Ray�� ������ ǥ���ϱ� ���� ���� �׸�
        Gizmos.DrawSphere(rayStart + rayDirection, 0.1f);
    }
}
