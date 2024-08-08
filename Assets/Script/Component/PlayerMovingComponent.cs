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
    
    private float yIncreaseTime = 0.0f; // Y�� ���� 4�� ���� ���� �ð��� ����

    // IK�� ����
    public float checkSphereRadius = 0.1f; // CheckSphere�� �ݰ�
    public LayerMask groundLayers;
    public float jumpForce = 5.0f;


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
            // ���� ��ġ���� ��ǥ ��ġ�� �� �����Ӹ��� �̵�
            transform.position = Vector3.MoveTowards(transform.position, target, 10f * Time.deltaTime);
            yield return null; // ���� �����ӱ��� ���
        }

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

    bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position, checkSphereRadius, groundLayers);
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

    // Gizmos�� ����Ͽ� CheckSphere�� �ð�ȭ
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkSphereRadius);
    }
}
