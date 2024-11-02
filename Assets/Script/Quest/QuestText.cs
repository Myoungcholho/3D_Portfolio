using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestText : MonoBehaviour
{
    [Header("자식 Check Img")]
    [SerializeField]
    private Image image;

    private TextMeshProUGUI text;

    // 퀘스트 참조, CheckMark가 활성이 되면
    // AlertManager에 정보 전달하기 위함
    public Quest Quest { get; set; }
    private QuestAlretManager alretManager;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        alretManager = FindObjectOfType<QuestAlretManager>();
        if (alretManager == null)
            Debug.Log("alretManager 없음");
    }

    void Start()
    {
        if (image == null)
        {
            Debug.Log("CheckMark.cs Img is null");
        }

        image.enabled = Quest.isAlret;
    }

    // Button에서 호출함
    public void OnChangeCheckMark()
    {
        Quest.isAlret = !Quest.isAlret;
        image.enabled = Quest.isAlret;

        // 퀘스트 알람 처리
        if(Quest.isAlret == true)
        {
            alretManager.DisplayQuestAlert(Quest);
        }
        // 알람 비활성 처리
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
