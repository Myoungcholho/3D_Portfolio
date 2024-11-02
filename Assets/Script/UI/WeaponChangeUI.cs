using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponChangeUI : MonoBehaviour
{
    [SerializeField]
    private GameObject weaponChangePanel;

    private string playerName = "Player";
    private WeaponComponent weapon;

    private Dictionary<WeaponType, System.Action> weaponActions;
    private WeaponType selectedWeapon = WeaponType.Unarmed;

    private void Awake()
    {
        GameObject obj = GameObject.Find(playerName);
        Debug.Assert(obj != null);

        weapon = obj.GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);

        weaponActions = new Dictionary<WeaponType, System.Action>
        {
            { WeaponType.Fist, () => weapon.SetFistMode() },
            { WeaponType.Sword, () => weapon.SetSwordMode() },
            { WeaponType.Hammer, () => weapon.SetHammerMode() },
            { WeaponType.FireBall, () => weapon.SetFireBallMode() },
            { WeaponType.DualSword, () => weapon.SetDualSwordMode() }
        };
    }

    private void Start()
    {
        weaponChangePanel.SetActive(false);
    }

    public void ToggleUIPanel(bool isActive)
    {
        weaponChangePanel.SetActive(isActive);

        if (isActive == true)
            return;

        if (selectedWeapon == WeaponType.Unarmed)
            return;

        // ���õ� ���� ����
        if (weaponActions.TryGetValue(selectedWeapon, out System.Action equipAction))
        {
            equipAction();
        }

        selectedWeapon = WeaponType.Unarmed;
    }

    // WeaponHoverHandler�� ȣ���ϴ� �޼���
    public void SetSelectedWeapon(WeaponType type)
    {
        selectedWeapon = type;
    }
}