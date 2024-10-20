using System.Collections.Generic;
using UnityEngine;

// PerceptionComponent 클래스: 감지 시스템을 구현하여 대상(플레이어 등)을 탐지하는 기능
public class PerceptionComponent : MonoBehaviour
{
    [Header("---감지 기능 스크립트---")]
    [SerializeField]
    private float distance = 10.0f;          // 탐지 거리
    [SerializeField]
    private float angle = 45.0f;             // 탐지 각도
    [SerializeField]
    private float lostTime = 5.0f;           // 탐지 후 일정 시간 동안 감지 유지
    [SerializeField]
    private LayerMask layerMask;             // 탐지할 대상의 레이어

    private Dictionary<GameObject, float> percievedTable;   // 감지된 오브젝트와 감지 시간 기록

    // 초기화: 레이어 마스크 설정
    private void Reset()
    {
        layerMask = 1 << LayerMask.NameToLayer("Player");
    }

    // Awake: 감지된 객체를 저장할 딕셔너리 초기화
    private void Awake()
    {
        percievedTable = new Dictionary<GameObject, float>();
    }

    // 매 프레임마다 탐지 대상 갱신 및 시간 초과된 대상 제거
    private void Update()
    {
        // 1. 탐지 범위 내의 오브젝트를 검색
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, layerMask);
        Vector3 forward = transform.forward;
        List<Collider> candidateList = new List<Collider>();

        // 1-1. 탐지된 오브젝트 중 각도 내에 있는 오브젝트를 후보자로 선정
        foreach (Collider collider in colliders)
        {
            Vector3 direction = collider.transform.position - transform.position;
            float signedAngle = Vector3.SignedAngle(forward, direction.normalized, Vector3.up);

            if (Mathf.Abs(signedAngle) <= angle)
            {
                candidateList.Add(collider);
            }
        }

        // 2. 후보자를 감지 테이블에 등록하거나 시간 갱신
        foreach (Collider collider in candidateList)
        {
            if (percievedTable.ContainsKey(collider.gameObject) == false)
            {
                percievedTable.Add(collider.gameObject, Time.realtimeSinceStartup);
                continue;
            }

            percievedTable[collider.gameObject] = Time.realtimeSinceStartup;
        }

        // 3. 시간 초과된 대상 제거
        List<GameObject> removeList = new List<GameObject>();
        foreach (var item in percievedTable)
        {
            if ((Time.realtimeSinceStartup - item.Value) >= lostTime)
                removeList.Add(item.Key);
        }

        removeList.RemoveAll(remove => percievedTable.Remove(remove));
    }

    // 감지된 객체 중 "Player" 태그가 있는 객체 반환
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
    // 선택된 오브젝트의 감지 영역을 기즈모로 시각화 (디버깅용)
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distance);  // 탐지 범위 표시

        Gizmos.color = Color.blue;

        Vector3 direction = Vector3.zero;
        Vector3 forward = transform.forward;

        // 각도에 따른 탐지 범위 표시
        direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);

        direction = Quaternion.AngleAxis(-angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);

        GameObject player = GetPercievedPlayer();
        if (player == null)
            return;

        // 감지된 플레이어를 기즈모로 시각화
        Gizmos.color = Color.magenta;
        Vector3 position = transform.position;
        position.y += 1f;
        Vector3 playerPosition = player.transform.position;
        playerPosition.y += 1.0f;

        Gizmos.DrawLine(position, playerPosition);
        Gizmos.DrawWireSphere(playerPosition, 0.25f);  // 플레이어 위치 표시
    }
#endif
}