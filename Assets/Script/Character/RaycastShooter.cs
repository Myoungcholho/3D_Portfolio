using UnityEngine;

public class RaycastShooter : MonoBehaviour
{
    // Ray를 쏠 거리 변수
    [SerializeField] private float rayDistance = 0.5f;
    [SerializeField] private Vector3 rayOffset = Vector3.zero;

    // 상,하,좌,우 RayHit 정보를 저장할 변수
    public RaycastHit hitForward;
    public RaycastHit hitBackward;
    public RaycastHit hitLeft;
    public RaycastHit hitRight;

    // Ray가 감지할 Layer 변수
    [SerializeField] private LayerMask layerMaskRay;

    // 구 형태의 RayHit 정보를 저장할 변수
    [SerializeField] private float sphereRadius = 1f;
    [SerializeField] private Vector3 sphereRayOffset = Vector3.zero;
    public Collider sphereHitCollider;

    [SerializeField] private LayerMask layerMaskSphere;
    void Update()
    {
        // 상하좌우로 Ray 쏘기, 길이는 rayDistance 변수만큼, Layer는 layerMask로
        ShootRaysUpdate();

        // 구 형태로 Ray 감지하기
        DetectSphereUpdate();
    }

    void ShootRaysUpdate()
    {
        // 상
        if (Physics.Raycast(transform.position + rayOffset, transform.forward, out hitForward, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit forward: " + hitForward.collider.name);
        }
        else
        {
            // 히트하지 않은 경우, hitUp의 collider를 null로 설정
            hitForward = new RaycastHit();  // hitUp을 초기화하여 이전 값 제거
            Debug.Log("No Hit. forward");
        }

        // 하
        if (Physics.Raycast(transform.position + rayOffset, -transform.forward, out hitBackward, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit backward: " + hitBackward.collider.name);
        }
        else
        {
            // 히트하지 않은 경우, hitUp의 collider를 null로 설정
            hitForward = new RaycastHit();  // hitUp을 초기화하여 이전 값 제거
            Debug.Log("No Hit. down");
        }

        // 좌
        if (Physics.Raycast(transform.position + rayOffset, -transform.right, out hitLeft, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit Left: " + hitLeft.collider.name);
        }
        else
        {
            // 히트하지 않은 경우, hitUp의 collider를 null로 설정
            hitForward = new RaycastHit();  // hitUp을 초기화하여 이전 값 제거
            Debug.Log("No Hit. left");
        }

        // 우
        if (Physics.Raycast(transform.position + rayOffset, transform.right, out hitRight, rayDistance, layerMaskRay))
        {
            Debug.Log("Hit Right: " + hitRight.collider.name);
        }
        else
        {
            // 히트하지 않은 경우, hitUp의 collider를 null로 설정
            hitForward = new RaycastHit();  // hitUp을 초기화하여 이전 값 제거
            Debug.Log("No Hit. right");
        }
    }

    void DetectSphereUpdate()
    {
        // 구 형태로 Ray 감지하기
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
        // Gizmos 색상 설정
        Gizmos.color = Color.magenta;

        // 상하좌우로 Ray 그리기
        Gizmos.DrawRay(transform.position + rayOffset, transform.forward * rayDistance);  // 상
        Gizmos.DrawRay(transform.position + rayOffset, -transform.forward * rayDistance); // 하
        Gizmos.DrawRay(transform.position + rayOffset, -transform.right * rayDistance); // 좌
        Gizmos.DrawRay(transform.position + rayOffset, transform.right * rayDistance); // 우

        // 구 형태의 Ray 그리기
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + sphereRayOffset, sphereRadius);
    }
}