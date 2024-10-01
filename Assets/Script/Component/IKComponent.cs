using TMPro;
using UnityEngine;

public class IKComponent : MonoBehaviour
{
    [SerializeField, Range(0, 1)]
    private float footOffset = 0.085f;

    [SerializeField]
    private LayerMask footLayerMask;
    [SerializeField, Range(0.9f, 2f)]
    private float raycastDistance = 1.5f;                // 0.9 이상이여함.
    [SerializeField, Range(0, 1)]
    private float defaultbodyOffset = 0.865f;                    // 몸체 offset
    private float bodyOffset;

    private Animator animator;
    private StateComponent state;
    private WeaponComponent weaponComponent;

    [Header("--Debug Text--")]
    public TextMeshProUGUI bodyposition;
    public TextMeshProUGUI leftfoot;
    public TextMeshProUGUI rightfoot;


    public TextMeshProUGUI leftFootCurveText;
    public TextMeshProUGUI rightFootCurveText;
    public TextMeshProUGUI leftFootRotationText;
    public TextMeshProUGUI rightFootRotationText;

    public bool bCheck = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.updateMode = AnimatorUpdateMode.Normal;

        weaponComponent = GetComponent<WeaponComponent>();
        state = GetComponent<StateComponent>();
    }

    private void Start()
    {
        // Enable Test용 
    }

    Vector3 targetBodyPosition;
    // 클래스 멤버로 IK 위치를 저장할 변수를 선언
    private Vector3 leftFootGizmoPosition;
    private Vector3 rightFootGizmoPosition;

    private void OnAnimatorIK(int layerIndex)
    {
        if (weaponComponent == null)
            return;

        if (state.IdleMode == false)
            return;

        SetWeaponBodyOffset();                      // 무기마다 다른 bodyOffset 값 셋팅

        float leftFootPositionWeight = animator.GetFloat("LeftFootPosition");
        float rightFootPositionWeight = animator.GetFloat("RightFootPosition");
        float leftFootRotationWeight = animator.GetFloat("LeftFootRotation");
        float rightFootRotationWeight = animator.GetFloat("RightFootRotation");


        // 텍스트 출력 코드
        leftFootCurveText.text = leftFootPositionWeight.ToString();
        rightFootCurveText.text = rightFootPositionWeight.ToString();
        leftFootRotationText.text = leftFootRotationWeight.ToString();
        rightFootRotationText.text = rightFootRotationWeight.ToString();

        RaycastHit leftFootHit, rightFootHit;
        GetFootHeight(AvatarIKGoal.LeftFoot, out leftFootHit);
        // 왼발 IK 적용
        if (leftFootHit.collider != null)
        {
            Vector3 leftFootPosition = leftFootHit.point;
            leftFootPosition.y += footOffset; // 지면에 붙지 않도록 오프셋 추가

            // Lerp 적용을 위한 현재 발 위치를 이전에 저장된 값으로 사용
            Vector3 currentLeftFootPosition = leftFootGizmoPosition;
            Vector3 smoothLeftFootPosition = Vector3.Lerp(currentLeftFootPosition, leftFootPosition, 0.5f);
            leftFootGizmoPosition = smoothLeftFootPosition;

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, smoothLeftFootPosition);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootPositionWeight);

            if (leftfoot != null)
            {
                leftfoot.text = smoothLeftFootPosition.y.ToString("F3");
            }

            if(bCheck)
            {
                // 발의 회전을 법선 벡터에 맞춤
                Quaternion leftFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, leftFootHit.normal), leftFootHit.normal);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotationWeight);
            }
        }

        GetFootHeight(AvatarIKGoal.RightFoot, out rightFootHit);
        // 오른발 IK 적용
        if (rightFootHit.collider != null)
        {
            Vector3 rightFootPosition = rightFootHit.point;
            rightFootPosition.y += footOffset; // 지면에 붙지 않도록 오프셋 추가

            // 현재 오른발의 IK 위치 가져오기
            Vector3 currentRightFootPosition = rightFootGizmoPosition;

            // 부드럽게 오른발 위치 보정 (Lerp)
            Vector3 smoothRightFootPosition = Vector3.Lerp(currentRightFootPosition, rightFootPosition, 0.5f);
            rightFootGizmoPosition = smoothRightFootPosition;  // 오른발 위치를 저장

            animator.SetIKPosition(AvatarIKGoal.RightFoot, smoothRightFootPosition);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootPositionWeight);

            if (rightfoot != null)
            {
                rightfoot.text = smoothRightFootPosition.y.ToString("F3");
            }

            if(bCheck)
            {
                // 발의 회전을 법선 벡터에 맞춤
                Quaternion rightFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, rightFootHit.normal), rightFootHit.normal);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootRotationWeight);
            }
        }


        // 두 발중 낮은 값의 발 y값 사용
        float lowerFootHeight = Mathf.Min(leftFootGizmoPosition.y, rightFootGizmoPosition.y);
        
        float targetBodyY = lowerFootHeight + bodyOffset;

        // x,y,z 위치 가져오고 y값 버림.
        targetBodyPosition = animator.bodyPosition;
        targetBodyPosition.y = targetBodyY;

        animator.bodyPosition = targetBodyPosition;

        if (bodyposition != null)
        {
            bodyposition.text = targetBodyPosition.y.ToString("F3");
        }
    }

    private float GetFootHeight(AvatarIKGoal foot, out RaycastHit hit)
    {
        Vector3 footPosition = animator.GetIKPosition(foot);

        Debug.DrawRay(footPosition + Vector3.up * 0.9f, Vector3.down * raycastDistance, Color.cyan);
        if (Physics.Raycast(footPosition + Vector3.up * 0.9f, Vector3.down, out hit, raycastDistance, footLayerMask))
        {
            return hit.point.y;
        }


        return footPosition.y;
    }

    // 무기마다 다른 bodyOffset 값 셋팅
    private void SetWeaponBodyOffset()
    {
        switch(weaponComponent.Type)
        {
            case WeaponType.Hammer:
                bodyOffset = 0.7f;
                break;

            default:
                bodyOffset = defaultbodyOffset;
                break;
        }
    }
}
