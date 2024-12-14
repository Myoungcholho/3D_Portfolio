using System;
using UnityEngine;

public class StateComponent : MonoBehaviour
{
    public enum StateType// : byte
    {
        Idle = 0, Equip, Action, Evade, Damaged, 
        Dead, Dodged, DodgedAttack, InstantKill, InstantKilled, UsingSkill,SkillCast
    }
    public enum SubStateType
    {
        Grounded = 0,       // 지면
        Airborne,           // 땅바닥
        KnockedDown         // 공격받고 자빠짐
    }

    public enum DamageStateType
    {
        Normal = 0,     // 기본 상태: 데미지를 그대로 받음
        SuperArmor,     // 슈퍼아머 상태: 데미지는 받지만 경직되지 않음
        Invincible      // 무적 상태: 데미지를 받지 않음
    }



    private StateType type = StateType.Idle;
    public event Action<StateType, StateType> OnStateTypeChanged;

    private SubStateType subType = SubStateType.Grounded;
    public event Action<SubStateType, SubStateType> OnSubStateTypeChanged;

    private DamageStateType damageType = DamageStateType.Normal;
    public event Action<DamageStateType,DamageStateType> OnDamageStateChanged;

    public bool IdleMode { get => type == StateType.Idle;}
    public bool EquipMode { get => type == StateType.Equip; }
    public bool ActionMode { get => type == StateType.Action; }
    public bool EvadeMode { get => type == StateType.Evade; }
    public bool DamagedMode { get => type == StateType.Damaged; }
    public bool DeadMode { get => type == StateType.Dead; }
    public bool DodgedMode { get => type == StateType.Dodged; }
    public bool DodgedAttackMode {get=>type == StateType.DodgedAttack; }
    public bool InstantKillMode { get=>type == StateType.InstantKill; }
    public bool InstantKilledMode { get => type == StateType.InstantKilled; }
    public bool UsingSkillMode { get => type == StateType.UsingSkill; }
    public bool SkillCastMode {  get => type == StateType.SkillCast; }


    // sub State
    public bool GroundedMode { get=> subType == SubStateType.Grounded; }
    public bool AirborneMode {  get=> subType == SubStateType.Airborne; }
    public bool KnockedDownMode { get => subType == SubStateType.KnockedDown; }


    // damage State
    public bool NormalMode { get => damageType == DamageStateType.Normal; }
    public bool SuperArmorMode { get => damageType == DamageStateType.SuperArmor; }
    public bool InvincibleMode { get => damageType == DamageStateType.Invincible; }

    // State
    public void SetIdleMode() => ChangeType(StateType.Idle);
    public void SetEquipMode() => ChangeType(StateType.Equip);
    public void SetActionMode() => ChangeType(StateType.Action);
    public void SetDamagedMode() => ChangeType(StateType.Damaged);
    public void SetDeadMode() => ChangeType(StateType.Dead);
    public void SetEvadeMode() => ChangeType(StateType.Evade);
    public void SetDodgedMode() => ChangeType(StateType.Dodged);
    public void SetDodgedAttackMode() => ChangeType(StateType.DodgedAttack);
    public void SetInstantKillMode() => ChangeType(StateType.InstantKill);
    public void SetInstantKilledMode() => ChangeType(StateType.InstantKilled);
    public void SetUsingSkillMode() => ChangeType(StateType.UsingSkill);
    public void SetSkillCastMode() => ChangeType(StateType.SkillCast);


    //sub State
    public void SetGroundedMode() => SubChangeType(SubStateType.Grounded);
    public void SetAirborneMode() => SubChangeType(SubStateType.Airborne);

    //Damaged State
    public void SetNormalMode() => DamageChangeType(DamageStateType.Normal);
    public void SetSuperArmorMode() => DamageChangeType(DamageStateType.SuperArmor);
    public void SetInvincibleMode() => DamageChangeType(DamageStateType.Invincible);


    private void ChangeType(StateType type)
    {
        if (this.type == type)
            return;

        if (this.type == StateType.Dead)
            return;

        StateType prevType = this.type;
        this.type = type;

        OnStateTypeChanged?.Invoke(prevType, type);
    }

    private void SubChangeType(SubStateType type)
    {
        if (this.subType == type)
            return;

        SubStateType prevType = this.subType;
        this.subType = type;

        OnSubStateTypeChanged?.Invoke(prevType, type);
    }

    private void DamageChangeType(DamageStateType type)
    {
        if(this.damageType == type) 
            return;

        DamageStateType prevType = this.damageType;
        this.damageType = type;

        OnDamageStateChanged?.Invoke(prevType, type);
    }
}