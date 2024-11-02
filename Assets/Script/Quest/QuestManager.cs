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

    public List<Quest> activeQuests;    // 현재 진행 중인 퀘스트 목록
    public List<Quest> clearQuests;     // 클리어한 퀘스트 목록

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


        // PlayerInput 설정 및 입력 매핑
        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        // 퀘스트
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

    // 클리어 시 따로 보관
    public void ClearQuest(Quest completedQuest)
    {
        activeQuests.Remove(completedQuest);
        clearQuests.Add(completedQuest);

        if(OnQuestCompleted != null)
            OnQuestCompleted.Invoke(completedQuest);
    }

    // 플레이어의 퀘스트 목록에서 특정 NPC가 주는 퀘스트를 찾아 반환
    public Quest GetQuest(Quest npcQuest)
    {
        return activeQuests.Find(q => q.questName == npcQuest.questName);
    }
}