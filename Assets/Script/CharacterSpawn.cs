using UnityEngine;

// 현재는 캐릭터의 위치만 조정하지만 이후 플레이어를 씬에 배치한 것을 삭제한 뒤 Instantiate로 변경할 것
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
