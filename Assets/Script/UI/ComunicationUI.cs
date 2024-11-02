using TMPro;
using UnityEngine;

public class ComunicationUI : MonoBehaviour
{
    // ��ȭâ �ǳ�
    [SerializeField]
    private GameObject panel1;

    // ��ȭ���� NPC Name
    [SerializeField]
    private TextMeshProUGUI npcNameText;

    // ��ȭ ����
    [SerializeField]
    private TextMeshProUGUI dialogueText;

    public bool isTalk;

    private void Start()
    {
        // ���� ���� �� �ǳ� ���ü� off
        panel1.SetActive(false);
    }

    // �Ű������� string�� �޾Ƽ� text�� ������Ʈ�ϴ� �޼���
    public void UpdateDialogue(string npcName, string dialogue)
    {
        // ��ȭâ�� Ȱ��ȭ
        panel1.SetActive(true);
        isTalk = true;

        // NPC �̸� ������Ʈ
        npcNameText.text = npcName;

        // ��ȭ ���� ������Ʈ
        dialogueText.text = dialogue;
    }

    // ��ȭâ�� �ݴ� �޼���
    public void CloseDialogue()
    {
        // ��ȭâ�� ��Ȱ��ȭ
        panel1.SetActive(false);
        isTalk = false;
    }
}
