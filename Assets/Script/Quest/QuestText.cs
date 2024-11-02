using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestText : MonoBehaviour
{
    [Header("�ڽ� Check Img")]
    [SerializeField]
    private Image image;

    private TextMeshProUGUI text;

    // ����Ʈ ����, CheckMark�� Ȱ���� �Ǹ�
    // AlertManager�� ���� �����ϱ� ����
    public Quest Quest { get; set; }
    private QuestAlretManager alretManager;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        alretManager = FindObjectOfType<QuestAlretManager>();
        if (alretManager == null)
            Debug.Log("alretManager ����");
    }

    void Start()
    {
        if (image == null)
        {
            Debug.Log("CheckMark.cs Img is null");
        }

        image.enabled = Quest.isAlret;
    }

    // Button���� ȣ����
    public void OnChangeCheckMark()
    {
        Quest.isAlret = !Quest.isAlret;
        image.enabled = Quest.isAlret;

        // ����Ʈ �˶� ó��
        if(Quest.isAlret == true)
        {
            alretManager.DisplayQuestAlert(Quest);
        }
        // �˶� ��Ȱ�� ó��
        else
        {
            alretManager.HideQuestAlert(Quest);
        }
    }

    public void UpdateQuestText(string str)
    {
        text.text = str;
    }
}
