using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.FilePathAttribute;

public class CameraRotationMouseComponent : MonoBehaviour
{
    [SerializeField]
    private bool IsMouseRotation = true;
    [SerializeField]
    private string followTargetName = "FollowTarget";
    [SerializeField]
    private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f);
    [SerializeField]
    private Vector2 limitPitchAngle = new Vector2(20, 340);
    [SerializeField]
    private float mouseRotationLerp = 0.25f;

    // ���� ȸ�� ���� ���� Ÿ���� �Ŀ� �����Ǵ� ������ public���� �����ص�.
    public Transform followTargetTransform;

    private Vector2 inputLook;              // ���콺 �Է� ���� ���� Vector2
    public Quaternion rotation;
    private StateComponent state;

    private void Awake()
    {
        state = GetComponent<StateComponent>();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        InputAction lookAction = actionMap.FindAction("Look");
        lookAction.performed += Input_Look_Performed;
        lookAction.canceled += Input_Look_Cancled;
    }


    // Start is called before the first frame update
    void Start()
    {
        // ���콺 ���� ����
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        followTargetTransform = transform.FindChildByName(followTargetName);
    }

    // Update is called once per frame
    void Update()
    {
        bool bCheck = false;
        bCheck |= state.EvadeMode == true;
        bCheck |= state.DodgedMode == true;
        bCheck |= state.DodgedAttackMode == true;
        bCheck |= state.InstantKillMode == true;
        bCheck |= state.UsingSkillMode == true;
        bCheck |= state.ActionMode == true;

        if (bCheck)
            return;

        if (!IsMouseRotation)
            return;
        // �� ���� ȸ��
        // b���� �������� a���� ȸ�� ���� ��ȯ��.
        rotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up);
        rotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        followTargetTransform.rotation = rotation;
        // y���� ������, �ƴϸ� �Ųٷ� �Ǿ

        // ���� ȸ�� ����
        Vector3 angles = followTargetTransform.localEulerAngles;
        angles.z = 0.0f; // ������ ���� ���Ƶ�, 2���� ȸ���� �Ϸ���

        // ȸ�� �� ����
        float xAngle = followTargetTransform.localEulerAngles.x;

        // 180���� ������ �ɸ� ���ɼ��־ ����
        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            angles.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            angles.x = limitPitchAngle.y;

        followTargetTransform.localEulerAngles = angles;

        // �ε巯�� ȭ�� ��ȯ�� ���� ����
        rotation = Quaternion.Lerp(followTargetTransform.rotation, rotation, mouseRotationLerp * Time.deltaTime);
        //Debug.Log("followTargetTransform : " + followTargetTransform.rotation.eulerAngles);
        //Debug.Log("rotation : " + rotation.eulerAngles);


        transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        followTargetTransform.localEulerAngles = new Vector3(angles.x, 0, 0);
        //ĳ���Ͱ� ���� ���� follow(�ڽ�)�̵��ƾ��Ѵ�.


    }
    private void Input_Look_Performed(InputAction.CallbackContext context)
    {
        inputLook = context.ReadValue<Vector2>();
    }

    private void Input_Look_Cancled(InputAction.CallbackContext context)
    {
        inputLook = Vector2.zero;
    }
}
