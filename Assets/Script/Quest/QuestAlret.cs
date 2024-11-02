using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestAlret : MonoBehaviour
{
    // 어떤 퀘스트를 노출하고 있는지 판단하기 위함
    [HideInInspector]
    public QuestID id;

    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI questDescription;

    public void UpdateText(string title, string description, int cnt =0, int cntMax = 0)
    {
        questTitle.text = title;
        questDescription.text = description;

        if (cntMax != 0)
            questDescription.text += " " + cnt + " / " + cntMax;
    }
}
