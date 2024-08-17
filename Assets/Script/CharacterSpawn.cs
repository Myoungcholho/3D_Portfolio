using UnityEngine;

// ����� ĳ������ ��ġ�� ���������� ���� �÷��̾ ���� ��ġ�� ���� ������ �� Instantiate�� ������ ��
public class CharacterSpawn : MonoBehaviour
{
    private GameObject player;

    private void Awake()
    {
        player = GameObject.Find("Player");
        Debug.Assert(player != null);

    }

    void Start()
    {
        string spawnObjectName = SceneDataManager.Instance.SpawnPoint;
        GameObject obj = GameObject.Find(spawnObjectName);
        Debug.Assert(obj != null);

        if (obj == null)
            return;

        Transform spawnTransform = obj.transform;

        player.transform.position = spawnTransform.position;
        player.transform.rotation = spawnTransform.rotation;
    }

}
