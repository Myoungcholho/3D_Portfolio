using UnityEditor;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    // Ÿ��Ʋ ȭ���� Button���� ȣ����.
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
