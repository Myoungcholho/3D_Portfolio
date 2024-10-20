using System;
using System.Collections.Generic;
using UnityEngine;

// WeaponType enum : 게임 내 다양한 무기 타입 정의
public enum WeaponType
{
    Unarmed = 0, Fist, Sword, Hammer, FireBall,DualSword, BossHammer, Max,
}

// WeaponComponent class: 캐릭터의 무기 관리 기능 담당 클래스
public class WeaponComponent : MonoBehaviour
{
    // 무기 프리팹을 저장하는 배열 (Unity 에디터에서 연결)
    [SerializeField]
    private GameObject[] originPrefabs;

    // 영향을 받는 주요 컴포넌트들
    private Animator animator;
    private StateComponent state;

    // 현재 장착된 무기의 타입을 저장
    private WeaponType type = WeaponType.Unarmed;
    public WeaponType Type { get => type; }

    // 무기 액션 시 호출될 이벤트들
    public event Action<WeaponType, WeaponType> OnWeaponTypeChanged;
    public event Action OnEndEquip;
    public event Action OnEndDoAction;
    public event Action<bool> OnDodgeAttack;

    // 각 무기 타입에 따른 모드 체크
    public bool UnarmedMode { get => type == WeaponType.Unarmed; }
    public bool FistMode { get => type == WeaponType.Fist; }
    public bool SwordMode { get => type == WeaponType.Sword; }
    public bool HammerMode { get => type == WeaponType.Hammer; }
    public bool FireBallMode { get => type == WeaponType.FireBall; }
    public bool DualSwordMode { get=> type == WeaponType.DualSword; }

    // 무기 타입에 따른 무기 저장 자료구조
    private Dictionary<WeaponType, Weapon> weaponTable;

    // 현재 장착된 무기 정보를 반환 (없는 경우 null 반환)
    public Weapon GetCurrentWeapon() => weaponTable.TryGetValue(Type, out Weapon weapon) ? weapon : null;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
    }

    private void Start()
    {
        // 무기 테이블 초기화 (모든 무기 타입에 null로 설정)
        weaponTable = new Dictionary<WeaponType, Weapon>();

        for (int i = 0; i < (int)WeaponType.Max; i++)
            weaponTable.Add((WeaponType)i, null);

        // 프리팹으로 무기 생성 후 테이블에 등록
        for (int i = 0; i < originPrefabs.Length; i++)
        {
            GameObject obj = Instantiate<GameObject>(originPrefabs[i], transform);
            Weapon weapon = obj.GetComponent<Weapon>();
            obj.name = weapon.Type.ToString();

            weaponTable[weapon.Type] = weapon;
        }
    }

    // 무기 모드를 설정
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

    // 애니메이션 이벤트로 호출되는 무기 장착 시작
    #region Equip Event
    // 애니메이션 이벤트에 의해 호출
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

    // 공격 액션 처리
    #region Action

    // 공격 액션 처리 (마우스 좌클릭 시 호출)
    public void DoAction()
    {
        // 공격하기 위한 조건 체크
        if (weaponTable[type] == null)
            return;
        if (state.DamagedMode == true)
            return;
        if (state.UsingSkillMode == true)
            return;
        if (state.DeadMode == true)
            return;

        animator.SetBool("IsAction", true);             // 애니메이션 상태 변환
        weaponTable[type].DoAction();                   // Weapon의 Action() 호출
    }

    // 회피 상태에서 공격 액션 처리 (마우스 좌클릭 시 호출)
    public void DodgedDoAction()
    {
        if (weaponTable[type] == null)
            return;


        if (state.DodgedAttackMode == true)
            return;

        // Sword만 가능
        if (type != WeaponType.Sword)
            return;
        
        weaponTable[type].DodgedDoAction();
    }
    #endregion

    // 스킬 액션 처리
    #region Skill
    // Q 스킬 액션 처리 (Q 입력 시 호출)
    public void ActivateQSkill()
    {
        if (weaponTable[type] == null) 
            return;
        if (!(state.IdleMode || state.SkillCastMode))
            return;
        
        weaponTable[type].ActivateQSkill();
    }
    // E 스킬 액션 처리 (E 입력 시 호출)
    public void ActivateESkill()
    {
        if (weaponTable[type] == null)
            return;
        if (!(state.IdleMode || state.SkillCastMode))
            return;

        weaponTable[type].ActivateESkill();
    }

    // 스킬 액션 종료 (애니메이션 이벤트로 호출됨)
    private void End_SkillAction()
    {
        animator.SetBool("IsSkillAction", false);
        weaponTable[type].End_SkillAction();
    }

    // Q스킬 파티클 효과 재생 (애니메이션 이벤트로 호출됨)
    private void Play_QSkillParticles()
    {
        weaponTable[type].Play_QSkillParticles();
    }

    // E스킬 파티클 효과 재생 (애니메이션 이벤트로 호출됨)
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

    // 애니메이션 Event 호출 기능 처리
    #region Animation Events
    // 애니메이션 이벤트로 회피 공격 액션 종료
    private void End_DodgedDoAction()
    {
        if (SwordMode == false)
            return;

        weaponTable[type].End_DodgedDoAction(); // 회피 공격 종료
    }

    // 애니메이션 이벤트로 액션 시작 (AI에서 호출)
    private void Begin_DoAction()
    {
        weaponTable[type].Begin_DoAction();     // 현재 무기 액션 시작
    }

    // 공격 액션 종료 처리
    // AI에서 Action 중 피격 시 강제호출 때문에 보호 수준 변경 (07.20)
    public void End_DoAction()
    {
        if (UnarmedMode)
            return;

        animator.SetBool("IsAction", false);    // 액션 애니메이션 상태 해제
        OnDodgeAttack?.Invoke(false);           // 회피 공격 종료 이벤트 호출
        weaponTable[type].End_DoAction();       // 현재 무기 액션 종료
        OnEndDoAction?.Invoke();                // 액션 종료 이벤트 호출
    }

    // 콤보 공격 시작 (애니메이션 이벤트로 호출됨)
    private void Begin_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Combo();
    }

    // 콤보 공격 종료 (애니메이션 이벤트로 호출됨)
    private void End_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Combo();
    }

    // 공격 충돌 시작 (애니메이션 이벤트로 호출됨)
    private void Begin_Collision(AnimationEvent e)
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Collision(e);
    }

    // 공격 충돌 종료 (애니메이션 이벤트로 호출됨)
    private void End_Collision()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Collision();
    }

    // 무기의 카메라 쉐이킹 효과 실행 (애니메이션 이벤트로 호출됨)
    private void Play_Impulse()
    {
        Melee melee = weaponTable[type] as Melee;
        melee?.Play_Impulse();
    }

    // 공격 액션 시 파티클 효과 재생 (애니메이션 이벤트로 호출됨)
    private void Play_DoAction_Particle()
    {
        weaponTable[type].Play_Particle();
    }

    // 무기 사운드 재생 (애니메이션 이벤트로 호출됨)
    private void Play_Sound()
    {
        if (state.DeadMode == true)
            return;

        weaponTable[type].Play_Sound();
    }

    // 공격 범위가 좁아 무기 Collider에 닿지 않는 경우 호출 (애니메이션 이벤트로 호출됨)
    private void Create_Attack_Collision()
    {
        weaponTable[type].Create_Attack_Collision();
    }
    #endregion
}