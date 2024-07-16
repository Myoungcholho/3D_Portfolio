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
            LookAtTarget();

            return;
        }
    }

    private void LookAtTarget()
    {
        if (targetObject == null)
            return;

        Vector3 direction = targetObject.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
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
