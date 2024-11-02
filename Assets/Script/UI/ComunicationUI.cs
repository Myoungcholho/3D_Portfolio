using TMPro;
using UnityEngine;

public class ComunicationUI : MonoBehaviour
{
    // 대화창 판넬
    [SerializeField]
    private GameObject panel1;

    // 대화중인 NPC Name
    [SerializeField]
    private TextMeshProUGUI npcNameText;

    // 대화 내용
    [SerializeField]
    private TextMeshProUGUI dialogueText;

    public bool isTalk;

    private void Start()
    {
        // 게임 시작 시 판넬 가시성 off
        panel1.SetActive(false);
    }

    // 매개변수로 string을 받아서 text를 업데이트하는 메서드
    public void UpdateDialogue(string npcName, string dialogue)
    {
        // 대화창을 활성화
        panel1.SetActive(true);
        isTalk = true;

        // NPC 이름 업데이트
        npcNameText.text = npcName;

        // 대화 내용 업데이트
        dialogueText.text = dialogue;
    }

    // 대화창을 닫는 메서드
    public void CloseDialogue()
    {
        // 대화창을 비활성화
        panel1.SetActive(false);
        isTalk = false;
    }
}
