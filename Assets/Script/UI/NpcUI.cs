using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum QuestStatus
{
    None,           // 아님
    Available,      // 받을 수 있음
    Completeable    // 완료할 수 있음
}

public class NpcUI : MonoBehaviour
{
    // Npc 컴포넌트
    private Npc npc;

    // npc 이름 Text
    [SerializeField]
    private string nameTextName = "NpcName";
    private TextMeshProUGUI nameText;
    [SerializeField]
    private Vector3 NameOffset;
    private RectTransform rectTransform;

    [SerializeField]
    private Image questIcon;

    private Canvas canvas;
    private Sprite questStartIcon;
    private Sprite questFinishIcon;


    private void Awake()
    {
        nameText = transform.FindChildByName(nameTextName).GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        canvas = GetComponent<Canvas>();
        questStartIcon = Resources.Load<Sprite>("QuestStartIcon");
        questFinishIcon = Resources.Load<Sprite>("QuestFinishIcon");
    }

    void Start()
    {
        // rootObject 초기화
        GetCharacterRoot();

        // text 이름으로 초기화
        nameText.text = npc.NpcName;

        Vector3 newPosition = rectTransform.position;
        newPosition += NameOffset;
        rectTransform.position = newPosition;
    }

    private void Update()
    {
        canvas.transform.rotation = Camera.main.transform.rotation;
    }

    public void UpdateQuestSprite(QuestStatus questStatus)
    {
        if (questIcon == null)
            return;

        switch(questStatus)
        {
            case QuestStatus.None:
                questIcon.color = new Color(1, 1, 1, 0);
                break;
            case QuestStatus.Available:
                questIcon.color = new Color(1, 1, 1, 1);
                questIcon.sprite = questStartIcon;
                break;
            case QuestStatus.Completeable:
                questIcon.color = new Color(1, 1, 1, 1);
                questIcon.sprite = questFinishIcon;
                break;
        }
    }

    // 부모를 탐색하면서 부모 오브젝트를 찾음
    private void GetCharacterRoot()
    {
        Transform current = transform;

        // 상위 부모가 없을 때까지 탐색
        while (current != null)
        {
            // 부모 객체가 Npc 컴포넌트를 가지고 있는지 확인
            Npc npcComponent = current.GetComponent<Npc>();

            if (npcComponent != null)
            {
                // Npc 컴포넌트 등록
                npc = npcComponent;
                return;
            }

            // 부모로 올라감
            current = current.parent;
        }
    }



}
