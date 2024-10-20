using System;
using System.Collections.Generic;
using UnityEngine;

// WeaponType enum : ���� �� �پ��� ���� Ÿ�� ����
public enum WeaponType
{
    Unarmed = 0, Fist, Sword, Hammer, FireBall,DualSword, BossHammer, Max,
}

// WeaponComponent class: ĳ������ ���� ���� ��� ��� Ŭ����
public class WeaponComponent : MonoBehaviour
{
    // ���� �������� �����ϴ� �迭 (Unity �����Ϳ��� ����)
    [SerializeField]
    private GameObject[] originPrefabs;

    // ������ �޴� �ֿ� ������Ʈ��
    private Animator animator;
    private StateComponent state;

    // ���� ������ ������ Ÿ���� ����
    private WeaponType type = WeaponType.Unarmed;
    public WeaponType Type { get => type; }

    // ���� �׼� �� ȣ��� �̺�Ʈ��
    public event Action<WeaponType, WeaponType> OnWeaponTypeChanged;
    public event Action OnEndEquip;
    public event Action OnEndDoAction;
    public event Action<bool> OnDodgeAttack;

    // �� ���� Ÿ�Կ� ���� ��� üũ
    public bool UnarmedMode { get => type == WeaponType.Unarmed; }
    public bool FistMode { get => type == WeaponType.Fist; }
    public bool SwordMode { get => type == WeaponType.Sword; }
    public bool HammerMode { get => type == WeaponType.Hammer; }
    public bool FireBallMode { get => type == WeaponType.FireBall; }
    public bool DualSwordMode { get=> type == WeaponType.DualSword; }

    // ���� Ÿ�Կ� ���� ���� ���� �ڷᱸ��
    private Dictionary<WeaponType, Weapon> weaponTable;

