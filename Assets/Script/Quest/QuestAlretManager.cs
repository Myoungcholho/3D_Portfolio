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
            Debug.Log("QuestManager ����");

        // ����Ʈ Ŭ�����ϸ� ȣ��޾� ���̵� ����
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

    // QuestText.cs OnChangeCheckMark()���� ȣ�� [��ư�� ������]
    // �߰��ϱ�
    public void DisplayQuestAlert(Quest quest)
    {
        // pool�� ���� �ִٸ�
        if(alertPool.Count > 0) 
        {
            GameObject alertInstance = alertPool.Dequeue();
            alertInstance.SetActive(true);                                  // Ȱ��ȭ
            alertInstance.transform.SetParent(alertContainer);              // �����̳ʿ� ���̱�
            alertInstance.transform.localScale = Vector3.one;

            QuestAlret qa = alertInstance.GetComponent<QuestAlret>();
            Debug.Assert(qa != null);

            quest.OnMonsterCountChanged += UpdateQuestAlert;

            if(quest.type == QuestType.DefeatMonster)
                qa.UpdateText(quest.questName, quest.questGuide, quest.currentAmount, quest.requiredAmount);
            else
                qa.UpdateText(quest.questName, quest.questGuide);

            activeAlerts[(int)quest.QuestID] = alertInstance;  // Dictionary�� �߰�
        }
    }

    // ���� óġ �� ������Ʈ, Invoke�� ȣ���
    public void UpdateQuestAlert(Quest quest)
    {
        // quest ID�� �´� �˸� ���� ������Ʈ�� �ִ��� Ȯ��
        if (activeAlerts.TryGetValue((int)quest.QuestID, out GameObject alertInstance))
        {
            QuestAlret qa = alertInstance.GetComponent<QuestAlret>();
            Debug.Assert(qa != null, "QuestAlret ������Ʈ�� ã�� �� �����ϴ�.");

            if (quest.type == QuestType.DefeatMonster)
                qa.UpdateText(quest.questName, quest.questGuide, quest.currentAmount, quest.requiredAmount);
            else
                qa.UpdateText(quest.questName, quest.questGuide);
        }
    }

    // ���̵忡�� �����ϱ�
    // üũ ������������, Ŭ���� ���� ��
    public void HideQuestAlert(Quest quest)
    {
        if (activeAlerts.TryGetValue((int)quest.QuestID, out GameObject alertInstance))
        {
            alertInstance.SetActive(false);
            alertInstance.transform.SetParent(null);        // �θ� ����
            alertPool.Enqueue(alertInstance);               // �ٽ� Ǯ�� �߰�
            activeAlerts.Remove((int)quest.QuestID);        // Dictionary���� ����
        }
    }
}