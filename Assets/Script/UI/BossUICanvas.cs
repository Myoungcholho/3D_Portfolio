using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUICanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject BossGameObject;
    [SerializeField]
    private float fillDownSpeed = 0.5f;


    private TextMeshProUGUI bossNameText;
    private Image bossOverKillHpBar;
    private Image healthBarImage;

    
    private float prevHp;
    private float curHp;

    private void Awake()
    {
        bossNameText = transform.FindChildByName("BossText").GetComponent<TextMeshProUGUI>();
        bossOverKillHpBar = transform.FindChildByName("BossHpBar_Yellow").GetComponent<Image>();
        healthBarImage = transform.FindChildByName("BossHpBar").GetComponent<Image>();
    }

    private void Start()
    {
        SetBossObject(BossGameObject);
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

}
