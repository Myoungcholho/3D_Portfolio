using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SkillType2
{
    None,one,two,three,four,Max
}

public class SkillIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SkillType2 skillType = SkillType2.None;

    private GameObject draggedInstance; // �巡�� �� �����Ǵ� ���纻
    private bool isDropped = false;     // ��� ���� ���� ����

    // �θ� ĵ����
    private Canvas parentCanvas;
    private Transform rootTransform;

    public GameObject DraggedInstance => draggedInstance;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        rootTransform = transform.root;

        Debug.Assert(parentCanvas != null);
        Debug.Assert(rootTransform != null);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag ȣ��");

        if (parentCanvas == null)
            return;

        // �巡���� ���纻 ����
        draggedInstance = Instantiate(gameObject, transform.parent);
        draggedInstance.transform.SetParent(rootTransform, false); // Canvas�� �巡�� ���纻 �߰�

        // �ؽ�Ʈ ����
        foreach (Transform child in draggedInstance.transform)
        {
            Destroy(child.gameObject);
        }

        CanvasGroup group = draggedInstance.GetComponent<CanvasGroup>();
        group.alpha = 0.5f; // ���� ����
        group.blocksRaycasts = false; // Raycast ���� (�ٸ� UI�� ��ȣ�ۿ��� ����)

        draggedInstance.transform.SetAsLastSibling(); // ���� �տ� ����ǵ��� ����

        isDropped = false; // �巡�� ���� �� ��� �÷��� �ʱ�ȭ
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedInstance != null)
        {
            // ���콺�� ���� ���纻 �̵�
            RectTransform rect = draggedInstance.GetComponent<RectTransform>();
            rect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDropped == false && draggedInstance != null)
        {
            // ����� ������ ��� ���纻�� ����
            Destroy(draggedInstance);
        }
    }

    // ��� ���� ���� ����
    public void SetDropped(bool dropped)
    {
        isDropped = dropped;
    }

}
