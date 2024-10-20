using UnityEditor;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    // 타이틀 화면의 Button에서 호출함.
    [SerializeField]
    private string sceneName = "CombatTestScene";
    [SerializeField]
    private string spawnLocation = "StartLocation";

    public void GameStart()
    {
        LoadingSceneController.LoadScene(sceneName, spawnLocation);
    }

    public void GameExit()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
    }
}
