using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPointComponent : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100.0f;
    private float currentHealth;


    public bool Dead { get => currentHealth <= 0.0f; }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void Damage(float damage)
    {
        if (damage < 1.0f)
            return;

        currentHealth += (damage * -1.0f);
        currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);

        Debug.Log($"currentHealth : {currentHealth}");
    }
}
