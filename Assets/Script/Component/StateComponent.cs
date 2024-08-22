using System;
using UnityEngine;

public class StateComponent : MonoBehaviour
{
    public enum StateType// : byte
    {
        Idle = 0, Equip, Action, Evade, Damaged, Dead, Dodged, DodgedAttack, InstantKill, InstantKilled
    }
    public enum SubStateType
    {
        Grounded = 0, Airborne
    }

    private StateType type = StateType.Idle;
    public event Action<StateType, StateType> OnStateTypeChanged;

    private SubStateType subType = SubStateType.Grounded;
    public event Action<SubStateType, SubStateType> OnSubStateTypeChanged;

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


    // sub State
    public bool GroundedMode { get=> subType == SubStateType.Grounded; }
    public bool AirborneMode {  get=> subType == SubStateType.Airborne; }

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

    //sub State
    public void SetGroundedMode() => SubChangeType(SubStateType.Grounded);
    public void SetAirborneMode() => SubChangeType(SubStateType.Airborne);


    private void ChangeType(StateType type)
    {
        if (this.type == type)
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
}