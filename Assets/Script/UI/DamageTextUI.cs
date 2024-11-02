using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DamageTextUI : MonoBehaviour
{
    public TextMeshProUGUI damageText;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        Destroy(gameObject, 2f);
    }

    private void Update()
    {
        canvas.transform.rotation = Camera.main.transform.rotation;
    }

    public void UpdateDamgeText(float damge)
    {
        damageText.text = "" + damge;
    }
}
