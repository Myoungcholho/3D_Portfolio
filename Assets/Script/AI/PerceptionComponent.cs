using System.Collections.Generic;
using UnityEngine;

// PerceptionComponent Ŭ����: ���� �ý����� �����Ͽ� ���(�÷��̾� ��)�� Ž���ϴ� ���
public class PerceptionComponent : MonoBehaviour
{
    [Header("---���� ��� ��ũ��Ʈ---")]
    [SerializeField]
    private float distance = 10.0f;          // Ž�� �Ÿ�
    [SerializeField]
    private float angle = 45.0f;             // Ž�� ����
    [SerializeField]
    private float lostTime = 5.0f;           // Ž�� �� ���� �ð� ���� ���� ����
    [SerializeField]
    private LayerMask layerMask;             // Ž���� ����� ���̾�

    private Dictionary<GameObject, float> percievedTable;   // ������ ������Ʈ�� ���� �ð� ���

    // �ʱ�ȭ: ���̾� ����ũ ����
    private void Reset()
    {
        layerMask = 1 << LayerMask.NameToLayer("Player");
    }

    // Awake: ������ ��ü�� ������ ��ųʸ� �ʱ�ȭ
    private void Awake()
    {
        percievedTable = new Dictionary<GameObject, float>();
    }

    // �� �����Ӹ��� Ž�� ��� ���� �� �ð� �ʰ��� ��� ����
    private void Update()
    {
        // 1. Ž�� ���� ���� ������Ʈ�� �˻�
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, layerMask);
        Vector3 forward = transform.forward;
        List<Collider> candidateList = new List<Collider>();

        // 1-1. Ž���� ������Ʈ �� ���� ���� �ִ� ������Ʈ�� �ĺ��ڷ� ����
        foreach (Collider collider in colliders)
        {
            Vector3 direction = collider.transform.position - transform.position;
            float signedAngle = Vector3.SignedAngle(forward, direction.normalized, Vector3.up);

            if (Mathf.Abs(signedAngle) <= angle)
            {
                candidateList.Add(collider);
            }
        }

        // 2. �ĺ��ڸ� ���� ���̺� ����ϰų� �ð� ����
        foreach (Collider collider in candidateList)
        {
            if (percievedTable.ContainsKey(collider.gameObject) == false)
            {
                percievedTable.Add(collider.gameObject, Time.realtimeSinceStartup);
                continue;
            }

            percievedTable[collider.gameObject] = Time.realtimeSinceStartup;
        }

        // 3. �ð� �ʰ��� ��� ����
        List<GameObject> removeList = new List<GameObject>();
        foreach (var item in percievedTable)
        {
            if ((Time.realtimeSinceStartup - item.Value) >= lostTime)
                removeList.Add(item.Key);
        }

        removeList.RemoveAll(remove => percievedTable.Remove(remove));
    }

    // ������ ��ü �� "Player" �±װ� �ִ� ��ü ��ȯ
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
    // ���õ� ������Ʈ�� ���� ������ ������ �ð�ȭ (������)
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distance);  // Ž�� ���� ǥ��

        Gizmos.color = Color.blue;

        Vector3 direction = Vector3.zero;
        Vector3 forward = transform.forward;

        // ������ ���� Ž�� ���� ǥ��
        direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);

        direction = Quaternion.AngleAxis(-angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);

        GameObject player = GetPercievedPlayer();
        if (player == null)
            return;

        // ������ �÷��̾ ������ �ð�ȭ
        Gizmos.color = Color.magenta;
        Vector3 position = transform.position;
        position.y += 1f;
        Vector3 playerPosition = player.transform.position;
        playerPosition.y += 1.0f;

        Gizmos.DrawLine(position, playerPosition);
        Gizmos.DrawWireSphere(playerPosition, 0.25f);  // �÷��̾� ��ġ ǥ��
    }
#endif
}