using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUICanvas : MonoBehaviour
{
    [SerializeField]
    private string PlayerName = "Player";
    private GameObject player;

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

    public TextMeshProUGUI weaponText;

    private float prevHp;
    private float curHp;

    private void Awake()
    {
        overKillHpBar = transform.FindChildByName(overPanelName).GetComponent<Image>();     // OverBar Image
        healthBarImage = transform.FindChildByName(hpBarName).GetComponent<Image>();        // hpBar
        player = GameObject.Find(PlayerName);
        Debug.Assert(player != null);
    }

    private void Start()
    {
        WeaponComponent weapon = player.GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);
        weapon.OnWeaponTypeChanged += OnChangeWeapon;
        
        SetObject(player);
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

    public void OnChangeWeapon(WeaponType prev, WeaponType curr)
    {
        string str = curr.ToString();
        if (weaponText == null)
            return;

        weaponText.text = str;
    }
}
