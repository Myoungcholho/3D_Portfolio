using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthPointComponent : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100.0f;
    [SerializeField]
    private float currentHealth;
    [SerializeField]
    private string hpBarCanvasName = "HpBarCanvas";
    [SerializeField]
    private string hpBarImageName = "PanelHp";
    [SerializeField]
    private Vector2 hpBarPosition = new Vector2(0, 1.2f);
    [SerializeField]
    private bool hpBarActive = true;

    public Action<float> takeDamage;
    public Action lowHpAction;

    public bool Dead { get => currentHealth <= 0.0f; }

    private GameObject canvasObject;
    private Canvas hpCanvas;
    private Image hpBarImage;

    void Start()
    {
        currentHealth = maxHealth;
        if(GetComponent<Enemy>() != null)
        {
            GameObject prefab = Resources.Load<GameObject>(hpBarCanvasName);
            canvasObject = Instantiate<GameObject>(prefab, transform);
            canvasObject.transform.position += new Vector3(hpBarPosition.x, hpBarPosition.y, 0);
            Debug.Assert(canvasObject != null);
            hpCanvas = canvasObject.GetComponent<Canvas>();
            hpBarImage = canvasObject.transform.FindChildByName(hpBarImageName).GetComponent<Image>();
            Debug.Assert(hpBarImage != null);
        }

    }

    private void Update()
    {
        if(currentHealth <= 20f)
            lowHpAction?.Invoke();

        if(hpCanvas != null)
        {
            hpCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void Damage(float damage)
    {
        if (damage < 1.0f)
            return;

        currentHealth += (damage * -1.0f);
        currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);

        Debug.Log(gameObject + " : " + currentHealth);

        if(hpBarImage != null && hpBarActive) 
        {
            hpBarImage.fillAmount = currentHealth / maxHealth;
        }

        takeDamage?.Invoke(currentHealth / maxHealth);
    }
}
