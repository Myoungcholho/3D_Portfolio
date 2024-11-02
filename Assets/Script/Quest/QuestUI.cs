using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quest UI ���� Ŀ�ǵ�
/// </summary>
public class QuestUI : MonoBehaviour
{
    public GameObject contents;
    public GameObject questPrefab;

    public float questListSpacing = 45f;

    private List<GameObject> questList;
    private QuestManager qm;
    private RectTransform contentRect;
    private Canvas canvas;
    private GameObject player;
    private CursorComponent cursorComponent;

    // canvas Ȱ������
    private bool isActive = false;

    private void Awake()
    {
        questList = new List<GameObject>();
        player = GameObject.Find("Player");

        if(player == null)
        {
            Debug.Log("QuestUI , player�� NULL");
            return;
        }
        qm = player.GetComponent<QuestManager>();
        cursorComponent = player.GetComponent<CursorComponent>();

        if (contents == null)
        {
            Debug.Log("QuestUI , player�� NULL");
            return;
        }
        contentRect = contents.GetComponent<RectTransform>();

        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        canvas.enabled = isActive;
    }
    private void OpenQuestPanel()
    {
        // Ŀ��
        cursorComponent.ShowCursorForUI();

        // content ������ ����
        int cnt = qm.activeQuests.Count;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, questListSpacing * cnt);

        // ������ ����Ʈ �� ��ŭ �������� ����� �߰�
        foreach (Quest quest in qm.activeQuests)
        {
            GameObject obj = Instantiate<GameObject>(questPrefab);
            QuestText qText = obj.GetComponent<QuestText>();

            qText.UpdateQuestText(quest.questName);
            qText.Quest = quest;

            // content �ڽ����� �߰� obj �߰�
            obj.transform.SetParent(contents.transform,false);

            // questList�� ���
            questList.Add(obj);
        }
    }

    private void CloseQuestPanel()
    {
        // Ŀ��
        cursorComponent.HideCursorForUI();

        // questList�� ��ϵ� ������Ʈ�� ������ ���� �ı�
        foreach (GameObject obj in questList)
        {
            Destroy(obj);
        }

        // ����Ʈ ����
        questList.Clear();
    }

    public void TogglePanel()
    {
        isActive = !isActive;

        canvas.enabled = isActive;
        if (isActive == true)
            OpenQuestPanel();
        else
            CloseQuestPanel();

    }
}
