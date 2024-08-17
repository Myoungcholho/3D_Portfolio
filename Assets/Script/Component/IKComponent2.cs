using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKComponent2 : MonoBehaviour
{
    private Animator anim;

    // 발의 위치 및 IK 포지션과 회전 값을 저장하기 위한 변수들
    private Vector3 rightFootPosition, leftFootPosition, leftFootIKPosition, rightFootIKPosition;
    private Quaternion leftFootIKRotation, rightFootIKRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

    [Header("Feet Grounder")]
    public bool enableFeetIK = true; // 발 IK 기능 활성화 여부

    [Range(0, 2)]
    [SerializeField]
    private float heightFromGroundRaycast = 1.14f; // 지면으로부터 발 위치를 계산할 때 사용하는 레이캐스트의 높이

    [Range(0, 2)]
    [SerializeField]
    private float raycastDownDistance = 1.5f; // 레이캐스트의 아래 방향 거리

    [SerializeField]
    private LayerMask environmentLayer; // 환경 레이어를 설정하여 발의 위치를 계산할 때 사용할 레이캐스트에 적용

    [SerializeField]
    private float pelvisOffset = 0f; // 골반 위치의 오프셋 값

    [Range(0, 1)]
    [SerializeField]
    private float pelvisUpAndDownSpeed = 0.28f; // 골반이 위아래로 움직이는 속도

    [Range(0, 1)]
    [SerializeField]
    private float feetToIKPositionSpeed = 0.5f; // 발이 IK 위치로 이동하는 속도

    public string leftFootAnimVariableName = "LeftFootCurve"; // 왼발 애니메이션 변수 이름
    public string rightFootAnimVariableName = "RightFootCurve"; // 오른발 애니메이션 변수 이름

    public bool useProIKFeature = false; // 고급 IK 기능 사용 여부
    public bool showSolverDebug = true; // 디버그 정보를 표시할지 여부

    private void Awake()
    {
        anim = GetComponent<Animator>(); // Animator 컴포넌트 가져오기
    }

    /// <summary>
    /// AdjustFeetTarget 메서드를 업데이트하고 각 발의 위치를 Solver Position 내에서 찾음
    /// </summary>
    private void FixedUpdate()
    {
        if (enableFeetIK == false)
            return;
        if (anim == null)
            return;

        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot); // 오른발의 목표 위치 조정
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot); // 왼발의 목표 위치 조정

        // 레이캐스트를 사용해 지면의 위치를 찾고 발의 위치를 계산
        FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
        FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (enableFeetIK == false)
            return;
        if (anim == null)
            return;

        MovePelvisHeight(); // 골반의 높이를 조정

        // 오른발의 IK 위치와 회전 설정
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        if (useProIKFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName)); // 고급 IK 기능 사용 시 회전 가중치 설정
        }

        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY); // 오른발을 IK 위치로 이동

        // 왼발의 IK 위치와 회전 설정
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        if (useProIKFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName)); // 고급 IK 기능 사용 시 회전 가중치 설정
        }

        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY); // 왼발을 IK 위치로 이동

    }

    // 발을 IK 포인트로 이동시키는 메서드
    private void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    {
        Vector3 targetIKPosition = anim.GetIKPosition(foot);

        if (positionIKHolder != Vector3.zero)
        {
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition); // 로컬 좌표로 변환
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed); // Y축 위치 보간
            targetIKPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIKPosition = transform.TransformPoint(targetIKPosition); // 월드 좌표로 변환

            anim.SetIKPosition(foot, targetIKPosition); // IK 위치 설정
        }
    }

    // 골반의 높이를 조정하는 메서드
    private void MovePelvisHeight()
    {
        if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = anim.bodyPosition.y; // 마지막 골반 위치 저장
            return;
        }

        float lOffsetPosition = leftFootIKPosition.y - transform.position.y; // 왼발의 오프셋 계산
        float rOffsetPosition = rightFootIKPosition.y - transform.position.y; // 오른발의 오프셋 계산

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition; // 더 작은 오프셋 선택

        Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset; // 새로운 골반 위치 계산

        newPelvisPosition.y = Mathf.Lerp(lastLeftFootPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed); // 골반 위치 보간

        anim.bodyPosition = newPelvisPosition; // 골반 위치 적용

        lastPelvisPositionY = anim.bodyPosition.y; // 마지막 골반 위치 업데이트
    }

    // 레이캐스트를 사용해 발의 위치를 찾고 회전을 계산하는 메서드
    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
    {
        RaycastHit feetOutHit;

        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow); // 디버그 라인 그리기

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
        {
            // 하늘 위치에서 발의 IK 위치 찾기
            feetIKPositions = fromSkyPosition;
            feetIKPositions.y = feetOutHit.point.y + pelvisOffset; // 발의 Y축 위치 설정
            feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation; // 발의 회전 설정

            return;
        }

        feetIKPositions = Vector3.zero; // IK 위치 초기화
    }

    // 발의 목표 위치를 조정하는 메서드
    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = anim.GetBoneTransform(foot).position; // 뼈의 위치 가져오기
        feetPositions.y = transform.position.y + heightFromGroundRaycast; // 발의 Y축 위치 조정
    }

    private void OnDrawGizmos()
    {
        // 디버그 정보나 기타 정보를 그리기 위해 사용될 수 있음 (현재는 비어 있음)
    }
}