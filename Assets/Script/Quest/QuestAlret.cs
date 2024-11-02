using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestAlret : MonoBehaviour
{
    // � ����Ʈ�� �����ϰ� �ִ��� �Ǵ��ϱ� ����
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
