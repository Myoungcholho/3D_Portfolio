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
    private Queue<DialogueData> dialogueQueue = new Queue<DialogueData>(); // ��� ť

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

    // public ť�� ���ִ��� Ȯ�� ������ true ������ false

    // ť���� ���̾� �α׸� �ϳ������� ����
    // ���� ������ ���̾�α׿��ٸ� �ǳڿ� �����ؼ� ����
    public bool PlayDialogue()
    {
        if (panel == null)
            return false;

        if (dialogueQueue.Count <= 0)
        {
            // ������ִµ� ��ȭ���̾��ٸ� ����
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

    // ť ������ Ȯ��
    public bool QueueEmpty()
    {
        if (dialogueQueue.Count <= 0)
            return true;

        return false;
    }

}
