using TMPro;
using UnityEngine;

public class NpcUI : MonoBehaviour
{
    // Npc 컴포넌트
    private Npc npc;

    // npc 이름 Text
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
        // rootObject 초기화
        GetCharacterRoot();

        // text 이름으로 초기화
        nameText.text = npc.NpcName;
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
