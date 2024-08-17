using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneDataManager : MonoBehaviour
{
    public static SceneDataManager Instance { get; private set; }

    public string SpawnPoint
    {  get; set; }

    private void Awake()
    {
        // Singleton 패턴 적용: 중복된 오브젝트가 있으면 파괴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 이 오브젝트를 씬 전환 시 파괴하지 않음
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 이 오브젝트 파괴
            return;
        }
    }
}