    // ���� ������ ���� ������ ��ȯ (���� ��� null ��ȯ)
    public Weapon GetCurrentWeapon() => weaponTable.TryGetValue(Type, out Weapon weapon) ? weapon : null;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
    }

    private void Start()
    {
        // ���� ���̺� �ʱ�ȭ (��� ���� Ÿ�Կ� null�� ����)
        weaponTable = new Dictionary<WeaponType, Weapon>();

        for (int i = 0; i < (int)WeaponType.Max; i++)
            weaponTable.Add((WeaponType)i, null);

        // ���������� ���� ���� �� ���̺� ���
        for (int i = 0; i < originPrefabs.Length; i++)
        {
            GameObject obj = Instantiate<GameObject>(originPrefabs[i], transform);
            Weapon weapon = obj.GetComponent<Weapon>();
            obj.name = weapon.Type.ToString();

            weaponTable[weapon.Type] = weapon;
        }
    }

    // ���� ��带 ����
    #region SetMode
    public void SetFistMode()
    {
        if (!(state.IdleMode || state.SkillCastMode))
            return;

        SetMode(WeaponType.Fist);
    }

    public void SetSwordMode()
    {
        if (!(state.IdleMode || state.SkillCastMode))
            return;

        SetMode(WeaponType.Sword);
    }

    public void SetHammerMode()
    {
        if (!(state.IdleMode || state.SkillCastMode))
            return;

        SetMode(WeaponType.Hammer);
    }

    public void SetFireBallMode()
    {
        if (!(state.IdleMode || state.SkillCastMode))
            return;

        SetMode(WeaponType.FireBall);
    }

    public void SetDualSwordMode()
    {
        if (!(state.IdleMode || state.SkillCastMode))
            return;

        SetMode(WeaponType.DualSword);
    }

    public void SetUnarmedMode()
    {
        if (!(state.IdleMode || state.SkillCastMode))
            return;


        animator.SetInteger("WeaponType", (int)WeaponType.Unarmed);

        if (weaponTable[type] != null)
            weaponTable[type].UnEquip();


        ChangeType(WeaponType.Unarmed);
    }

    public bool IsEquippingMode()
    {
        if (UnarmedMode)
            return false;

        Weapon weapon = weaponTable[type];
        if (weapon == null)
            return false;

        return weapon.Equipping;
    }

    private void SetMode(WeaponType type)
    {
        if (state.DamagedMode == true)
            return;

        if (this.type == type)
        {
            SetUnarmedMode();

            return;
        }
        else if (UnarmedMode == false)
        {
            weaponTable[this.type].UnEquip();
        }

        if (weaponTable[type] == null)
        {
            SetUnarmedMode();

            return;
        }


        animator.SetBool("IsEquipping", true);
        animator.SetInteger("WeaponType", (int)type);

        weaponTable[type].Equip();


        ChangeType(type);
    }

    private void ChangeType(WeaponType type)
    {
        if (this.type == type)
            return;


        WeaponType prevType = this.type;
        this.type = type;

        OnWeaponTypeChanged?.Invoke(prevType, type);
    }
    #endregion

    // �ִϸ��̼� �̺�Ʈ�� ȣ��Ǵ� ���� ���� ����
    #region Equip Event
    // �ִϸ��̼� �̺�Ʈ�� ���� ȣ��
    public void Begin_Equip()
    {
        weaponTable[type].Begin_Equip();
    }

    public void End_Equip()
    {
        animator.SetBool("IsEquipping", false);

        weaponTable[type].End_Equip();
        OnEndEquip?.Invoke();
    }
    #endregion

    // ���� �׼� ó��
    #region Action

    // ���� �׼� ó�� (���콺 ��Ŭ�� �� ȣ��)
    public void DoAction()
    {
        // �����ϱ� ���� ���� üũ
        if (weaponTable[type] == null)
            return;
        if (state.DamagedMode == true)
            return;
        if (state.UsingSkillMode == true)
            return;
        if (state.DeadMode == true)
            return;

        animator.SetBool("IsAction", true);             // �ִϸ��̼� ���� ��ȯ
        weaponTable[type].DoAction();                   // Weapon�� Action() ȣ��
    }

    // ȸ�� ���¿��� ���� �׼� ó�� (���콺 ��Ŭ�� �� ȣ��)
    public void DodgedDoAction()
    {
        if (weaponTable[type] == null)
            return;


        if (state.DodgedAttackMode == true)
            return;

        // Sword�� ����
        if (type != WeaponType.Sword)
            return;
        
        weaponTable[type].DodgedDoAction();
    }
    #endregion

    // ��ų �׼� ó��
    #region Skill
    // Q ��ų �׼� ó�� (Q �Է� �� ȣ��)
    public void ActivateQSkill()
    {
        if (weaponTable[type] == null) 
            return;
        if (!(state.IdleMode || state.SkillCastMode))
            return;
        
        weaponTable[type].ActivateQSkill();
    }
    // E ��ų �׼� ó�� (E �Է� �� ȣ��)
    public void ActivateESkill()
    {
        if (weaponTable[type] == null)
            return;
        if (!(state.IdleMode || state.SkillCastMode))
            return;

        weaponTable[type].ActivateESkill();
    }

    // ��ų �׼� ���� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void End_SkillAction()
    {
        animator.SetBool("IsSkillAction", false);
        weaponTable[type].End_SkillAction();
    }

    // Q��ų ��ƼŬ ȿ�� ��� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Play_QSkillParticles()
    {
        weaponTable[type].Play_QSkillParticles();
    }

    // E��ų ��ƼŬ ȿ�� ��� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Play_ESkillParticles()
    {
        weaponTable[type].Play_ESkillParticles();
    }

    #region Skill Camera Cinematics
    private void Begin_Skill01VCam()
    {
        weaponTable[type].Begin_Skill01VCam();
    }

    
    private void End_Skill01VCam()
    {
        weaponTable[type].End_Skill01VCam();
    }


    private void Begin_Skill02VCam()
    {
        weaponTable[type].Begin_Skill02VCam();
    }
    private void End_Skill02VCam()
    {
        weaponTable[type].End_Skill02VCam();
    }
    #endregion

    #endregion

    // �ִϸ��̼� Event ȣ�� ��� ó��
    #region Animation Events
    // �ִϸ��̼� �̺�Ʈ�� ȸ�� ���� �׼� ����
    private void End_DodgedDoAction()
    {
        if (SwordMode == false)
            return;

        weaponTable[type].End_DodgedDoAction(); // ȸ�� ���� ����
    }

    // �ִϸ��̼� �̺�Ʈ�� �׼� ���� (AI���� ȣ��)
    private void Begin_DoAction()
    {
        weaponTable[type].Begin_DoAction();     // ���� ���� �׼� ����
    }

    // ���� �׼� ���� ó��
    // AI���� Action �� �ǰ� �� ����ȣ�� ������ ��ȣ ���� ���� (07.20)
    public void End_DoAction()
    {
        if (UnarmedMode)
            return;

        animator.SetBool("IsAction", false);    // �׼� �ִϸ��̼� ���� ����
        OnDodgeAttack?.Invoke(false);           // ȸ�� ���� ���� �̺�Ʈ ȣ��
        weaponTable[type].End_DoAction();       // ���� ���� �׼� ����
        OnEndDoAction?.Invoke();                // �׼� ���� �̺�Ʈ ȣ��
    }

    // �޺� ���� ���� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Begin_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Combo();
    }

    // �޺� ���� ���� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void End_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Combo();
    }

    // ���� �浹 ���� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Begin_Collision(AnimationEvent e)
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Collision(e);
    }

    // ���� �浹 ���� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void End_Collision()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Collision();
    }

    // ������ ī�޶� ����ŷ ȿ�� ���� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Play_Impulse()
    {
        Melee melee = weaponTable[type] as Melee;
        melee?.Play_Impulse();
    }

    // ���� �׼� �� ��ƼŬ ȿ�� ��� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Play_DoAction_Particle()
    {
        weaponTable[type].Play_Particle();
    }

    // ���� ���� ��� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Play_Sound()
    {
        if (state.DeadMode == true)
            return;

        weaponTable[type].Play_Sound();
    }

    // ���� ������ ���� ���� Collider�� ���� �ʴ� ��� ȣ�� (�ִϸ��̼� �̺�Ʈ�� ȣ���)
    private void Create_Attack_Collision()
    {
        weaponTable[type].Create_Attack_Collision();
    }
    #endregion
}