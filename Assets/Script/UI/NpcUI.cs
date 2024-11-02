using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum QuestStatus
{
    None,           // �ƴ�
    Available,      // ���� �� ����
    Completeable    // �Ϸ��� �� ����
}

public class NpcUI : MonoBehaviour
{
    // Npc ������Ʈ
    private Npc npc;

    // npc �̸� Text
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
        // rootObject �ʱ�ȭ
        GetCharacterRoot();

        // text �̸����� �ʱ�ȭ
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

    // �θ� Ž���ϸ鼭 �θ� ������Ʈ�� ã��
    private void GetCharacterRoot()
    {
        Transform current = transform;

        // ���� �θ� ���� ������ Ž��
        while (current != null)
        {
            // �θ� ��ü�� Npc ������Ʈ�� ������ �ִ��� Ȯ��
            Npc npcComponent = current.GetComponent<Npc>();

            if (npcComponent != null)
            {
                // Npc ������Ʈ ���
                npc = npcComponent;
                return;
            }

            // �θ�� �ö�
            current = current.parent;
        }
    }



}
