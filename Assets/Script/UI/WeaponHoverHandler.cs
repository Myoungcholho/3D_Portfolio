using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponHoverHandler : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    public WeaponType type;   // ���� �̸� �Ǵ� ID
    public Image image;         // ���콺 ���� �÷��θ� �ٲ� �̹���
    private WeaponChangeUI weaponChangeUI;

    [SerializeField]
    private Color activeImageColor = new Color(0.5f, 0.5f, 0.5f, 1);
    private Color originColor;

    private void Start()
    {
        weaponChangeUI = FindObjectOfType<WeaponChangeUI>();
        Debug.Assert(weaponChangeUI != null);

        if (image == null)
            return;

        originColor = image.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = activeImageColor;
        weaponChangeUI.SetSelectedWeapon(type);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = originColor;
        weaponChangeUI.SetSelectedWeapon(WeaponType.Unarmed);
    }
}
