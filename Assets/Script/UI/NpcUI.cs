using TMPro;
using UnityEngine;

public class NpcUI : MonoBehaviour
{
    // Npc ������Ʈ
    private Npc npc;

    // npc �̸� Text
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private string nameTextName = "NpcName";


    private void Awake()
    {
        nameText = transform.FindChildByName(nameTextName).GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        // rootObject �ʱ�ȭ
        GetCharacterRoot();

        // text �̸����� �ʱ�ȭ
        nameText.text = npc.NpcName;
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
