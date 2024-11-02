using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    TalkToNpc,              //��ȭ
    DefeatMonster,          //����óġ
    CollectItem             //�����ۼ���
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
    [Header("����Ʈ �⺻ ����")]
    public QuestID QuestID;                     // ����Ʈ ID
    public QuestID RequiredQuestID;             // ���� ����Ʈ ID
    public NpcList questGiver;                  // ����Ʈ�� �� NpcID
    public string questName;                    // ����Ʈ �̸�
    public bool isAccepted;                     // ���� ����
    public bool isCompleted;                    // �Ϸ� ����
    public bool isAlret;                        // ����Ʈ �˸� ����
    [TextArea(5, 10)]
    public string questGuide;                   // ����Ʈ ���̵�


    [Header("�Ϸ� �� ��ȭ �ʿ��� NPC �ۼ�")]
    public QuestType type;                  // ����Ʈ Ÿ��
    public NpcList targetNpc;               // ���ɾ���ϴ� NPC ID


    [Header("����Ʈ ���� ���� ����, ��ȭ�� ��� TRUE �Ҵ�")]
    public bool hasInteracted;              // [����Ʈ ������ �����Ǿ�����, ��ȭ�ΰ�� �ٷ� ����]

    [Header("���� óġ �ʿ� �� �ۼ�")]
    public MonsterList monsterID;                       // ���� ID
    public int currentAmount;                           // ���� óġ ����
    public int requiredAmount;                          // óġ �䱸 ����
    public List<EnemyInformation> connectedEnemies = new List<EnemyInformation>();     // ��������Ʈ ������ ģ���� ���
    public event Action<Quest> OnMonsterCountChanged;

    [Header("����Ʈ ���� ���")]
    [TextArea(5, 10)]
    public List<string> initialDialog; // ����Ʈ ���� �� ��Ÿ���� ��ȭ ���

    [Header("����Ʈ ���� ���� �� �ٽ� ���ƿ��� ���� ���")]
    [TextArea(5, 10)]
    public List<string> inProgressDialog;

    [Header("����Ʈ�� �Ϸ�� �� NPC�� �ϴ� ���")]
    [TextArea(5, 10)]
    public List<string> completedDialog;

    // ����Ʈ ���� ����
    public void StartQuest()
    {
        isAccepted = true;

        Debug.Log($"{questName} ����Ʈ�� ���۵Ǿ����ϴ�.");
    }

    // ����Ʈ �Ϸ� ����
    public void EndQuest()
    {
        isCompleted = true;

        // ����Ʈ�� �Ϸ�Ǿ��� �� �̺�Ʈ ����
        if(QuestType.DefeatMonster == type)
        {
            foreach (EnemyInformation enemy in connectedEnemies)
            {
                enemy.OnDeath -= OnMonsterKilled;
            }
        }    

        Debug.Log($"{questName} ����Ʈ�� �Ϸ�Ǿ����ϴ�.");
    }

    // ����Ʈ �Ϸ� �������� Ȯ��
    public bool CheckCompletionTalk()
    {
        return hasInteracted;
    }

    public bool CheckCompletionHunter()
    {
        return currentAmount >= requiredAmount;
    }

    // ���Ͱ� �׾��� �� ȣ��Ǵ� �޼���
    public void OnMonsterKilled(MonsterList monsterID)
    {
        // ���� ���� ���� ����Ʈ�� Ȯ��
        if (type != QuestType.DefeatMonster)
            return;

        if (isAccepted == false || isCompleted == true)
            return;



        if (this.monsterID == monsterID)
        {
            currentAmount++;

            if (OnMonsterCountChanged != null)
                OnMonsterCountChanged.Invoke(this);

            Debug.Log($"{monsterID} ���� óġ��. ���� {currentAmount}/{requiredAmount}");

            // óġ ī��Ʈ�� �䱸 ������ �Ѿ����� Ȯ��
            if (CheckCompletionHunter())
            {
                hasInteracted = true;

                if (NpcManager.Instance == null)
                {
                    Debug.Log("NpcManage is null , Add the NpcManager script to the scene.");
                    return;
                }
                

                // �ش� Npc�� �Ӹ��� "!"�� ����� �� �ְԲ�
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