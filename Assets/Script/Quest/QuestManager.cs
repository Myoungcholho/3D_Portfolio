using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestManager : MonoBehaviour
{
    [SerializeField]
    private string QuestUIObjectName = "QuestCanvas";
    private GameObject questUIObject;
    private QuestUI questUI;

    public List<Quest> activeQuests;    // ���� ���� ���� ����Ʈ ���
    public List<Quest> clearQuests;     // Ŭ������ ����Ʈ ���

    public Action<Quest> OnQuestCompleted;

    private void Awake()
    {
        questUIObject = GameObject.Find(QuestUIObjectName);
        if (questUIObject == null)
        {
            Debug.Log("questUIObject is NULL");
            return;
        }

        questUI = questUIObject.GetComponent<QuestUI>();


        // PlayerInput ���� �� �Է� ����
        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        // ����Ʈ
        actionMap.FindAction("Quest").started += context =>
        {
            questUI.TogglePanel();
        };
    }

    public void AddQuest(Quest newQuest)
    {
        if (!activeQuests.Contains(newQuest))
        {
            activeQuests.Add(newQuest);
        }

        if (OnQuestCompleted != null)
            OnQuestCompleted.Invoke(newQuest);
    }

    // Ŭ���� �� ���� ����
    public void ClearQuest(Quest completedQuest)
    {
        activeQuests.Remove(completedQuest);
        clearQuests.Add(completedQuest);

        if(OnQuestCompleted != null)
            OnQuestCompleted.Invoke(completedQuest);
    }

    // �÷��̾��� ����Ʈ ��Ͽ��� Ư�� NPC�� �ִ� ����Ʈ�� ã�� ��ȯ
    public Quest GetQuest(Quest npcQuest)
    {
        return activeQuests.Find(q => q.questName == npcQuest.questName);
    }
}