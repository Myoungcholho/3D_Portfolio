using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public struct SkillInfo
{
    public Sprite skill_Image;                      // 스킬 아이콘 또는 이미지 배열
    [TextArea] public string skillExplanation;      // 스킬 설명
}

/// <summary>
/// 스킬창 판넬 UI, 무기 마다 보여주는 스킬 아이콘 다름
/// </summary>
public class SkillUIRenewal : MonoBehaviour
{
    [SerializeField]
    private string playerName = "Player";

    [SerializeField]
    private Image[] skill_Image;
    [SerializeField]
    private TextMeshProUGUI[] skill_text;

    // 플레이어 정보
    private GameObject player;
    private WeaponComponent weapon;

    // 판넬 활성 유무
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

    // 커서
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

    // 무기 변환될때마다 스킬 판넬 변경
    private void OnChagedWeapon(WeaponType prev, WeaponType curr)
    {
        if (curr == WeaponType.Unarmed)
        {
            // skill_image 전부 비활성화
            foreach (var image in skill_Image)
            {
                image.gameObject.SetActive(false);
            }

            // skill_text 전부 비활성화
            foreach (var text in skill_text)
            {
                text.gameObject.SetActive(false);
            }
            return;
        }

        // skill_image 전부 활성화
        foreach (var image in skill_Image)
        {
            image.gameObject.SetActive(true);
        }

        // skill_text 전부 활성화
        foreach (var text in skill_text)
        {
            text.gameObject.SetActive(true);
        }

        SkillInfo[] selectedSkillInfo = null;

        // 각 무기 타입에 따른 스킬 정보 선택
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

        // 선택된 스킬 정보에 따라 UI 업데이트
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
