using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    // NPC의 이름
    [SerializeField]
    private string npcName = "None";
    public string NpcName { get { return npcName; } }

    // NPC와 대화 가능한 상태인지 확인
    [SerializeField]
    private bool canTalk = true;

    // 대화를 보여줄 UI
    [SerializeField]
    private GameObject comunicationPanel;
    

    // 대화 string
    [SerializeField]
    private string[] dialogueLines = { "안녕하세요.", "무엇을 도와드릴까요?" };

    // 상호작용 인터페이스, 우선순위 반환
    public int GetPriority()
    {
        return (int)InteractionPriority.NpcComunication;
    }

    // 상호작용 시 실행할 메서드
    public void Interact()
    {
        // 대화 실행


        // 
    }
}
