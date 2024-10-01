using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUICanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject Object;
    [SerializeField]
    private float fillDownSpeed = 0.5f;     // hp filldown speed
    [SerializeField]
    public float duration = 3f;             // fade


    [SerializeField]
    private string overPanelName = "PlayerHpBar_Yellow";
    private Image overKillHpBar;

    [SerializeField]
    private string hpBarName = "PlayerHpBar";
    private Image healthBarImage;


    private float prevHp;
    private float curHp;

    private void Awake()
    {
        overKillHpBar = transform.FindChildByName(overPanelName).GetComponent<Image>();     // OverBar Image
        healthBarImage = transform.FindChildByName(hpBarName).GetComponent<Image>();        // hpBar
    }

    private void Start()
    {
        SetObject(Object);
    }

    void Update()
    {
        if (Mathf.Abs(curHp - prevHp) > 0.001f)
        {
            // fillAmount를 서서히 감소시킵니다
            prevHp = Mathf.Lerp(prevHp, curHp, fillDownSpeed * Time.deltaTime);
            overKillHpBar.fillAmount = prevHp;
        }
    }

    public void SetObject(GameObject obj)
    {
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
