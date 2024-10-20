using System.Collections;
using System.Linq;
using UnityEngine;

public class TargetComponent : MonoBehaviour
{
    [Header("------Debug------")]
    public bool bDebug = true;
    public GameObject targetObject;

    [Header("------Ÿ���� ����------")]
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
            StopCoroutine(lookAtCoroutine); // �̹� ���� ���� �ڷ�ƾ�� ������ ����

        lookAtCoroutine = StartCoroutine(LookAtTarget());

        //Vector3 direction = targetObject.transform.position - transform.position;
        //transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private float rotationSpeed = 720f;               // ��ü ȸ�� �ӵ�, 1�ʿ� 720����ŭ ȸ��
    // Ÿ�� ��ǥ���� �����Ͽ� ȸ��
    private IEnumerator LookAtTarget()
    {
        if (targetObject == null)
            yield break;

        while (true)
        {
            Vector3 direction = targetObject.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            // ���� ȸ���� ��ǥ ȸ�� ���̿��� �ε巴�� ȸ��
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // ��ǥ ȸ���� ���� �����ϸ� ����
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                CameraRotationMouseComponent component = GetComponent<CameraRotationMouseComponent>();

                // targetRotation���� y, z �� ����
                Vector3 targetEulerAngles = targetRotation.eulerAngles;

                // ���� rotation���� x �� �����ϰ� y, z ���� ����
                Vector3 newEulerAngles = new Vector3(component.rotation.eulerAngles.x, targetEulerAngles.y, targetEulerAngles.z);

                // ���ο� rotation ����
                Quaternion newRotation = Quaternion.Euler(newEulerAngles);

                // ����
                component.followTargetTransform.rotation = newRotation;
                component.rotation = newRotation;

                yield break;
            }

            yield return null; // ���� �����ӱ��� ���
        }

        

    }


    /// <summary>
    /// �ֺ��� Collider�� ���, ���� ����� ���� TargetObject�� �����մϴ�.
    /// </summary>
    private void Begin_Targeting()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);
        GameObject[] candidates = colliders.Select(collider => collider.gameObject).ToArray();

        GameObject nearlyObject = GetNealyFrontAngle(candidates);

        ChangeTarget(nearlyObject);
    }

    /// <summary>
    /// GameObject[]�� ���ڷ� �޾� ���� ����� GameObject�� return.
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
    /// targetObject�� ���ڷ� ���� GameObject�� �����մϴ�.
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
    /// targetObject�� null�� �����մϴ�.
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
