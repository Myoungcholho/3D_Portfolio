using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Dummy : EnemyInformation, IDamagable
{
    #region HitMaterialChangeData

    [SerializeField]
    private Color damageColor;  // �ǰ� �� ���� ����
    [SerializeField]
    private float changeColorTime = 0.15f;  // ���� ���� ���� �ð�
    private Color originColor;  // ���� ���� ����
    [SerializeField]
    private string surfaceText = "Alpha_Surface";  // �޽� �̸�
    private Material skinMaterial;  // ��Ų ���͸���
    #endregion

    protected override void Awake()
    {
        base.Awake();
        Transform surface = transform.FindChildByName(surfaceText);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;  // �⺻ ���� ����
    }

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        // ü�� ���� ó��
        healthPoint.Damage(data.Power);

        // ī�޶� ��鸲 ó��
        if (data.abilityType == AbilityType.None)
            HitCameraShake(causer);

        // �ǰ� �� ���� ����
        StartCoroutine(Change_Color(changeColorTime));

        // �ǰ� �� ��ƼŬ ȿ�� ����
        if (data.HitParticle != null)
        {
            GameObject obj = Instantiate(data.HitParticle, transform, false);
            obj.transform.localPosition = hitPoint + data.HitParticlePositionOffset;
            obj.transform.localScale = data.HitParticleScaleOffset;
        }

        // ������� �ʾ��� �� ���� ���� �� �ִϸ��̼� ó��
        if (!healthPoint.Dead)
        {
            state.SetDamagedMode();

            animator.SetTrigger("Impact");
            animator.SetInteger("ImpactIndex", 1);

            return;
        }

        // ��� ó��
        state.SetDeadMode();
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        animator.SetTrigger("Dead");

        // ��� �� Action ȣ��
        if (OnDeath != null)
            OnDeath.Invoke(monsterType);

        // ��� �̺�Ʈ ȣ��
        DeathEvent deathEvent = GetComponent<DeathEvent>();
        if (deathEvent != null)
        {
            deathEvent.OnDeath();
            return;
        }
        Destroy(gameObject, 5f);  // 5�� �� �� ����
    }

    // ī�޶� ��鸲 ó��
    private void HitCameraShake(Weapon causer)
    {
        Melee melee = causer as Melee;
        melee?.Play_Impulse();
    }

    // ���� ���� ó��
    private IEnumerator Change_Color(float time)
    {
        skinMaterial.color = damageColor;
        yield return new WaitForSeconds(time);
        skinMaterial.color = originColor;
    }

    // �ǰ� ���� ���� ó��
    protected override void End_Damaged()
    {
        base.End_Damaged();
        animator.SetInteger("ImpactIndex", 0);
    }
}
