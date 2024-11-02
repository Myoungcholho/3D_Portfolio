using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    // NPC의 이름
    [SerializeField]
    private string npcName = "None";
    public string NpcName { get { return npcName; } }
    // NPC ID
    [SerializeField]
    public NpcList npcID;


    // NPC와 대화 가능한 상태인지 확인
    [SerializeField]
    private bool canTalk = true;


    // 대화를 보여줄 UI
    [Header("오브젝트 할당해야 함")]
    [SerializeField]
    private GameObject comunicationPanel;
    private ComunicationUI comuUI;

    #region Outline Variable
    // 근접하면 outline을 그리기 위해 Layer 동적 변경
    // 현재 OutLineLayer만 외곽을 그림
    public GameObject[] objectsToChangeLayer;                       // 레이어 변경할 게임오브젝트들 등록
    private int originalLayer = 0;                                  // 본래 Layer 등록
    private int outlineLayer = 16;                                  // 변화할 Layer 등록
    #endregion

    // Quest
    [Header("퀘스트 및 대화")]
    public List<Quest> availableQuests;             // NPC가 줄 수 있는 퀘스트 목록
    public List<string> basicDailog;                // 평상 시 항상 하는 말들

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
        // 클리어 되면 호출
        qm.OnQuestCompleted += QuestIcon;
        
        QuestIcon();
    }

    // 상호작용 인터페이스, 우선순위 반환
    public int GetPriority()
    {
        return (int)InteractionPriority.NpcComunication;
    }

    // 상호작용 시 실행할 메서드 G를 누를때마다 호출
    public void Interact(GameObject interactor)
    {
        // 플레이어의 퀘스트 리스트를 가져옴
        QuestManager questManager = interactor.GetComponent<QuestManager>();
        DialogueManager dm = interactor.GetComponent<DialogueManager>();

        if (questManager == null)
            return;
        if(dm == null) 
            return;

        // 대화할게 없다면 포커싱 취소
        if (dm.QueueEmpty())
        {
            if (cam.Priority > 0)
                StartCoroutine(DelayCameraBlend(2f, 0, true));
        }

        // 대화가 가능하다면 큐 비우고 리턴
        if (dm.PlayDialogue())
        {
            return;
        }

        // 퀘스트 완료 검증 및 처리
        foreach (Quest quest in questManager.activeQuests)
        {
            bool bCheck = true;
            bCheck &= !quest.isCompleted;                       // 완료되지 않았으며
            bCheck &= quest.targetNpc == npcID;                 // 본인에게 말거는거라면

            if (bCheck == false)
                continue;

            // 몬스터를 잡는 퀘스트 였다면
            if(quest.type == QuestType.DefeatMonster)
            {
                // 다 잡았다면
                if(quest.CheckCompletionHunter())
                {
                    quest.EndQuest();
                    questManager.ClearQuest(quest);

                    // 완료 대사
                    StartDialogue(dm, quest.completedDialog, npcName);

                    return;
                }
            }
            // 대화만 하는 퀘스트 였다면
            else if(quest.type == QuestType.TalkToNpc)
            {
                // 완료 대사
                StartDialogue(dm, quest.completedDialog, npcName);

                // 퀘스트 완료
                quest.EndQuest();
                questManager.ClearQuest(quest);

                return;
            }
        }


        // 줄 퀘스트 선정
        Quest selectedQuest = SelectQuestToGive(questManager);
        if (selectedQuest == null)
        {
            // 본인이 준 퀘스트인지 확인
            Quest ongoingQuest = questManager.activeQuests.Find(q => q.questGiver == npcID);

            // 있다면
            if (ongoingQuest != null)
            {
                // 진행 중인 퀘스트가 있는 경우 진행 중 대사 출력
                StartDialogue(dm, ongoingQuest.inProgressDialog, npcName);

                return;
            }

            // 없다면
            StartDialogue(dm, basicDailog, npcName);

            return;
        }

        // 퀘스트 진행 상태에 따른 처리 (NPC본인)
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
            cam.Priority = priority;                        // 카메라 전환

        yield return new WaitForEndOfFrame();

        brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        brain.m_DefaultBlend.m_Time = easeInOutTime;        // 2초로
        
        if(!doFirst)
            cam.Priority = priority;
    }

    #region Quest
    // 줄 퀘스트 선정 메서드
    private Quest SelectQuestToGive(QuestManager questManager)
    {
        foreach(Quest availableQuest in availableQuests)
        {
            // 플레이어가 이미 클리어한 퀘스트는 건너뜀
            bool isQuestCleared = questManager.clearQuests.Exists(q => q.questName == availableQuest.questName);
            if (isQuestCleared)
            {
                continue;  // 이미 완료한 퀘스트라면 다음 퀘스트를 확인
            }

            // 플레이어가 이미 활성화 중인 퀘스트도 건너뜀
            bool isQuestActive = questManager.activeQuests.Exists(q => q.questName == availableQuest.questName);
            if (isQuestActive)
            {
                continue;  // 이미 진행 중인 퀘스트라면 다음 퀘스트를 확인
            }

            // 수락 가능한지 선행 퀘스트 확인
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

    // 퀘스트 수락 절차
    private void ProcessQuest(DialogueManager dm, Quest selectedQuest, QuestManager questManager)
    {
        // ScriptableObject 인스턴스화 (복사본 생성)
        Quest questInstance = ScriptableObject.CreateInstance<Quest>();

        // 선택된 퀘스트 데이터를 복사
        CopyQuestData(selectedQuest, questInstance);

        // 수락 대화
        StartDialogue(dm, questInstance.initialDialog, npcName);

        // 퀘스트를 수락 및 시작
        questManager.AddQuest(questInstance);
        questInstance.StartQuest();

        // 몬스터 처치 퀘스트라면 이벤트 연결 , 스폰되는 애들한테 대해서는 연결이 애매할 수 있는데..
        if (questInstance.type == QuestType.DefeatMonster)
        {
            // 필드에 있는 모든 EnemyInformation 컴포넌트를 가진 게임 오브젝트를 검색
            EnemyInformation[] enemiesInField = FindObjectsOfType<EnemyInformation>();

            // 적들 중에서 questInstance.monsterID와 같은 monsterType을 가진 적들에게 이벤트 연결
            foreach (EnemyInformation enemy in enemiesInField)
            {
                if (enemy.monsterType == questInstance.monsterID)
                {
                    // 몬스터 죽으면 호출받아 처치 수 늘림
                    enemy.OnDeath += questInstance.OnMonsterKilled;
                }
            }
        }
    }

    // 퀘스트 데이터를 복사하는 메서드
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

    // 머리 위에 알림 여부
    private QuestStatus RefreshQuestIcon()
    {
        // 1. '!' , 클리어 가능한지 확인
        foreach (Quest quest in qm.activeQuests)
        {
            if (quest.targetNpc != npcID)
                continue;

            if(quest.hasInteracted == false)
                continue;

            return QuestStatus.Completeable;
        }

        // 2. '?' , 수락할 수 있는지 확인
        foreach (Quest npcQuest in availableQuests)
        {
            // [불가능]이미 클리어 한거라면 continue
            if (qm.clearQuests.Any(quest => quest.QuestID == npcQuest.QuestID))
                continue;

            // [불가능] 이미 수행중이라면
            if (qm.activeQuests.Any(quest => quest.QuestID == npcQuest.QuestID))
                continue;

            // [가능]선행조건이 없다면
            if(npcQuest.RequiredQuestID == QuestID.None)
                return QuestStatus.Available;

            // [가능]선행조건 퀘스트가 달성되어 있다면
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
    // 모든 오브젝트의 Layer를 변경하는 메서드
    private void ChangeLayer(int newLayer)
    {
        foreach (GameObject obj in objectsToChangeLayer)
        {
            obj.layer = newLayer;
        }
    }


    // 플레이어가 Trigger 안으로 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 플레이어 태그로 비교
        {
            ChangeLayer(outlineLayer); // 모든 오브젝트의 Layer를 아웃라인 Layer로 변경
        }
    }

    // 플레이어가 Trigger 밖으로 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ChangeLayer(originalLayer); // 모든 오브젝트의 Layer를 원래 Layer로 복구
        }
    }
    #endregion
}
