using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character, IDamagable
{

    protected override void Awake()
    {
        base.Awake();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        actionMap.FindAction("Sword").started += context =>
        {
            weapon.SetSwordMode();
        };

        actionMap.FindAction("FireBall").started += context =>
        {
            weapon.SetFireBallMode();
        };

        actionMap.FindAction("DualSword").started += context =>
        {
            weapon.SetDualSwordMode();
        };

        actionMap.FindAction("Action").started += context =>
        {
            weapon.DoAction();
        };

        actionMap.FindAction("Evade").started += context =>
        {
            /*if (weapon.UnarmedMode == false)
                return;*/
            /*if (state.IdleMode == false)
                return;*/

            state.SetEvadeMode();
        };
    }
    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        
    }
}
