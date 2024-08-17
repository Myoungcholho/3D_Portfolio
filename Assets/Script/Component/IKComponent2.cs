using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKComponent2 : MonoBehaviour
{
    private Animator anim;

    // ���� ��ġ �� IK �����ǰ� ȸ�� ���� �����ϱ� ���� ������
    private Vector3 rightFootPosition, leftFootPosition, leftFootIKPosition, rightFootIKPosition;
    private Quaternion leftFootIKRotation, rightFootIKRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

    [Header("Feet Grounder")]
    public bool enableFeetIK = true; // �� IK ��� Ȱ��ȭ ����

    [Range(0, 2)]
    [SerializeField]
    private float heightFromGroundRaycast = 1.14f; // �������κ��� �� ��ġ�� ����� �� ����ϴ� ����ĳ��Ʈ�� ����

    [Range(0, 2)]
    [SerializeField]
    private float raycastDownDistance = 1.5f; // ����ĳ��Ʈ�� �Ʒ� ���� �Ÿ�

    [SerializeField]
    private LayerMask environmentLayer; // ȯ�� ���̾ �����Ͽ� ���� ��ġ�� ����� �� ����� ����ĳ��Ʈ�� ����

    [SerializeField]
    private float pelvisOffset = 0f; // ��� ��ġ�� ������ ��

    [Range(0, 1)]
    [SerializeField]
    private float pelvisUpAndDownSpeed = 0.28f; // ����� ���Ʒ��� �����̴� �ӵ�

    [Range(0, 1)]
    [SerializeField]
    private float feetToIKPositionSpeed = 0.5f; // ���� IK ��ġ�� �̵��ϴ� �ӵ�

    public string leftFootAnimVariableName = "LeftFootCurve"; // �޹� �ִϸ��̼� ���� �̸�
    public string rightFootAnimVariableName = "RightFootCurve"; // ������ �ִϸ��̼� ���� �̸�

    public bool useProIKFeature = false; // ��� IK ��� ��� ����
    public bool showSolverDebug = true; // ����� ������ ǥ������ ����

    private void Awake()
    {
        anim = GetComponent<Animator>(); // Animator ������Ʈ ��������
    }

    /// <summary>
    /// AdjustFeetTarget �޼��带 ������Ʈ�ϰ� �� ���� ��ġ�� Solver Position ������ ã��
    /// </summary>
    private void FixedUpdate()
    {
        if (enableFeetIK == false)
            return;
        if (anim == null)
            return;

        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot); // �������� ��ǥ ��ġ ����
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot); // �޹��� ��ǥ ��ġ ����

        // ����ĳ��Ʈ�� ����� ������ ��ġ�� ã�� ���� ��ġ�� ���
        FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
        FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (enableFeetIK == false)
            return;
        if (anim == null)
            return;

        MovePelvisHeight(); // ����� ���̸� ����

        // �������� IK ��ġ�� ȸ�� ����
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        if (useProIKFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName)); // ��� IK ��� ��� �� ȸ�� ����ġ ����
        }

        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY); // �������� IK ��ġ�� �̵�

        // �޹��� IK ��ġ�� ȸ�� ����
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        if (useProIKFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName)); // ��� IK ��� ��� �� ȸ�� ����ġ ����
        }

        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY); // �޹��� IK ��ġ�� �̵�

    }

    // ���� IK ����Ʈ�� �̵���Ű�� �޼���
    private void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    {
        Vector3 targetIKPosition = anim.GetIKPosition(foot);

        if (positionIKHolder != Vector3.zero)
        {
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition); // ���� ��ǥ�� ��ȯ
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed); // Y�� ��ġ ����
            targetIKPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIKPosition = transform.TransformPoint(targetIKPosition); // ���� ��ǥ�� ��ȯ

            anim.SetIKPosition(foot, targetIKPosition); // IK ��ġ ����
        }
    }

    // ����� ���̸� �����ϴ� �޼���
    private void MovePelvisHeight()
    {
        if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = anim.bodyPosition.y; // ������ ��� ��ġ ����
            return;
        }

        float lOffsetPosition = leftFootIKPosition.y - transform.position.y; // �޹��� ������ ���
        float rOffsetPosition = rightFootIKPosition.y - transform.position.y; // �������� ������ ���

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition; // �� ���� ������ ����

        Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset; // ���ο� ��� ��ġ ���

        newPelvisPosition.y = Mathf.Lerp(lastLeftFootPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed); // ��� ��ġ ����

        anim.bodyPosition = newPelvisPosition; // ��� ��ġ ����

        lastPelvisPositionY = anim.bodyPosition.y; // ������ ��� ��ġ ������Ʈ
    }

    // ����ĳ��Ʈ�� ����� ���� ��ġ�� ã�� ȸ���� ����ϴ� �޼���
    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
    {
        RaycastHit feetOutHit;

        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow); // ����� ���� �׸���

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
        {
            // �ϴ� ��ġ���� ���� IK ��ġ ã��
            feetIKPositions = fromSkyPosition;
            feetIKPositions.y = feetOutHit.point.y + pelvisOffset; // ���� Y�� ��ġ ����
            feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation; // ���� ȸ�� ����

            return;
        }

        feetIKPositions = Vector3.zero; // IK ��ġ �ʱ�ȭ
    }

    // ���� ��ǥ ��ġ�� �����ϴ� �޼���
    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = anim.GetBoneTransform(foot).position; // ���� ��ġ ��������
        feetPositions.y = transform.position.y + heightFromGroundRaycast; // ���� Y�� ��ġ ����
    }

    private void OnDrawGizmos()
    {
        // ����� ������ ��Ÿ ������ �׸��� ���� ���� �� ���� (����� ��� ����)
    }
}