using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Player : Character, IDamagable
{
    private Material skinMaterial;

    [SerializeField]
    private Color damageColor;
    [SerializeField]
    private float changeColorTime = 0.15f;

    private Color originColor;

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

        Transform surface = transform.FindChildByName("Alpha_Surface");
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;

    }

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        healthPoint.Damage(data.Power);

        HitCameraShake(causer);

        StartCoroutine(Change_Color(changeColorTime));
        MovableStopper.Instance.Start_Delay(data.StopFrame);

        if (healthPoint.Dead == false)
        {
            state.SetDamagedMode();

            animator.SetInteger("ImpactType", (int)causer.Type);
            animator.SetInteger("ImpactIndex", (int)data.HitImpactIndex);
            animator.SetTrigger("Impact");

            rigidbody.isKinematic = false;
            float launch = rigidbody.drag * data.Distance * 10.0f;
            rigidbody.velocity = Vector3.zero;
            rigidbody.AddForce(-transform.forward * launch);

            StartCoroutine(Change_IsKinemetics(5));

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

    private IEnumerator Change_Color(float time)
    {
        skinMaterial.color = damageColor;
        yield return new WaitForSeconds(time);
        skinMaterial.color = originColor;
    }

    private IEnumerator Change_IsKinemetics(int frame)
    {
        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        rigidbody.isKinematic = true;
    }
}
