using System;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Player : Character, IDamagable
{
    private Material skinMaterial;

    [SerializeField]
    private Color damageColor;
    [SerializeField]
    private Color evadeColor;

    [SerializeField]
    private float changeColorTime = 0.15f;

    private Color originColor;
    private PlayerMovingComponent movingComponent;

    // Evade 성공 시 잔상
    private MeshTrail trail;
    public Action<bool> OnDodgeAttack;

    private SoundComponent soundComponent;

    protected override void Awake()
    {
        base.Awake();

        movingComponent = GetComponent<PlayerMovingComponent>();
        trail = GetComponent<MeshTrail>();
        soundComponent = GetComponent<SoundComponent>();

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

        actionMap.FindAction("Fist").started += context =>
        {
            weapon.SetFistMode();
        };

        actionMap.FindAction("Hammer").started += context =>
        {
            weapon.SetHammerMode();
        };

        actionMap.FindAction("Action").started += context =>
        {
            if(state.DodgedMode)
            {
                // 대상앞으로 나아가기

                StartCoroutine(movingComponent.MoveToTarget(lastAttacker.transform, 1.4f));
                weapon.DodgedDoAction();
                return;
            }

            weapon.DoAction();
        };

        actionMap.FindAction("Evade").started += context =>
        {
            bool bCheck = false;
            bCheck |= state.DamagedMode == true;
            bCheck |= state.EquipMode == true;
            bCheck |= state.DodgedMode == true;
            bCheck |= state.InstantKillMode == true;

            if(bCheck)
                return;

            state.SetEvadeMode();
        };

        Transform surface = transform.FindChildByName("Alpha_Surface");
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;

    }

    private GameObject lastAttacker;                // 반격 시 날라갈 대상 저장

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        lastAttacker = attacker;        // 공격자를 등록, 반격 대상으로 날라가기 위해 저장

        if (state.EvadeMode == true)
        {
            trail.ActivateMeshTrail();      // 잔상
            state.SetDodgedMode();          
            soundComponent.PlayLocalSound(SoundLibrary.Instance.evadeDodage01, SoundLibrary.Instance.mixerBasic, false);
            StartCoroutine(MovableStopper.Instance.EvadeDelay());
            OnDodgeAttack?.Invoke(true);    // 뭐였지?
            return;
        }

        healthPoint.Damage(data.Power);

        HitCameraShake(causer);

        StartCoroutine(Change_Color(changeColorTime,damageColor));
        MovableStopper.Instance.Start_Delay(data.StopFrame);

        if (healthPoint.Dead == false)
        {
            state.SetDamagedMode();

            animator.SetInteger("ImpactType", (int)causer.Type);
            animator.SetInteger("ImpactIndex", (int)data.HitImpactIndex);
            animator.SetTrigger("Impact");

            return;
        }
        state.SetDeadMode();

        Collider collider = GetComponent<Collider>();
        collider.enabled = false;

        animator.SetTrigger("Dead");
        Destroy(gameObject, 5f);
    }

    private void HitCameraShake(Weapon causer)
    {
        Melee melee = causer as Melee;
        melee?.Play_Impulse();
    }

    private IEnumerator Change_Color(float time, Color changeColor)
    {
        skinMaterial.color = changeColor;
        yield return new WaitForSeconds(time);
        skinMaterial.color = originColor;
    }

    private void EvadeSuccessColor()
    {
        StartCoroutine(Change_Color(changeColorTime,evadeColor));
    }

    public override bool IsSpecialObject()
    {
        return true;
    }

    public void ResetToNormal()
    {
        state.SetIdleMode();
        animator.SetBool("IsDodgedAction", false);
    }
}
