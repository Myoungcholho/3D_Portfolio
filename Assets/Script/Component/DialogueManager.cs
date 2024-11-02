using System.Collections.Generic;
using TMPro;
using UnityEngine;

struct DialogueData
{
    public string Name;
    public string Data;
}

public class DialogueManager : MonoBehaviour
{
    private Queue<DialogueData> dialogueQueue = new Queue<DialogueData>(); // 대사 큐

    [SerializeField]
    private string ComunicationUIName = "DialogueCanvas";
    private ComunicationUI panel;

    private void Awake()
    {
        GameObject obj = GameObject.Find(ComunicationUIName);
        panel = obj.GetComponent<ComunicationUI>();
        if(panel == null)
        {
            Debug.Log("panel is null");
        }
    }

    public void AddToDialogueQueue(List<string> dialogues, string npcName)
    {
        foreach(string dialogue in dialogues) 
        {
            DialogueData data = new DialogueData();
            data.Name = npcName;
            data.Data = dialogue;

            dialogueQueue.Enqueue(data);
        }
    }

    // public 큐가 차있는지 확인 있으면 true 없으면 false

    // 큐에서 다이어 로그를 하나꺼내고 실행
    // 만약 마지막 다이어로그였다면 판넬에 접근해서 끄기
    public bool PlayDialogue()
    {
        if (panel == null)
            return false;

        if (dialogueQueue.Count <= 0)
        {
            // 비워져있는데 대화중이었다면 종료
            if(panel.isTalk)
            {
                panel.CloseDialogue();
                return true;
            }


            return false;
        }

        DialogueData dialogue = dialogueQueue.Dequeue();
        panel.UpdateDialogue(dialogue.Name, dialogue.Data);

        

        return true;
    }

    // 큐 사이즈 확인
    public bool QueueEmpty()
    {
        if (dialogueQueue.Count <= 0)
            return true;

        return false;
    }

}
