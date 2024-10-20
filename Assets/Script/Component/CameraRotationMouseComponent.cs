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

    // 이전 회전 값이 남아 타겟팅 후에 복원되는 문제로 public으로 변경해둠.
    public Transform followTargetTransform;

    private Vector2 inputLook;              // 마우스 입력 값을 받을 Vector2
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
        // 마우스 게임 종속
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
        // 축 기준 회전
        // b축을 기준으로 a로의 회전 값을 반환함.
        rotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up);
        rotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        followTargetTransform.rotation = rotation;
        // y축은 음수로, 아니면 거꾸로 되어서

        // 로컬 회전 기준
        Vector3 angles = followTargetTransform.localEulerAngles;
        angles.z = 0.0f; // 짐벌락 때매 막아둠, 2차원 회전만 하려고

        // 회전 각 제한
        float xAngle = followTargetTransform.localEulerAngles.x;

        // 180도면 짐벌락 걸릴 가능성있어서 배제
        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            angles.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            angles.x = limitPitchAngle.y;

        followTargetTransform.localEulerAngles = angles;

        // 부드러운 화면 전환을 위한 보정
        rotation = Quaternion.Lerp(followTargetTransform.rotation, rotation, mouseRotationLerp * Time.deltaTime);
        //Debug.Log("followTargetTransform : " + followTargetTransform.rotation.eulerAngles);
        //Debug.Log("rotation : " + rotation.eulerAngles);


        transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        followTargetTransform.localEulerAngles = new Vector3(angles.x, 0, 0);
        //캐릭터가 먼저 돌고 follow(자식)이돌아야한다.


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
