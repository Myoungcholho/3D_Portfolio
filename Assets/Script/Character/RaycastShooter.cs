using UnityEngine;

public class RaycastShooter : MonoBehaviour
{
    // Ray�� �� �Ÿ� ����
    [SerializeField] private float rayDistance = 0.5f;
    [SerializeField] private Vector3 rayOffset = Vector3.zero;

    // ��,��,��,�� RayHit ������ ������ ����
    public RaycastHit hitForward;
    public RaycastHit hitBackward;
    public RaycastHit hitLeft;
    public RaycastHit hitRight;

    // Ray�� ������ Layer ����
    [SerializeField] private LayerMask layerMaskRay;

    // �� ������ RayHit ������ ������ ����
    [SerializeField] private float sphereRadius = 1f;
    [SerializeField] private Vector3 sphereRayOffset = Vector3.zero;
    public Collider sphereHitCollider;

    [SerializeField] private LayerMask layerMaskSphere;
    void Update()
    {
        // �����¿�� Ray ���, ���̴� rayDistance ������ŭ, Layer�� layerMask��
        ShootRaysUpdate();

        // �� ���·� Ray �����ϱ�
        DetectSphereUpdate();
    }

    void ShootRaysUpdate()
    {
        // ��
        if (Physics.Raycast(transform.position + rayOffset, transform.forward, out hitForward, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit forward: " + hitForward.collider.name);
        }
        else
        {
            // ��Ʈ���� ���� ���, hitUp�� collider�� null�� ����
            hitForward = new RaycastHit();  // hitUp�� �ʱ�ȭ�Ͽ� ���� �� ����
            Debug.Log("No Hit. forward");
        }

        // ��
        if (Physics.Raycast(transform.position + rayOffset, -transform.forward, out hitBackward, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit backward: " + hitBackward.collider.name);
        }
        else
        {
            // ��Ʈ���� ���� ���, hitUp�� collider�� null�� ����
            hitForward = new RaycastHit();  // hitUp�� �ʱ�ȭ�Ͽ� ���� �� ����
            Debug.Log("No Hit. down");
        }

        // ��
        if (Physics.Raycast(transform.position + rayOffset, -transform.right, out hitLeft, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit Left: " + hitLeft.collider.name);
        }
        else
        {
            // ��Ʈ���� ���� ���, hitUp�� collider�� null�� ����
            hitForward = new RaycastHit();  // hitUp�� �ʱ�ȭ�Ͽ� ���� �� ����
            Debug.Log("No Hit. left");
        }

        // ��
        if (Physics.Raycast(transform.position + rayOffset, transform.right, out hitRight, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit Right: " + hitRight.collider.name);
        }
        else
        {
            // ��Ʈ���� ���� ���, hitUp�� collider�� null�� ����
            hitForward = new RaycastHit();  // hitUp�� �ʱ�ȭ�Ͽ� ���� �� ����
            Debug.Log("No Hit. right");
        }
    }

    void DetectSphereUpdate()
    {
        // �� ���·� Ray �����ϱ�
        Collider[] colliders = Physics.OverlapSphere(transform.position + sphereRayOffset, sphereRadius, layerMaskSphere);
        if(colliders.Length > 0 ) 
        {
            sphereHitCollider = colliders[0];
            Debug.Log("Hit Shhere: " + sphereHitCollider.name);
        }
        else
        {
            sphereHitCollider = null;
            Debug.Log("No Hit. shpere");
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos ���� ����
        Gizmos.color = Color.magenta;

        // �����¿�� Ray �׸���
        Gizmos.DrawRay(transform.position + rayOffset, transform.forward * rayDistance);  // ��
        Gizmos.DrawRay(transform.position + rayOffset, -transform.forward * rayDistance); // ��
        Gizmos.DrawRay(transform.position + rayOffset, -transform.right * rayDistance); // ��
        Gizmos.DrawRay(transform.position + rayOffset, transform.right * rayDistance); // ��

        // �� ������ Ray �׸���
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + sphereRayOffset, sphereRadius);
    }
}