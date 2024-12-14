using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public struct SkillInfo
{
    public Sprite skill_Image;                      // ��ų ������ �Ǵ� �̹��� �迭
    [TextArea] public string skillExplanation;      // ��ų ����
}

/// <summary>
/// ��ųâ �ǳ� UI, ���� ���� �����ִ� ��ų ������ �ٸ�
/// </summary>
public class SkillUIRenewal : MonoBehaviour
{
    [SerializeField]
    private string playerName = "Player";

    [SerializeField]
    private Image[] skill_Image;
    [SerializeField]
    private TextMeshProUGUI[] skill_text;

    // �÷��̾� ����
    private GameObject player;
    private WeaponComponent weapon;

    // �ǳ� Ȱ�� ����
    [SerializeField]
    private GameObject panel;
    private bool bActive;

    [SerializeField]
    private SkillInfo[] fist;
    [SerializeField]
    private SkillInfo[] sword;
    [SerializeField]
    private SkillInfo[] hammer;
    [SerializeField]
    private SkillInfo[] staff;
    [SerializeField]
    private SkillInfo[] dual;

    // Ŀ��
    private CursorComponent cursorComponent;

    private void Awake()
    {
        player = GameObject.Find(playerName);
        weapon = player.GetComponent<WeaponComponent>();
        cursorComponent = player.GetComponent<CursorComponent>();

        weapon.OnWeaponTypeChanged += OnChagedWeapon;


        Debug.Assert(player != null);
        Debug.Assert(weapon != null);


        PlayerInput input = player.GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        actionMap.FindAction("SkillPanel").started += context =>
        {
            IsActive();
        };
    }

    private void Start()
    {
        bActive = false;
        panel.SetActive(bActive);

        if (weapon.Type == WeaponType.Unarmed)
        {
            foreach (var image in skill_Image)
            {
                image.gameObject.SetActive(false);
            }
            foreach (var text in skill_text)
            {
                text.gameObject.SetActive(false);
            }
        }
    }

    // ���� ��ȯ�ɶ����� ��ų �ǳ� ����
    private void OnChagedWeapon(WeaponType prev, WeaponType curr)
    {
        if (curr == WeaponType.Unarmed)
        {
            // skill_image ���� ��Ȱ��ȭ
            foreach (var image in skill_Image)
            {
                image.gameObject.SetActive(false);
            }

            // skill_text ���� ��Ȱ��ȭ
            foreach (var text in skill_text)
            {
                text.gameObject.SetActive(false);
            }
            return;
        }

        // skill_image ���� Ȱ��ȭ
        foreach (var image in skill_Image)
        {
            image.gameObject.SetActive(true);
        }

        // skill_text ���� Ȱ��ȭ
        foreach (var text in skill_text)
        {
            text.gameObject.SetActive(true);
        }

        SkillInfo[] selectedSkillInfo = null;

        // �� ���� Ÿ�Կ� ���� ��ų ���� ����
        if (curr == WeaponType.Fist)
        {
            selectedSkillInfo = fist;
        }
        else if (curr == WeaponType.Sword)
        {
            selectedSkillInfo = sword;
        }
        else if (curr == WeaponType.Hammer)
        {
            selectedSkillInfo = hammer;
        }
        else if (curr == WeaponType.FireBall)
        {
            selectedSkillInfo = staff;
        }
        else if (curr == WeaponType.DualSword)
        {
            selectedSkillInfo = dual;
        }

        // ���õ� ��ų ������ ���� UI ������Ʈ
        for (int i = 0; i < skill_Image.Length; i++)
        {
            if (i >= selectedSkillInfo.Length)
            {
                skill_Image[i].gameObject.SetActive(false);
                skill_text[i].gameObject.SetActive(false);
                continue;
            }

            skill_Image[i].sprite = selectedSkillInfo[i].skill_Image;
            skill_text[i].text = selectedSkillInfo[i].skillExplanation;
        }

    }

    private void IsActive()
    {
        bActive = !bActive;
        panel.SetActive(bActive);

        if(bActive == true)
        {
            cursorComponent.ShowCursorForUI();
            return;
        }
        cursorComponent.HideCursorForUI();
    }

}
