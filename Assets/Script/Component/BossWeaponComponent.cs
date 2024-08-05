using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class BossWeaponComponent : MonoBehaviour
{
	[SerializeField]
	private GameObject[] originPrefabs;

	private Animator animator;
	private StateComponent state;

	private WeaponType type = WeaponType.Unarmed;
	public WeaponType Type { get => type; }

	public event Action<WeaponType, WeaponType> OnWeaponTyeChanged;
	public event Action OnEndEquip;
	public event Action OnEndDoAction;

	public bool UnarmedMode { get => type == WeaponType.Unarmed; }
	public bool BossHammerMode { get=> type == WeaponType.BossHammer; }

	private void Awake()
	{
		animator = GetComponent<Animator>();
		state = GetComponent<StateComponent>();
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

	public void SetUnarmedMode()
	{
		if (state.IdleMode == false)
			return;

		animator.SetInteger("WeaponType", (int)WeaponType.Unarmed);

		if (weaponTable[type] != null)
			weaponTable[type].UnEquip();


		ChangeType(WeaponType.Unarmed);
	}

	public void SetBossHammerMode()
	{
		if (state.IdleMode == false)
			return;

		SetMode(WeaponType.BossHammer);
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

	// Boss용 ..
	public void DoAction(int pattern)
	{
		if (weaponTable[type] == null)
			return;
		if (type != WeaponType.BossHammer)
			return;

		Melee melee = weaponTable[type] as Melee;
		melee.Index = pattern;

        animator.SetBool("IsAction", true);
		animator.SetInteger("Pattern", pattern);
	}

/*	private void Begin_DoAction()
	{
		weaponTable[type].Begin_DoAction();
	}*/


	// 공격이 끝나는 경우 호출함.
	public void End_DoAction_Boss()
	{
		animator.SetBool("IsAction", false);
		weaponTable[type].End_DoAction();

		OnEndDoAction?.Invoke();
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

	// 무기의 카메라 쉐이킹 기능 호출 함수
	private void Play_Impulse()
	{
		Melee melee = weaponTable[type] as Melee;
		melee?.Play_Impulse();
	}

    private void Play_DoAction_Particle(AnimationEvent e)
	{
		weaponTable[type].Play_Particle_Index(e);
	}

	private void Play_DoAction_Telpo()
	{

	}
}