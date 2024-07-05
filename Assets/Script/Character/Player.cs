using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(PlayerMovingComponent))]
public class Player : MonoBehaviour
{
    private Animator animator;
    private StateComponent state;
    private WeaponComponent weapon;

    /// <summary>
    /// Evade시 변경된 회전 값을 복구하기 위한 prev 값 저장
    /// </summary>

    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        weapon = GetComponent<WeaponComponent>();


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

        actionMap.FindAction("Evade").started += context =>
        {
            /*if (weapon.UnarmedMode == false)
                return;*/
            /*if (state.IdleMode == false)
                return;*/

            state.SetEvadeMode();
        };


    }
}
