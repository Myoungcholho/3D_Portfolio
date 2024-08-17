using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUICanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject BossGameObject;
    [SerializeField]
    private float fillDownSpeed = 0.5f;     // hp filldown speed
    [SerializeField]
    public float duration = 3f;             // fade


    private GameObject hpPanel;
    private CanvasGroup canvasGroup;

    private TextMeshProUGUI bossNameText;
    private Image bossOverKillHpBar;
    private Image healthBarImage;

    
    private float prevHp;
    private float curHp;

    private void Awake()
    {
        hpPanel = transform.FindChildByName("HpPanel").gameObject;
        canvasGroup = transform.FindChildByName("ClearPanel").GetComponent<CanvasGroup>();
        Debug.Assert(canvasGroup != null);

        bossNameText = transform.FindChildByName("BossText").GetComponent<TextMeshProUGUI>();
        bossOverKillHpBar = transform.FindChildByName("BossHpBar_Yellow").GetComponent<Image>();
        healthBarImage = transform.FindChildByName("BossHpBar").GetComponent<Image>();
    }

    private void Start()
    {
        SetBossObject(BossGameObject);

        hpPanel.SetActive(false);
        canvasGroup.alpha = 0f;

    }

    void Update()
    {
        if (Mathf.Abs(curHp - prevHp) > 0.001f)
        {
            // fillAmount를 서서히 감소시킵니다
            prevHp = Mathf.Lerp(prevHp, curHp, fillDownSpeed * Time.deltaTime);
            bossOverKillHpBar.fillAmount = prevHp;
        }
    }

    public void SetBossObject(GameObject obj)
    {
        BossGameObject = obj;
        bossNameText.text = BossGameObject.name;

        HealthPointComponent health = obj.GetComponent<HealthPointComponent>();
        if (health == null)
            return;

        health.takeDamage += ReduceHpCanvas;
        curHp = 1.0f;
        prevHp = 1.0f;
    }

    public void ReduceHpCanvas(float hp)
    {
        curHp = hp;
        healthBarImage.fillAmount = hp;
    }

    public void SetActivehpPanel(bool active)
    {
        hpPanel.SetActive(active);
    }

    public void StartCoroutineClearPanel()
    {
        StartCoroutine(FadeIn());
    }

    #region ClearPanelFade
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(2f);
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
    #endregion
}
