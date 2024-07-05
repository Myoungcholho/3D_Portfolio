using System.Collections.Generic;
using UnityEngine;

public class PerceptionComponent : MonoBehaviour
{
    [SerializeField]
    private float distance = 5.0f;          // 탐지 거리

    [SerializeField]
    private float angle = 45.0f;            // 탐지 각도

    [SerializeField]
    private float lostTime = 2.0f;          // 범위 바깥에 나가도 탐지 가능한 시간

    [SerializeField]
    private LayerMask layerMask;            // 탐지할 Layer

    private Dictionary<GameObject, float> percievedTable;   // 감지된 오브젝트 탐지

    private void Reset()
    {
        layerMask = 1 << LayerMask.NameToLayer("Character");
    }
    private void Awake()
    {
        percievedTable = new Dictionary<GameObject, float>();
    }

    private void Update()
    {
        // 1. 탐지된 대상을 List에 등록 , 매 프레임 감지 대상을 List에 계속 등록함
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, layerMask);

        Vector3 forward = transform.forward;
        List<Collider> candidateList = new List<Collider>();

        foreach (Collider collider in colliders)
        {
            Vector3 direction = collider.transform.position - transform.position;
            float signedAngle = Vector3.SignedAngle(forward, direction.normalized, Vector3.up);

            if (Mathf.Abs(signedAngle) <= angle)
            {
                candidateList.Add(collider);
            }
        }

        // 2. 감시 대상자 등록 및 시간 업데이트
        // 만약 감지된 객체가 Dictionary에 없다면 등록
        // 그와 상관 없이 해당 감지된 GameObject는 감지된 시간을 저장
        foreach (Collider collider in candidateList)
        {
            if (percievedTable.ContainsKey(collider.gameObject) == false)
            {
                percievedTable.Add(collider.gameObject, Time.realtimeSinceStartup);
                continue;
            }

            percievedTable[collider.gameObject] = Time.realtimeSinceStartup;
        }

        // 3. 시간 초과 대상자 선정 및 삭제
        // 만약 등록했던 시간이 오래되어 2초가 넘겨졌다면
        // 해당 객체는 Dictionary에서 제거하기 위해 List에 등록해 두었다가
        // 마지막에 일괄 삭제 ,
        // 이유) 순회하는 도중에 제거되면 내부적으로 유지하고 있는 순회 포인터가
        // 일관성을 잃기 때문이다.
        List<GameObject> removeList = new List<GameObject>();
        foreach (var item in percievedTable)
        {
            if ((Time.realtimeSinceStartup - item.Value) >= lostTime)
                removeList.Add(item.Key);
        }

        removeList.RemoveAll(remove => percievedTable.Remove(remove));
    }


    public GameObject GetPercievedPlayer()
    {
        foreach (var item in percievedTable)
        {
            if (item.Key.CompareTag("Player"))
                return item.Key;
        }

        return null;
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distance);

        Gizmos.color = Color.blue;

        Vector3 direction = Vector3.zero;
        Vector3 forward = transform.forward;

        // 벡터를 쿼터니언에 넣으면 쿼터니언 회전 방향으로 벡터가 변경됨
        // 유니티는 열우선이라 벡터가 뒤에 온다.

        // AngleAxis는 45도 넣으면 Quaternion으로 반환
        direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);

        direction = Quaternion.AngleAxis(-angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);

        GameObject player = GetPercievedPlayer();
        if (player == null)
            return;

        // 추적할 대상 플레이어를 그려냄
        Gizmos.color = Color.magenta;
        Vector3 position = transform.position;
        position.y += 1f;
        Vector3 playerPosition = player.transform.position;
        playerPosition.y += 1.0f;

        Gizmos.DrawLine(position, playerPosition);
        Gizmos.DrawWireSphere(playerPosition, 0.25f);


    }
#endif
}
