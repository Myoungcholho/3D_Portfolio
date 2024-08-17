using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum WeaponType
{
    Unarmed = 0, Fist, Sword, Hammer, FireBall,DualSword, BossHammer, Max,
}

public class WeaponComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject[] originPrefabs;


    private Animator animator;
    private StateComponent state;
    private TargetComponent target;

    private WeaponType type = WeaponType.Unarmed;
    public WeaponType Type { get => type; }

    public event Action<WeaponType, WeaponType> OnWeaponTyeChanged;
    public event Action OnEndEquip;
    public event Action OnEndDoAction;
    public event Action<bool> OnDodgeAttack;


    public bool UnarmedMode { get => type == WeaponType.Unarmed; }
    public bool FistMode { get => type == WeaponType.Fist; }
    public bool SwordMode { get => type == WeaponType.Sword; }
    public bool HammerMode { get => type == WeaponType.Hammer; }
    public bool FireBallMode { get => type == WeaponType.FireBall; }
    public bool DualSwordMode { get=> type == WeaponType.DualSword; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        target = GetComponent<TargetComponent>();
    }

    private Dictionary<WeaponType, Weapon> weaponTable;

    private void Start()
    {
        weaponTable = new Dictionary<WeaponType, Weapon>();

        for (int i = 0; i < (int)WeaponType.Max; i++)
            weaponTable.Add((WeaponType)i, null);


        for (int i = 0; i < originPrefabs.Length; i++)
        {
            GameObject obj = Instantiate<GameObject>(originPrefabs[i], transform);
            Weapon weapon = obj.GetComponent<Weapon>();
            obj.name = weapon.Type.ToString();

            weaponTable[weapon.Type] = weapon;
        }
    }

    public void SetFistMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Fist);
    }

    public void SetSwordMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Sword);
    }

    public void SetHammerMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Hammer);
    }

    public void SetFireBallMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.FireBall);
    }

    public void SetDualSwordMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.DualSword);
    }

    public void SetUnarmedMode()
    {
        if (state.IdleMode == false)
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

        OnWeaponTyeChanged?.Invoke(prevType, type);
    }

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

    // 1. �ִϸ��̼� ����
    // 2. Ÿ����
    // 3. ���¸� Action���� �����ϴ� �޼��� ȣ��
    public void DoAction()
    {
        if (weaponTable[type] == null)
            return;
        if (state.DamagedMode == true)
            return;


        animator.SetBool("IsAction", true);
        target?.TargetSearch();              // Ÿ�� ��Ī
        weaponTable[type].DoAction();
    }


    
    public void DodgedDoAction()
    {
        if (weaponTable[type] == null)
            return;

        // Sword�� ����.
        if (type != WeaponType.Sword)
            return;

        if (state.DodgedAttackMode == true)
            return;

        MovableStopper.Instance.AttackCount++;  // �ݰ��� ���� 1ȸ �߰�
        Sword sword = weaponTable[type] as Sword;
        sword?.DodgedDoAction();
    }

    // Animation�� ���� ȣ��
    private void End_DodgedDoAction()
    {
        Sword sword = weaponTable[type] as Sword;
        sword?.End_DodgedDoAction();
    }

    private void Begin_DoAction()
    {
        weaponTable[type].Begin_DoAction();
    }

    // AI���� Action �� �ǰ� �� ����ȣ�� ������ ��ȣ ���� ���� (07.20)
    public void End_DoAction()
    {
        if (UnarmedMode)
            return;

        animator.SetBool("IsAction", false);
        target?.EndTargeting();
        OnDodgeAttack?.Invoke(false);            // ȸ�� ���� �� �˸�
        weaponTable[type].End_DoAction();
        OnEndDoAction?.Invoke();
    }


    private void Begin_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Combo();
    }

    private void End_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Combo();
    }

    private void Begin_Collision(AnimationEvent e)
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Collision(e);
    }

    private void End_Collision()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Collision();
    }

    // ������ ī�޶� ����ŷ ��� ȣ�� �Լ�
    private void Play_Impulse()
    {
        Melee melee = weaponTable[type] as Melee;
        melee?.Play_Impulse();
    }

    private void Play_DoAction_Particle()
    {
        weaponTable[type].Play_Particle();
    }
}