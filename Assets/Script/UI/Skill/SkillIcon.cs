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

    private GameObject draggedInstance; // 드래그 시 생성되는 복사본
    private bool isDropped = false;     // 드롭 성공 여부 추적

    // 부모 캔버스
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
        Debug.Log("OnBeginDrag 호출");

        if (parentCanvas == null)
            return;

        // 드래그할 복사본 생성
        draggedInstance = Instantiate(gameObject, transform.parent);
        draggedInstance.transform.SetParent(rootTransform, false); // Canvas에 드래그 복사본 추가

        // 텍스트 삭제
        foreach (Transform child in draggedInstance.transform)
        {
            Destroy(child.gameObject);
        }

        CanvasGroup group = draggedInstance.GetComponent<CanvasGroup>();
        group.alpha = 0.5f; // 투명도 설정
        group.blocksRaycasts = false; // Raycast 차단 (다른 UI와 상호작용을 위해)

        draggedInstance.transform.SetAsLastSibling(); // 가장 앞에 노출되도록 설정

        isDropped = false; // 드래그 시작 시 드롭 플래그 초기화
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedInstance != null)
        {
            // 마우스를 따라 복사본 이동
            RectTransform rect = draggedInstance.GetComponent<RectTransform>();
            rect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDropped == false && draggedInstance != null)
        {
            // 드롭이 실패한 경우 복사본을 제거
            Destroy(draggedInstance);
        }
    }

    // 드롭 성공 여부 설정
    public void SetDropped(bool dropped)
    {
        isDropped = dropped;
    }

}
