using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private string playerName = "Player";
    private GameObject player;

    private WeaponComponent weapon;
    private IWeaponCoolTime iWeapon;

    public SkillIcon[] originalSkillIcon;                 // 입력 들어온 스킬아이콘
    private GameObject[] draggedInstance;                 // 할당된 아이콘 오브젝트
    private GameObject[] shadowInstance;                  // 아이콘 오브젝트 ShadowImage

    private void Awake()
    {
        originalSkillIcon = new SkillIcon[(int)WeaponType.Max];
        draggedInstance = new GameObject[(int)WeaponType.Max];
        shadowInstance = new GameObject[(int)WeaponType.Max];

        player = GameObject.Find(playerName);
        weapon = player.GetComponent<WeaponComponent>();

        // 무기 변경 시 슬롯 아이콘 변경 ( 일단 초기화로 )
        weapon.OnWeaponTypeChanged += OnChangeWeapon;

        Debug.Assert(player != null);
        Debug.Assert(weapon != null);
    }

    private void Update()
    {
        if (weapon == null)
            return;

        iWeapon = weapon.GetCurrentWeapon() as IWeaponCoolTime;

        if (iWeapon == null)
            return;

        if (originalSkillIcon[(int)weapon.Type] == null)
            return;

        SkillType2 type = originalSkillIcon[(int)weapon.Type].skillType;

        float maxCool = iWeapon.GetSkillCooldown(type);
        float currCool = iWeapon.GetSkillCoolRemaining(type);

        if (shadowInstance[(int)weapon.Type] != null)
        {
            Image shadowImage = shadowInstance[(int)weapon.Type].GetComponent<Image>();
            shadowImage.fillAmount = currCool / maxCool;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 만약 드래그 한 곳에 이미 아이콘이 있다면 기존 아이콘 정보 삭제
        if(originalSkillIcon[(int)weapon.Type] != null)
        {
            Destroy(draggedInstance[(int)weapon.Type]);
            Destroy(shadowInstance[(int)weapon.Type]);
            
            originalSkillIcon[(int)weapon.Type] = null;
            draggedInstance[(int)weapon.Type] = null;
            shadowInstance[(int)weapon.Type] = null;
        }

        // 아이콘의 정보 얻어옴
        originalSkillIcon[(int)weapon.Type] = eventData.pointerDrag.GetComponent<SkillIcon>();
        if (originalSkillIcon[(int)weapon.Type] == null)
            return;
        if (originalSkillIcon[(int)weapon.Type].DraggedInstance == null)
            return;

        // 드롭 성공 플래그 설정
        originalSkillIcon[(int)weapon.Type].SetDropped(true);

        // 복사본의 부모를 현재 슬롯으로 설정
        draggedInstance[(int)weapon.Type] = originalSkillIcon[(int)weapon.Type].DraggedInstance;
        draggedInstance[(int)weapon.Type].transform.SetParent(transform);

        // 스킬 알파값 다시 올리기
        CanvasGroup group = draggedInstance[(int)weapon.Type].GetComponent<CanvasGroup>();
        group.alpha = 1.0f;

        // RectTransform 설정을 통해 앵커 프리셋을 Stretch로 설정
        // 부모와 같은 앵커 및 피벗 설정
        // 위치 초기화
        RectTransform rectTransform = draggedInstance[(int)weapon.Type].GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero; // 상하좌우 여백 0으로 설정
        rectTransform.offsetMax = Vector2.zero; // 상하좌우 여백 0으로 설정
        rectTransform.pivot = new Vector2(0.5f, 0.5f); // 피벗을 가운데로 설정

        rectTransform.anchoredPosition = Vector2.zero;

        // Shadow 이미지를 만들고 자식으로 붙임
        CreateShadowImage();
    }

    private void CreateShadowImage()
    {
        // 쉐도우를 만들어 자식으로 추가
        shadowInstance[(int)weapon.Type] = new GameObject("ShadowImage");
        shadowInstance[(int)weapon.Type].transform.SetParent(transform, false);
        Image shadowImage = shadowInstance[(int)weapon.Type].AddComponent<Image>();
        shadowImage.type = Image.Type.Filled;

        // 원본 이미지의 스프라이트 복사
        Image originalImage = draggedInstance[(int)weapon.Type].GetComponent<Image>();
        shadowImage.sprite = originalImage.sprite;
        shadowImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        // Shadow 이미지 RectTransform 설정
        RectTransform shadowRectTransform = shadowInstance[(int)weapon.Type].GetComponent<RectTransform>();
        shadowRectTransform.anchorMin = new Vector2(0, 0);
        shadowRectTransform.anchorMax = new Vector2(1, 1);
        shadowRectTransform.offsetMin = Vector2.zero;
        shadowRectTransform.offsetMax = Vector2.zero;
        shadowRectTransform.pivot = new Vector2(0.5f, 0.5f);
        shadowRectTransform.anchoredPosition = Vector2.zero;
    }

    private void OnChangeWeapon(WeaponType prev, WeaponType next)
    {
        ClearSlot();
        // 다음 무기에 할당된 스킬 아이콘이 있는지 확인하고, 있으면 슬롯에 적용
        if (draggedInstance[(int)next] == null)
            return;
        if (originalSkillIcon[(int)next] == null)
            return;

        // 저장돤 아이콘 불러들이기
        {
            draggedInstance[(int)next].SetActive(true);
            shadowInstance[(int)next].SetActive(true);
        }
    }

    // 모든 스킬 아이콘 비활성화
    private void ClearSlot()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
