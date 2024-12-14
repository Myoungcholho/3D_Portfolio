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

    public SkillIcon[] originalSkillIcon;                 // �Է� ���� ��ų������
    private GameObject[] draggedInstance;                 // �Ҵ�� ������ ������Ʈ
    private GameObject[] shadowInstance;                  // ������ ������Ʈ ShadowImage

    private void Awake()
    {
        originalSkillIcon = new SkillIcon[(int)WeaponType.Max];
        draggedInstance = new GameObject[(int)WeaponType.Max];
        shadowInstance = new GameObject[(int)WeaponType.Max];

        player = GameObject.Find(playerName);
        weapon = player.GetComponent<WeaponComponent>();

        // ���� ���� �� ���� ������ ���� ( �ϴ� �ʱ�ȭ�� )
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
        // ���� �巡�� �� ���� �̹� �������� �ִٸ� ���� ������ ���� ����
        if(originalSkillIcon[(int)weapon.Type] != null)
        {
            Destroy(draggedInstance[(int)weapon.Type]);
            Destroy(shadowInstance[(int)weapon.Type]);
            
            originalSkillIcon[(int)weapon.Type] = null;
            draggedInstance[(int)weapon.Type] = null;
            shadowInstance[(int)weapon.Type] = null;
        }

        // �������� ���� ����
        originalSkillIcon[(int)weapon.Type] = eventData.pointerDrag.GetComponent<SkillIcon>();
        if (originalSkillIcon[(int)weapon.Type] == null)
            return;
        if (originalSkillIcon[(int)weapon.Type].DraggedInstance == null)
            return;

        // ��� ���� �÷��� ����
        originalSkillIcon[(int)weapon.Type].SetDropped(true);

        // ���纻�� �θ� ���� �������� ����
        draggedInstance[(int)weapon.Type] = originalSkillIcon[(int)weapon.Type].DraggedInstance;
        draggedInstance[(int)weapon.Type].transform.SetParent(transform);

        // ��ų ���İ� �ٽ� �ø���
        CanvasGroup group = draggedInstance[(int)weapon.Type].GetComponent<CanvasGroup>();
        group.alpha = 1.0f;

        // RectTransform ������ ���� ��Ŀ �������� Stretch�� ����
        // �θ�� ���� ��Ŀ �� �ǹ� ����
        // ��ġ �ʱ�ȭ
        RectTransform rectTransform = draggedInstance[(int)weapon.Type].GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero; // �����¿� ���� 0���� ����
        rectTransform.offsetMax = Vector2.zero; // �����¿� ���� 0���� ����
        rectTransform.pivot = new Vector2(0.5f, 0.5f); // �ǹ��� ����� ����

        rectTransform.anchoredPosition = Vector2.zero;

        // Shadow �̹����� ����� �ڽ����� ����
        CreateShadowImage();
    }

    private void CreateShadowImage()
    {
        // �����츦 ����� �ڽ����� �߰�
        shadowInstance[(int)weapon.Type] = new GameObject("ShadowImage");
        shadowInstance[(int)weapon.Type].transform.SetParent(transform, false);
        Image shadowImage = shadowInstance[(int)weapon.Type].AddComponent<Image>();
        shadowImage.type = Image.Type.Filled;

        // ���� �̹����� ��������Ʈ ����
        Image originalImage = draggedInstance[(int)weapon.Type].GetComponent<Image>();
        shadowImage.sprite = originalImage.sprite;
        shadowImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        // Shadow �̹��� RectTransform ����
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
        // ���� ���⿡ �Ҵ�� ��ų �������� �ִ��� Ȯ���ϰ�, ������ ���Կ� ����
        if (draggedInstance[(int)next] == null)
            return;
        if (originalSkillIcon[(int)next] == null)
            return;

        // ����� ������ �ҷ����̱�
        {
            draggedInstance[(int)next].SetActive(true);
            shadowInstance[(int)next].SetActive(true);
        }
    }

    // ��� ��ų ������ ��Ȱ��ȭ
    private void ClearSlot()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
