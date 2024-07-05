using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using StateType = StateComponent.StateType;

public enum EvadeDirection
{
    Forward, Backward, Left, Right,
}

public class PlayerMovingComponent : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 2.0f;
    [SerializeField]
    private float runSpeed = 4.0f;
    [SerializeField]
    private float sensitivity = 10.0f;
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
    private Quaternion? evadeRotation = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        weapon = GetComponent<WeaponComponent>();
        state = GetComponent<StateComponent>();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction moveAction = actionMap.FindAction("Move");
        moveAction.performed += Input_Move_Performed;
        moveAction.canceled += Input_Move_Cancled;

        InputAction runAction = actionMap.FindAction("Run");
        runAction.started += Input_Run_Started;
        runAction.canceled += Input_Run_Cancled;

        state.OnStateTypeChanged += OnStateTypeChanged;
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
    Vector3 newPosition;
    private void Update()
    {
        currInputMove = Vector2.SmoothDamp(currInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        if (bCanMove == false)
            return;

        Vector3 direction = Vector3.zero;

        float speed = bRun ? runSpeed : walkSpeed;
        if (currInputMove.magnitude > deadZone)
        {
            direction = (Vector3.right * currInputMove.x) + (Vector3.forward * currInputMove.y);
            direction = direction.normalized * speed;
        }

        transform.Translate(direction * Time.deltaTime);
        
        if (weapon.UnarmedMode)
        {
            animator.SetFloat("SpeedY", direction.magnitude);

            return;
        }

        animator.SetFloat("SpeedX", currInputMove.x * speed);
        animator.SetFloat("SpeedY", currInputMove.y * speed);
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



        // Ű �Է��� ���� ���
        if (value.y == 0.0f)
        {
            direction = EvadeDirection.Forward;

            if (value.x < 0.0f)
            {
                direction = EvadeDirection.Left;
            }
            else if (value.x > 0.0f)
            {
                direction = EvadeDirection.Right;
            }
        }
        // �� �Է� ��
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;
            if (value.x < 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
            else if (value.x > 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
            if (value.x < 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
            else if (value.x > 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f);
            }
        }
        animator.SetInteger("Direction", (int)direction);
        animator.SetTrigger("Evade");

        if (animator.GetInteger("WeaponType") == (int)WeaponType.FireBall)
        {
            StaffEvade(direction);
        }

    }

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
        {
            rd.enabled = true;
        }
        animator.SetTrigger("TestExit");
        Move();
        End_Evade();
    }

    /// <summary>
    /// ������ ������ Idle���� �����ϰ�, ȸ���� ���� ���� ���·� ȸ���մϴ�.
    /// </summary>
    public void End_Evade()
    {
        if (evadeRotation.HasValue)
            StartCoroutine(Reset_EvadeRotation());

        state.SetIdleMode();
    }

    private IEnumerator Reset_EvadeRotation()
    {
        float delta = 0.0f;

        while (true)
        {
            float angle = Quaternion.Angle(transform.rotation, evadeRotation.Value);
            if (angle < 2.0f)
                break;

            delta += Time.deltaTime * 50f;
            Quaternion rotate = Quaternion.RotateTowards(transform.rotation, evadeRotation.Value, delta);
            transform.rotation = rotate;

            yield return new WaitForFixedUpdate();
        }

        transform.rotation = evadeRotation.Value;
    }

    private void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.Label("inputMove" + inputMove.ToString());
        GUILayout.Label("newPosition" + newPosition.ToString());
        

    }
}