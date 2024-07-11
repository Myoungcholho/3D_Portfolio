using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    [SerializeField]
    private Image panelImage;
    
    private Color beginColor = new Color(1f, 0f, 0f, 0.2f);
    private Color endColor = new Color(1f, 0f, 0f, 0.75f);
    private Color tempColor;
    private float baseTime = 1.0f;                      // 기준 시간
    private float randomTime = 0.5f;                    // 랜덤 시간 범위
    private float elapsedTime = 0f;                     // 누적 시간
    private float totalTime;
    float t;

    void Start()
    {
        HealthPointComponent playerHealth = GameObject.Find("Player").GetComponent<HealthPointComponent>();
        playerHealth.lowHpAction += FadePanel;

        totalTime = baseTime + Random.Range(-randomTime, +randomTime);

    }

    // HealthComponent의 update에서 호출함.
    private void FadePanel()
    {
        if (t > 1)
        {
            t = 0;
            elapsedTime = 0;

            tempColor = beginColor;
            beginColor = endColor;
            endColor = tempColor;
            totalTime = baseTime + Random.Range(-randomTime, +randomTime);
        }
        elapsedTime += Time.deltaTime;
        t = elapsedTime / totalTime;

        panelImage.color = Color.Lerp(beginColor, endColor, t);
    }
}
