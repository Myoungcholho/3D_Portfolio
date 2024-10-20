using UnityEditor;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    // Ÿ��Ʋ ȭ���� Button���� ȣ����.
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
