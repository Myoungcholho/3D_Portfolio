using UnityEditor;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    // 타이틀 화면의 Button에서 호출함.
    public void GameStart()
    {
        LoadingSceneController.LoadScene("Main", "TestSpawn01");
    }

    public void GameExit()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
    }
}
