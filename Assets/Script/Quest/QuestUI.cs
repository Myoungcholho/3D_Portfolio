using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quest UI 관리 커맨드
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

    // canvas 활성여부
    private bool isActive = false;

    private void Awake()
    {
        questList = new List<GameObject>();
        player = GameObject.Find("Player");

        if(player == null)
        {
            Debug.Log("QuestUI , player가 NULL");
            return;
        }
        qm = player.GetComponent<QuestManager>();
        cursorComponent = player.GetComponent<CursorComponent>();

        if (contents == null)
        {
            Debug.Log("QuestUI , player가 NULL");
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
        // 커서
        cursorComponent.ShowCursorForUI();

        // content 사이즈 조정
        int cnt = qm.activeQuests.Count;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, questListSpacing * cnt);

        // 수락한 퀘스트 수 만큼 동적으로 만들어 추가
        foreach (Quest quest in qm.activeQuests)
        {
            GameObject obj = Instantiate<GameObject>(questPrefab);
            QuestText qText = obj.GetComponent<QuestText>();

            qText.UpdateQuestText(quest.questName);
            qText.Quest = quest;

            // content 자식으로 추가 obj 추가
            obj.transform.SetParent(contents.transform,false);

            // questList에 등록
            questList.Add(obj);
        }
    }

    private void CloseQuestPanel()
    {
        // 커서
        cursorComponent.HideCursorForUI();

        // questList에 등록된 오브젝트들 씬에서 전부 파괴
        foreach (GameObject obj in questList)
        {
            Destroy(obj);
        }

        // 리스트 비우기
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
