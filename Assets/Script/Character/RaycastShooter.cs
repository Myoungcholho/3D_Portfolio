using UnityEngine;

public class RaycastShooter : MonoBehaviour
{
    // Ray를 쏠 거리 변수
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private Vector3 rayOffset = Vector3.zero;

    // 앞 RayHit 정보를 저장할 변수
    public RaycastHit hitForward;

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
            //Debug.Log("Hit forward: " + hitForward.collider.name);
            
            // 캐릭터의 forward 벡터와 충돌체의 forward 벡터를 내적합니다.
            Vector3 colliderForward = hitForward.collider.transform.forward;
            float dotProduct = Vector3.Dot(transform.forward, colliderForward);

            // 내적 값을 로그로 출력합니다.
            //Debug.Log("Dot Product: " + dotProduct);
        }
        else
        {
            // 히트하지 않은 경우, hitUp의 collider를 null로 설정
            hitForward = new RaycastHit();  // hitUp을 초기화하여 이전 값 제거
            //Debug.Log("No Hit. forward");
        }
    }

    void DetectSphereUpdate()
    {
        // 구 형태로 Ray 감지하기
        Collider[] colliders = Physics.OverlapSphere(transform.position + sphereRayOffset, sphereRadius, layerMaskSphere);
        if(colliders.Length > 0 ) 
        {
            sphereHitCollider = colliders[0];
            //Debug.Log("Hit Shhere: " + sphereHitCollider.name);
        }
        else
        {
            sphereHitCollider = null;
            //Debug.Log("No Hit. shpere");
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos 색상 설정
        Gizmos.color = Color.magenta;

        // 상하좌우로 Ray 그리기
        //Gizmos.DrawRay(transform.position + rayOffset, transform.forward * rayDistance);  // 앞

        // 구 형태의 Ray 그리기
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(transform.position + sphereRayOffset, sphereRadius);
    }
}