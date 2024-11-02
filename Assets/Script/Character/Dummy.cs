using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Dummy : EnemyInformation, IDamagable
{
    #region HitMaterialChangeData

    [SerializeField]
    private Color damageColor;  // 피격 시 색상 변경
    [SerializeField]
    private float changeColorTime = 0.15f;  // 색상 변경 지속 시간
    private Color originColor;  // 원래 색상 저장
    [SerializeField]
    private string surfaceText = "Alpha_Surface";  // 메쉬 이름
    private Material skinMaterial;  // 스킨 메터리얼
    #endregion

    protected override void Awake()
    {
        base.Awake();
        Transform surface = transform.FindChildByName(surfaceText);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;  // 기본 색상 저장
    }

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        // 체력 감소 처리
        healthPoint.Damage(data.Power);

        // 카메라 흔들림 처리
        if (data.abilityType == AbilityType.None)
            HitCameraShake(causer);

        // 피격 시 색상 변경
        StartCoroutine(Change_Color(changeColorTime));

        // 피격 시 파티클 효과 생성
        if (data.HitParticle != null)
        {
            GameObject obj = Instantiate(data.HitParticle, transform, false);
            obj.transform.localPosition = hitPoint + data.HitParticlePositionOffset;
            obj.transform.localScale = data.HitParticleScaleOffset;
        }

        // 사망하지 않았을 때 상태 변경 및 애니메이션 처리
        if (!healthPoint.Dead)
        {
            state.SetDamagedMode();

            animator.SetTrigger("Impact");
            animator.SetInteger("ImpactIndex", 1);

            return;
        }

        // 사망 처리
        state.SetDeadMode();
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        animator.SetTrigger("Dead");

        // 사망 시 Action 호출
        if (OnDeath != null)
            OnDeath.Invoke(monsterType);

        // 사망 이벤트 호출
        DeathEvent deathEvent = GetComponent<DeathEvent>();
        if (deathEvent != null)
        {
            deathEvent.OnDeath();
            return;
        }
        Destroy(gameObject, 5f);  // 5초 후 적 제거
    }

    // 카메라 흔들림 처리
    private void HitCameraShake(Weapon causer)
    {
        Melee melee = causer as Melee;
        melee?.Play_Impulse();
    }

    // 색상 변경 처리
    private IEnumerator Change_Color(float time)
    {
        skinMaterial.color = damageColor;
        yield return new WaitForSeconds(time);
        skinMaterial.color = originColor;
    }

    // 피격 상태 종료 처리
    protected override void End_Damaged()
    {
        base.End_Damaged();
        animator.SetInteger("ImpactIndex", 0);
    }
}
