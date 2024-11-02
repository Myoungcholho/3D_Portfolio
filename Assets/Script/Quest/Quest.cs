using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    TalkToNpc,              //대화
    DefeatMonster,          //몬스터처치
    CollectItem             //아이템수집
}

public enum QuestID
{
    None =0, Arisa01 = 1, Arisa02 =2, Brute01 =3, Max
}

public enum NpcList
{
    None = 0, Arisa = 1, Brute = 2, Max
}

public enum MonsterList
{
    None = 0, Sword, Staff, Boss, Dummy,Max
}


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Quest", order = 1)]
[System.Serializable]
public class Quest : ScriptableObject
{
    [Header("퀘스트 기본 정보")]
    public QuestID QuestID;                     // 퀘스트 ID
    public QuestID RequiredQuestID;             // 선행 퀘스트 ID
    public NpcList questGiver;                  // 퀘스트를 준 NpcID
    public string questName;                    // 퀘스트 이름
    public bool isAccepted;                     // 수락 여부
    public bool isCompleted;                    // 완료 여부
    public bool isAlret;                        // 퀘스트 알림 여부
    [TextArea(5, 10)]
    public string questGuide;                   // 퀘스트 가이드


    [Header("완료 시 대화 필요한 NPC 작성")]
    public QuestType type;                  // 퀘스트 타입
    public NpcList targetNpc;               // 말걸어야하는 NPC ID


    [Header("퀘스트 조건 충족 여부, 대화인 경우 TRUE 할당")]
    public bool hasInteracted;              // [퀘스트 조건이 충족되었는지, 대화인경우 바로 충족]

    [Header("몬스터 처치 필요 시 작성")]
    public MonsterList monsterID;                       // 몬스터 ID
    public int currentAmount;                           // 현재 처치 수량
    public int requiredAmount;                          // 처치 요구 수량
    public List<EnemyInformation> connectedEnemies = new List<EnemyInformation>();     // 델리게이트 연결한 친구들 기록
    public event Action<Quest> OnMonsterCountChanged;

    [Header("퀘스트 시작 대사")]
    [TextArea(5, 10)]
    public List<string> initialDialog; // 퀘스트 시작 시 나타나는 대화 목록

    [Header("퀘스트 진행 중일 때 다시 돌아왔을 때의 대사")]
    [TextArea(5, 10)]
    public List<string> inProgressDialog;

    [Header("퀘스트가 완료된 후 NPC가 하는 대사")]
    [TextArea(5, 10)]
    public List<string> completedDialog;

    // 퀘스트 시작 설정
    public void StartQuest()
    {
        isAccepted = true;

        Debug.Log($"{questName} 퀘스트가 시작되었습니다.");
    }

    // 퀘스트 완료 설정
    public void EndQuest()
    {
        isCompleted = true;

        // 퀘스트가 완료되었을 때 이벤트 해제
        if(QuestType.DefeatMonster == type)
        {
            foreach (EnemyInformation enemy in connectedEnemies)
            {
                enemy.OnDeath -= OnMonsterKilled;
            }
        }    

        Debug.Log($"{questName} 퀘스트가 완료되었습니다.");
    }

    // 퀘스트 완료 가능한지 확인
    public bool CheckCompletionTalk()
    {
        return hasInteracted;
    }

    public bool CheckCompletionHunter()
    {
        return currentAmount >= requiredAmount;
    }

    // 몬스터가 죽었을 때 호출되는 메서드
    public void OnMonsterKilled(MonsterList monsterID)
    {
        // 현재 진행 중인 퀘스트를 확인
        if (type != QuestType.DefeatMonster)
            return;

        if (isAccepted == false || isCompleted == true)
            return;



        if (this.monsterID == monsterID)
        {
            currentAmount++;

            if (OnMonsterCountChanged != null)
                OnMonsterCountChanged.Invoke(this);

            Debug.Log($"{monsterID} 몬스터 처치됨. 현재 {currentAmount}/{requiredAmount}");

            // 처치 카운트가 요구 수량을 넘었는지 확인
            if (CheckCompletionHunter())
            {
                hasInteracted = true;

                if (NpcManager.Instance == null)
                {
                    Debug.Log("NpcManage is null , Add the NpcManager script to the scene.");
                    return;
                }
                

                // 해당 Npc의 머리에 "!"를 띄어줄 수 있게끔
                Npc npc = NpcManager.Instance.GetNpc(targetNpc);
                if(npc == null)
                {
                    Debug.Log("NpcManager is null, Quest.cs");
                    return;
                }

                npc.QuestIcon();

                return;
            }

        }
    }
}