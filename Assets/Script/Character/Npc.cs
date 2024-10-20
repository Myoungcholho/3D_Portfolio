using UnityEngine;

public class Npc : MonoBehaviour, IInteractable
{
    // NPC�� �̸�
    [SerializeField]
    private string npcName = "None";
    public string NpcName { get { return npcName; } }

    // NPC�� ��ȭ ������ �������� Ȯ��
    [SerializeField]
    private bool canTalk = true;

    // ��ȭ�� ������ UI
    [SerializeField]
    private GameObject comunicationPanel;
    

    // ��ȭ string
    [SerializeField]
    private string[] dialogueLines = { "�ȳ��ϼ���.", "������ ���͵帱���?" };

    // ��ȣ�ۿ� �������̽�, �켱���� ��ȯ
    public int GetPriority()
    {
        return (int)InteractionPriority.NpcComunication;
    }

    // ��ȣ�ۿ� �� ������ �޼���
    public void Interact()
    {
        // ��ȭ ����


        // 
    }
}
