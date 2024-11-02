using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    // NPC�� �̸�
    [SerializeField]
    private string npcName = "None";
    public string NpcName { get { return npcName; } }
    // NPC ID
    [SerializeField]
    public NpcList npcID;


    // NPC�� ��ȭ ������ �������� Ȯ��
    [SerializeField]
    private bool canTalk = true;


    // ��ȭ�� ������ UI
    [Header("������Ʈ �Ҵ��ؾ� ��")]
    [SerializeField]
    private GameObject comunicationPanel;
    private ComunicationUI comuUI;

    #region Outline Variable
    // �����ϸ� outline�� �׸��� ���� Layer ���� ����
    // ���� OutLineLayer�� �ܰ��� �׸�
    public GameObject[] objectsToChangeLayer;                       // ���̾� ������ ���ӿ�����Ʈ�� ���
    private int originalLayer = 0;                                  // ���� Layer ���
    private int outlineLayer = 16;                                  // ��ȭ�� Layer ���
    #endregion

    // Quest
    [Header("����Ʈ �� ��ȭ")]
    public List<Quest> availableQuests;             // NPC�� �� �� �ִ� ����Ʈ ���
    public List<string> basicDailog;                // ��� �� �׻� �ϴ� ����

    private NpcUI npcUI;
    private QuestManager qm;

    private CinemachineVirtualCamera cam;
    private CinemachineBrain brain;

    private void Awake()
    {
        if(comunicationPanel != null)
            comuUI = comunicationPanel.GetComponent<ComunicationUI>();

        npcUI = GetComponentInChildren<NpcUI>();
        qm = FindObjectOfType<QuestManager>();
        cam = GetComponentInChildren<CinemachineVirtualCamera>();
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void Start()
    {
        // Ŭ���� �Ǹ� ȣ��
        qm.OnQuestCompleted += QuestIcon;
        
        QuestIcon();
    }

    // ��ȣ�ۿ� �������̽�, �켱���� ��ȯ
    public int GetPriority()
    {
        return (int)InteractionPriority.NpcComunication;
    }

    // ��ȣ�ۿ� �� ������ �޼��� G�� ���������� ȣ��
    public void Interact(GameObject interactor)
    {
        // �÷��̾��� ����Ʈ ����Ʈ�� ������
        QuestManager questManager = interactor.GetComponent<QuestManager>();
        DialogueManager dm = interactor.GetComponent<DialogueManager>();

        if (questManager == null)
            return;
        if(dm == null) 
            return;

        // ��ȭ�Ұ� ���ٸ� ��Ŀ�� ���
        if (dm.QueueEmpty())
        {
            if (cam.Priority > 0)
                StartCoroutine(DelayCameraBlend(2f, 0, true));
        }

        // ��ȭ�� �����ϴٸ� ť ���� ����
        if (dm.PlayDialogue())
        {
            return;
        }

        // ����Ʈ �Ϸ� ���� �� ó��
        foreach (Quest quest in questManager.activeQuests)
        {
            bool bCheck = true;
            bCheck &= !quest.isCompleted;                       // �Ϸ���� �ʾ�����
            bCheck &= quest.targetNpc == npcID;                 // ���ο��� ���Ŵ°Ŷ��

            if (bCheck == false)
                continue;

            // ���͸� ��� ����Ʈ ���ٸ�
            if(quest.type == QuestType.DefeatMonster)
            {
                // �� ��Ҵٸ�
                if(quest.CheckCompletionHunter())
                {
                    quest.EndQuest();
                    questManager.ClearQuest(quest);

                    // �Ϸ� ���
                    StartDialogue(dm, quest.completedDialog, npcName);

                    return;
                }
            }
            // ��ȭ�� �ϴ� ����Ʈ ���ٸ�
            else if(quest.type == QuestType.TalkToNpc)
            {
                // �Ϸ� ���
                StartDialogue(dm, quest.completedDialog, npcName);

                // ����Ʈ �Ϸ�
                quest.EndQuest();
                questManager.ClearQuest(quest);

                return;
            }
        }


        // �� ����Ʈ ����
        Quest selectedQuest = SelectQuestToGive(questManager);
        if (selectedQuest == null)
        {
            // ������ �� ����Ʈ���� Ȯ��
            Quest ongoingQuest = questManager.activeQuests.Find(q => q.questGiver == npcID);

            // �ִٸ�
            if (ongoingQuest != null)
            {
                // ���� ���� ����Ʈ�� �ִ� ��� ���� �� ��� ���
                StartDialogue(dm, ongoingQuest.inProgressDialog, npcName);

                return;
            }

            // ���ٸ�
            StartDialogue(dm, basicDailog, npcName);

            return;
        }

        // ����Ʈ ���� ���¿� ���� ó�� (NPC����)
        ProcessQuest(dm,selectedQuest, questManager);
    }

    private void StartDialogue(DialogueManager dm, List<string> dialogue, string name)
    {
        StartCoroutine(DelayCameraBlend(0.5f, 15, false));

        dm.AddToDialogueQueue(dialogue, name);
        dm.PlayDialogue();
    }

    IEnumerator DelayCameraBlend(float easeInOutTime, int priority, bool doFirst)
    {
        if(doFirst)
            cam.Priority = priority;                        // ī�޶� ��ȯ

        yield return new WaitForEndOfFrame();

        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        brain.m_DefaultBlend.m_Time = easeInOutTime;        // 2�ʷ�
        
        if(!doFirst)
            cam.Priority = priority;
    }

    #region Quest
    // �� ����Ʈ ���� �޼���
    private Quest SelectQuestToGive(QuestManager questManager)
    {
        foreach(Quest availableQuest in availableQuests)
        {
            // �÷��̾ �̹� Ŭ������ ����Ʈ�� �ǳʶ�
            bool isQuestCleared = questManager.clearQuests.Exists(q => q.questName == availableQuest.questName);
            if (isQuestCleared)
            {
                continue;  // �̹� �Ϸ��� ����Ʈ��� ���� ����Ʈ�� Ȯ��
            }

            // �÷��̾ �̹� Ȱ��ȭ ���� ����Ʈ�� �ǳʶ�
            bool isQuestActive = questManager.activeQuests.Exists(q => q.questName == availableQuest.questName);
            if (isQuestActive)
            {
                continue;  // �̹� ���� ���� ����Ʈ��� ���� ����Ʈ�� Ȯ��
            }

            // ���� �������� ���� ����Ʈ Ȯ��
            bool bCheck = false;

            QuestID required = availableQuest.RequiredQuestID;

            if(required != QuestID.None)
            {
                bCheck = questManager.clearQuests.Exists(q => q.QuestID == required);
            }
            else
            {
                bCheck = true;
            }

            if(bCheck)
            {
                return availableQuest;
            }
        }

        return null;
    }

    // ����Ʈ ���� ����
    private void ProcessQuest(DialogueManager dm, Quest selectedQuest, QuestManager questManager)
    {
        // ScriptableObject �ν��Ͻ�ȭ (���纻 ����)
        Quest questInstance = ScriptableObject.CreateInstance<Quest>();

        // ���õ� ����Ʈ �����͸� ����
        CopyQuestData(selectedQuest, questInstance);

        // ���� ��ȭ
        StartDialogue(dm, questInstance.initialDialog, npcName);

        // ����Ʈ�� ���� �� ����
        questManager.AddQuest(questInstance);
        questInstance.StartQuest();

        // ���� óġ ����Ʈ��� �̺�Ʈ ���� , �����Ǵ� �ֵ����� ���ؼ��� ������ �ָ��� �� �ִµ�..
        if (questInstance.type == QuestType.DefeatMonster)
        {
            // �ʵ忡 �ִ� ��� EnemyInformation ������Ʈ�� ���� ���� ������Ʈ�� �˻�
            EnemyInformation[] enemiesInField = FindObjectsOfType<EnemyInformation>();

            // ���� �߿��� questInstance.monsterID�� ���� monsterType�� ���� ���鿡�� �̺�Ʈ ����
            foreach (EnemyInformation enemy in enemiesInField)
            {
                if (enemy.monsterType == questInstance.monsterID)
                {
                    // ���� ������ ȣ��޾� óġ �� �ø�
                    enemy.OnDeath += questInstance.OnMonsterKilled;
                }
            }
        }
    }

    // ����Ʈ �����͸� �����ϴ� �޼���
    private void CopyQuestData(Quest original, Quest copy)
    {
        copy.QuestID = original.QuestID;
        copy.RequiredQuestID = original.RequiredQuestID;
        copy.questGiver = original.questGiver;
        copy.questName = original.questName;
        copy.isAccepted = original.isAccepted;
        copy.isCompleted = original.isCompleted;
        copy.questGuide = original.questGuide;

        copy.type = original.type;
        copy.targetNpc = original.targetNpc;
        copy.hasInteracted = original.hasInteracted;
        copy.monsterID = original.monsterID;
        copy.currentAmount = original.currentAmount;
        copy.requiredAmount = original.requiredAmount;
        copy.initialDialog = new List<string>(original.initialDialog);
        copy.inProgressDialog = new List<string>(original.inProgressDialog);
        copy.completedDialog = new List<string>(original.completedDialog);
    }

    public void QuestIcon(Quest quest = null)
    {
        QuestStatus state = RefreshQuestIcon();
        npcUI.UpdateQuestSprite(state);
    }

    // �Ӹ� ���� �˸� ����
    private QuestStatus RefreshQuestIcon()
    {
        // 1. '!' , Ŭ���� �������� Ȯ��
        foreach (Quest quest in qm.activeQuests)
        {
            if (quest.targetNpc != npcID)
                continue;

            if(quest.hasInteracted == false)
                continue;

            return QuestStatus.Completeable;
        }

        // 2. '?' , ������ �� �ִ��� Ȯ��
        foreach (Quest npcQuest in availableQuests)
        {
            // [�Ұ���]�̹� Ŭ���� �ѰŶ�� continue
            if (qm.clearQuests.Any(quest => quest.QuestID == npcQuest.QuestID))
                continue;

            // [�Ұ���] �̹� �������̶��
            if (qm.activeQuests.Any(quest => quest.QuestID == npcQuest.QuestID))
                continue;

            // [����]���������� ���ٸ�
            if(npcQuest.RequiredQuestID == QuestID.None)
                return QuestStatus.Available;

            // [����]�������� ����Ʈ�� �޼��Ǿ� �ִٸ�
            foreach (Quest quest in qm.clearQuests)
            {
                if(npcQuest.RequiredQuestID == quest.QuestID)
                {
                    return QuestStatus.Available;
                }
            }
        }// foreach end

        return QuestStatus.None;
    }
    #endregion

    #region Outline
    // ��� ������Ʈ�� Layer�� �����ϴ� �޼���
    private void ChangeLayer(int newLayer)
    {
        foreach (GameObject obj in objectsToChangeLayer)
        {
            obj.layer = newLayer;
        }
    }


    // �÷��̾ Trigger ������ ������ ��
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // �÷��̾� �±׷� ��
        {
            ChangeLayer(outlineLayer); // ��� ������Ʈ�� Layer�� �ƿ����� Layer�� ����
        }
    }

    // �÷��̾ Trigger ������ ������ ��
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ChangeLayer(originalLayer); // ��� ������Ʈ�� Layer�� ���� Layer�� ����
        }
    }
    #endregion
}
