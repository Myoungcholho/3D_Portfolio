using TMPro;
using UnityEngine;

public class IKComponent : MonoBehaviour
{
    [SerializeField, Range(0, 1)]
    private float footOffset = 0.085f;

    [SerializeField]
    private LayerMask footLayerMask;
    [SerializeField, Range(0.9f, 2f)]
    private float raycastDistance = 1.5f;                // 0.9 �̻��̿���.
    [SerializeField, Range(0, 1)]
    private float defaultbodyOffset = 0.865f;                    // ��ü offset
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
        // Enable Test�� 
    }

    Vector3 targetBodyPosition;
    // Ŭ���� ����� IK ��ġ�� ������ ������ ����
    private Vector3 leftFootGizmoPosition;
    private Vector3 rightFootGizmoPosition;

    private void OnAnimatorIK(int layerIndex)
    {
        if (weaponComponent == null)
            return;

        if (state.IdleMode == false)
            return;

        SetWeaponBodyOffset();                      // ���⸶�� �ٸ� bodyOffset �� ����

        float leftFootPositionWeight = animator.GetFloat("LeftFootPosition");
        float rightFootPositionWeight = animator.GetFloat("RightFootPosition");
        float leftFootRotationWeight = animator.GetFloat("LeftFootRotation");
        float rightFootRotationWeight = animator.GetFloat("RightFootRotation");


        // �ؽ�Ʈ ��� �ڵ�
        leftFootCurveText.text = leftFootPositionWeight.ToString();
        rightFootCurveText.text = rightFootPositionWeight.ToString();
        leftFootRotationText.text = leftFootRotationWeight.ToString();
        rightFootRotationText.text = rightFootRotationWeight.ToString();

        RaycastHit leftFootHit, rightFootHit;
        GetFootHeight(AvatarIKGoal.LeftFoot, out leftFootHit);
        // �޹� IK ����
        if (leftFootHit.collider != null)
        {
            Vector3 leftFootPosition = leftFootHit.point;
            leftFootPosition.y += footOffset; // ���鿡 ���� �ʵ��� ������ �߰�

            // Lerp ������ ���� ���� �� ��ġ�� ������ ����� ������ ���
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
                // ���� ȸ���� ���� ���Ϳ� ����
                Quaternion leftFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, leftFootHit.normal), leftFootHit.normal);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotationWeight);
            }
        }

        GetFootHeight(AvatarIKGoal.RightFoot, out rightFootHit);
        // ������ IK ����
        if (rightFootHit.collider != null)
        {
            Vector3 rightFootPosition = rightFootHit.point;
            rightFootPosition.y += footOffset; // ���鿡 ���� �ʵ��� ������ �߰�

            // ���� �������� IK ��ġ ��������
            Vector3 currentRightFootPosition = rightFootGizmoPosition;

            // �ε巴�� ������ ��ġ ���� (Lerp)
            Vector3 smoothRightFootPosition = Vector3.Lerp(currentRightFootPosition, rightFootPosition, 0.5f);
            rightFootGizmoPosition = smoothRightFootPosition;  // ������ ��ġ�� ����

            animator.SetIKPosition(AvatarIKGoal.RightFoot, smoothRightFootPosition);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootPositionWeight);

            if (rightfoot != null)
            {
                rightfoot.text = smoothRightFootPosition.y.ToString("F3");
            }

            if(bCheck)
            {
                // ���� ȸ���� ���� ���Ϳ� ����
                Quaternion rightFootRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, rightFootHit.normal), rightFootHit.normal);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootRotationWeight);
            }
        }


        // �� ���� ���� ���� �� y�� ���
        float lowerFootHeight = Mathf.Min(leftFootGizmoPosition.y, rightFootGizmoPosition.y);
        
        float targetBodyY = lowerFootHeight + bodyOffset;

        // x,y,z ��ġ �������� y�� ����.
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

    // ���⸶�� �ٸ� bodyOffset �� ����
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
