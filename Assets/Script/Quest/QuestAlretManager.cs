using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestAlretManager : MonoBehaviour
{
    private Queue<GameObject> alertPool = new Queue<GameObject>();
    private Dictionary<int, GameObject> activeAlerts = new Dictionary<int, GameObject>();

    [SerializeField]
    private int poolSize = 3;

    [SerializeField]
    private GameObject questAlertPrefab;

    [SerializeField]
    private Transform alertContainer;
    private QuestManager qm;

    private void Awake()
    {
        qm = FindObjectOfType<QuestManager>();
        if (qm == null)
            Debug.Log("QuestManager 없음");

        // 퀘스트 클리어하면 호출받아 가이드 제거
        qm.OnQuestCompleted += HideQuestAlert;
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for(int i=0; i<poolSize; i++)
        {
            GameObject alertInstance = Instantiate<GameObject>(questAlertPrefab);
            alertInstance.SetActive(false);
            alertPool.Enqueue(alertInstance);
        }
    }

    // QuestText.cs OnChangeCheckMark()에서 호출 [버튼이 눌리면]
    // 추가하기
    public void DisplayQuestAlert(Quest quest)
    {
        // pool이 남아 있다면
        if(alertPool.Count > 0) 
        {
            GameObject alertInstance = alertPool.Dequeue();
            alertInstance.SetActive(true);                                  // 활성화
            alertInstance.transform.SetParent(alertContainer);              // 컨테이너에 붙이기
            alertInstance.transform.localScale = Vector3.one;

            QuestAlret qa = alertInstance.GetComponent<QuestAlret>();
            Debug.Assert(qa != null);

            quest.OnMonsterCountChanged += UpdateQuestAlert;

            if(quest.type == QuestType.DefeatMonster)
                qa.UpdateText(quest.questName, quest.questGuide, quest.currentAmount, quest.requiredAmount);
            else
                qa.UpdateText(quest.questName, quest.questGuide);

            activeAlerts[(int)quest.QuestID] = alertInstance;  // Dictionary에 추가
        }
    }

    // 몬스터 처치 시 업데이트, Invoke로 호출됨
    public void UpdateQuestAlert(Quest quest)
    {
        // quest ID에 맞는 알림 게임 오브젝트가 있는지 확인
        if (activeAlerts.TryGetValue((int)quest.QuestID, out GameObject alertInstance))
        {
            QuestAlret qa = alertInstance.GetComponent<QuestAlret>();
            Debug.Assert(qa != null, "QuestAlret 컴포넌트를 찾을 수 없습니다.");

            if (quest.type == QuestType.DefeatMonster)
                qa.UpdateText(quest.questName, quest.questGuide, quest.currentAmount, quest.requiredAmount);
            else
                qa.UpdateText(quest.questName, quest.questGuide);
        }
    }

    // 가이드에서 삭제하기
    // 체크 해제했을때와, 클리어 했을 때
    public void HideQuestAlert(Quest quest)
    {
        if (activeAlerts.TryGetValue((int)quest.QuestID, out GameObject alertInstance))
        {
            alertInstance.SetActive(false);
            alertInstance.transform.SetParent(null);        // 부모 해제
            alertPool.Enqueue(alertInstance);               // 다시 풀에 추가
            activeAlerts.Remove((int)quest.QuestID);        // Dictionary에서 제거
        }
    }
}