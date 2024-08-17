using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    static string nextScene;
    private TextMeshProUGUI loadingText;

    public string[] loadText;
    private int loadTextLength;
    private int textIndex;

    [SerializeField]
    private Image progressBar;

    public static void LoadScene(string sceneName,string spawnPointName)
    {
        nextScene = sceneName;

        SceneDataManager.Instance.SpawnPoint = spawnPointName;
        SceneManager.LoadScene("LoadingScene");
    }


    private void Awake()
    {
        loadingText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        // loadingText
        loadTextLength = loadText.Length;
        textIndex = 0;
        LoadingTextUpdate();

        // Loadingbar
        StartCoroutine(LoadSceneProcess());
    }

    #region UI Button
    public void LeftButtonDown()
    {
        textIndex--;

        if (textIndex < 0)
        {
            textIndex = loadTextLength - 1;
            LoadingTextUpdate();
            return;
        }

        LoadingTextUpdate();
    }

    public void RightButtonDown()
    {
        textIndex++;
        if(loadTextLength -1 < textIndex)
        {
            textIndex = 0;
            LoadingTextUpdate();
            return;
        }

        LoadingTextUpdate();
    }

    private void LoadingTextUpdate()
    {
        loadingText.text = loadText[textIndex];
    }
    #endregion

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            // 0% ~ 40%
            while (op.progress < 0.4f)
            {
                yield return null;
            }

            // 40%에서 2초 서서히 증가
            yield return StartCoroutine(LerpFillAmount(0.4f, 4f));

            // 40% ~ 60%
            while (op.progress < 0.6f)
            {
                yield return null;
            }

            // 60%에서 2초 서서히 증가
            yield return StartCoroutine(LerpFillAmount(0.6f, 0.5f));

            // 60% ~ 90%
            while (op.progress < 0.9f)
            {
                yield return null;
            }

            // 90%에서 2초 서서히 증가
            yield return StartCoroutine(LerpFillAmount(0.9f, 0.5f));

            // 로딩 완료 후 0.5초 동안 90%에서 100%로 서서히 증가
            yield return StartCoroutine(LerpFillAmount(1f, 0.5f));

            // 씬 활성화
            op.allowSceneActivation = true;
            yield break;
        }
    }

    IEnumerator LerpFillAmount(float targetFill, float duration)
    {
        float startFill = progressBar.fillAmount;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            progressBar.fillAmount = Mathf.Lerp(startFill, targetFill, timer / duration);
            yield return null;
        }

        progressBar.fillAmount = targetFill;
    }
}
