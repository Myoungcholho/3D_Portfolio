using System.Collections;
using System.Linq;
using UnityEngine;

public class TargetComponent : MonoBehaviour
{
    [Header("------Debug------")]
    public bool bDebug = true;
    public GameObject targetObject;

    [Header("------타겟팅 설정------")]
    [SerializeField]
    private float radius = 5.0f;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private GameObject targetPrefab;

    private GameObject targetPrefabObject;

    public void TargetSearch()
    {
        if (targetObject == null)
        {
            Begin_Targeting();
            StartLookAtTarget();

            return;
        }
    }

    private Coroutine lookAtCoroutine;
    private void StartLookAtTarget()
    {
        if (targetObject == null)
            return;

        if (lookAtCoroutine != null)
            StopCoroutine(lookAtCoroutine); // 이미 실행 중인 코루틴이 있으면 중지

        lookAtCoroutine = StartCoroutine(LookAtTarget());

        //Vector3 direction = targetObject.transform.position - transform.position;
        //transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private float rotationSpeed = 720f;               // 몸체 회전 속도, 1초에 720도만큼 회전
    // 타겟 목표까지 보간하여 회전
    private IEnumerator LookAtTarget()
    {
        if (targetObject == null)
            yield break;

        while (true)
        {
            Vector3 direction = targetObject.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            // 현재 회전과 목표 회전 사이에서 부드럽게 회전
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 목표 회전에 거의 도달하면 종료
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                CameraRotationMouseComponent component = GetComponent<CameraRotationMouseComponent>();

                // targetRotation에서 y, z 값 추출
                Vector3 targetEulerAngles = targetRotation.eulerAngles;

                // 현재 rotation에서 x 값 유지하고 y, z 값만 변경
                Vector3 newEulerAngles = new Vector3(component.rotation.eulerAngles.x, targetEulerAngles.y, targetEulerAngles.z);

                // 새로운 rotation 생성
                Quaternion newRotation = Quaternion.Euler(newEulerAngles);

                // 적용
                component.followTargetTransform.rotation = newRotation;
                component.rotation = newRotation;

                yield break;
            }

            yield return null; // 다음 프레임까지 대기
        }

        

    }


    /// <summary>
    /// 주변의 Collider를 얻고, 가장 가까운 적을 TargetObject로 설정합니다.
    /// </summary>
    private void Begin_Targeting()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);
        GameObject[] candidates = colliders.Select(collider => collider.gameObject).ToArray();

        GameObject nearlyObject = GetNealyFrontAngle(candidates);

        ChangeTarget(nearlyObject);
    }

    /// <summary>
    /// GameObject[]을 인자로 받아 가장 가까운 GameObject를 return.
    /// </summary>
    private GameObject GetNealyFrontAngle(GameObject[] candidates)
    {
        GameObject obj = null;
        Vector3 pos = transform.position;
        float minDistance = float.MaxValue;

        foreach (GameObject candidate in candidates)
        {
            Vector3 enemyPos = candidate.transform.position;
            Vector3 direction = enemyPos - pos;
            if (bDebug)
            {
                Debug.DrawLine(transform.position, transform.position+ direction, Color.red);
            }

            float distance = Vector3.Distance(transform.position, transform.position + direction);

            if (minDistance > distance)
            {
                minDistance = distance;
                obj = candidate;
            }
        }
        return obj;
    }

    /// <summary>
    /// targetObject를 인자로 받은 GameObject로 설정합니다.
    /// </summary>
    private void ChangeTarget(GameObject target)
    {
        if (target == null)
        {
            EndTargeting();
            return;
        }

        if (targetPrefab != null)
        {            
            targetPrefabObject = Instantiate(targetPrefab,target.transform);
            targetPrefabObject.transform.localPosition = new Vector3(0, 2.5f, 0);
        }

        EndTargeting();

        targetObject = target;
    }

    /// <summary>
    /// targetObject를 null로 변경합니다.
    /// </summary>
    public void EndTargeting()
    {
        if (targetObject != null)
        {
            if (targetPrefabObject != null)
                Destroy(targetPrefabObject.gameObject);
        }
        targetObject = null;
    }
}
