using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    public enum SkillType
    {
        FistSkill01,
        FistSkill02,
        SwordSkill01,
        SwordSkill02,
        HammerSkill01,
        HammerSkill02,
        StaffSkill01,
        StaffSkill02,
        DualSkill01,
        DualSkill02,
    }

    public GameObject target;           // Player , (스킬 쿨 보여줄 대상)

    public Text qSkillCooldownText;
    public Text eSkillCooldownText;

    public Image qSkillCooldownImage;
    public Image eSkillCooldownImage;
    public Image qSkillCooldownImage_Shadow;
    public Image eSkillCooldownImage_Shadow;

    private WeaponComponent weaponComponent;
    private IWeaponCoolTime iWeapon;

    private Dictionary<SkillType, Sprite> skillImages;
    [Header("Skill Image")]
    public Sprite fistSkill01Icon;
    public Sprite fistSkill02Icon;

    public Sprite swordSkill01Icon;
    public Sprite swordSkill02Icon;

    public Sprite hammerSkill01Icon;
    public Sprite hammerSkill02Icon;

    public Sprite staffSkill01Icon;
    public Sprite staffSkill02Icon;

    public Sprite dualSkill01Icon;
    public Sprite dualSkill02Icon;

    private void Awake()
    {
        Debug.Assert(target != null);
        if (target == null)
            return;

        weaponComponent = target.GetComponent<WeaponComponent>();
        weaponComponent.OnWeaponTypeChanged += ChangeSkillImage;
    }

    private void Start()
    {
        skillImages = new Dictionary<SkillType, Sprite>
        {
            { SkillType.FistSkill01, fistSkill01Icon },
            { SkillType.FistSkill02, fistSkill02Icon },
            { SkillType.SwordSkill01, swordSkill01Icon },
            { SkillType.SwordSkill02, swordSkill02Icon },
            { SkillType.HammerSkill01, hammerSkill01Icon },
            { SkillType.HammerSkill02, hammerSkill02Icon },
            { SkillType.StaffSkill01, staffSkill01Icon },
            { SkillType.StaffSkill02, staffSkill02Icon },
            { SkillType.DualSkill01, dualSkill01Icon },
            { SkillType.DualSkill02, dualSkill02Icon }
        };
    }

    //여기부터

    void Update()
    {
        if (weaponComponent == null)
            return;

        iWeapon = weaponComponent.GetCurrentWeapon() as IWeaponCoolTime;

        if (iWeapon != null)
        {
            // 쿨타임 업데이트
            float qSkillCooldown = iWeapon.GetQSkillCooldown();
            float qSkillCurrentCool = iWeapon.GetQSkillCoolRemaining();
            
            float eSkillCooldown = iWeapon.GetESkillCooldown();
            float eSkillCurrentCool = iWeapon.GetESkillCoolRemaining();


            // UI 업데이트
            if (qSkillCooldownText != null && eSkillCooldownText != null)
            { 
                qSkillCooldownText.text = qSkillCooldown.ToString("F1");
                eSkillCooldownText.text = eSkillCooldown.ToString("F1");
            }

            // 이미지 업데이트
            if (qSkillCooldownImage != null && eSkillCooldownImage != null)
            {
                qSkillCooldownImage.fillAmount = qSkillCurrentCool / qSkillCooldown;
                eSkillCooldownImage.fillAmount = eSkillCurrentCool / eSkillCooldown;
            }
        }
    }

    private void ChangeSkillImage(WeaponType prev, WeaponType current)
    {
        switch (current)
        {
            case WeaponType.Fist:
                {
                    qSkillCooldownImage.sprite = skillImages[SkillType.FistSkill01];
                    eSkillCooldownImage.sprite = skillImages[SkillType.FistSkill02];

                    qSkillCooldownImage_Shadow.sprite = skillImages[SkillType.FistSkill01];
                    eSkillCooldownImage_Shadow.sprite = skillImages[SkillType.FistSkill02];
                }
                break;
            case WeaponType.Sword:
                {
                    qSkillCooldownImage.sprite = skillImages[SkillType.SwordSkill01];
                    eSkillCooldownImage.sprite = skillImages[SkillType.SwordSkill02];

                    qSkillCooldownImage_Shadow.sprite = skillImages[SkillType.SwordSkill01];
                    eSkillCooldownImage_Shadow.sprite = skillImages[SkillType.SwordSkill02];
                }
                break;
            case WeaponType.Hammer:
                {
                    qSkillCooldownImage.sprite = skillImages[SkillType.HammerSkill01];
                    eSkillCooldownImage.sprite = skillImages[SkillType.HammerSkill02];

                    qSkillCooldownImage_Shadow.sprite = skillImages[SkillType.HammerSkill01];
                    eSkillCooldownImage_Shadow.sprite = skillImages[SkillType.HammerSkill02];
                }
                break;
            case WeaponType.FireBall:
                {
                    qSkillCooldownImage.sprite = skillImages[SkillType.StaffSkill01];
                    eSkillCooldownImage.sprite = skillImages[SkillType.StaffSkill02];

                    qSkillCooldownImage_Shadow.sprite = skillImages[SkillType.StaffSkill01];
                    eSkillCooldownImage_Shadow.sprite = skillImages[SkillType.StaffSkill02];
                }
                break;
            case WeaponType.DualSword:
                {
                    qSkillCooldownImage.sprite = skillImages[SkillType.DualSkill01];
                    eSkillCooldownImage.sprite = skillImages[SkillType.DualSkill02];

                    qSkillCooldownImage_Shadow.sprite = skillImages[SkillType.DualSkill01];
                    eSkillCooldownImage_Shadow.sprite = skillImages[SkillType.DualSkill02];
                }
                break;
        }
    }
}
