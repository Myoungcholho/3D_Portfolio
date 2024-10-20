using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

// Enemy 클래스: 적 캐릭터의 피격 처리와 데미지 반응을 구현하는 클래스
public class Enemy : Character, IDamagable, IDodgeDamageHandler
{
    [Header("Animation에 의한 발 오차 offset")]
    [SerializeField]
    private Vector3 footOffset = new Vector3(0f, -0.08f, 0f);  // 애니메이션 발 위치 오차 보정

    [SerializeField]
    private Color damageColor;  // 피격 시 색상 변경
    [SerializeField]
    private float changeColorTime = 0.15f;  // 색상 변경 지속 시간

    private Color originColor;  // 원래 색상 저장
    [SerializeField]
    private string surfaceText = "Alpha_Surface";  // 메쉬 이름
    private Material skinMaterial;  // 스킨 메터리얼

    private AIController aiController;  // AI 제어
    private BossAIController bossAIController;  // 보스 AI 제어

    // 반격 데미지 처리
    private float knockbackDistance;  // 밀리기 거리 누적
    private int knockbackAttackCount;  // 반격 공격 횟수 누적

    // 컴포넌트 초기화
    protected override void Awake()
    {
        base.Awake();
        aiController = GetComponent<AIController>();
        bossAIController = GetComponent<BossAIController>();
        Transform surface = transform.FindChildByName(surfaceText);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;  // 기본 색상 저장
    }

    Vector3 oppositeDirection;  // 적 회전 방향 저장

    // 피격 시 호출되는 메서드 (일반/반격 데미지 공통 처리)
    public void HandleDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data, bool isDodgeDamage)
    {
        // 아군이라면 피격 처리하지 않음
        if (attacker.CompareTag("Enemy"))
            return;

        // 체력 감소 처리
        healthPoint.Damage(data.Power);

        // 카메라 흔들림 처리
        if (data.abilityType == AbilityType.None)
            HitCameraShake(causer);

        // 피격 시 색상 변경
        StartCoroutine(Change_Color(changeColorTime));

        // 사망하지 않았을 때 적의 회전 처리
        if (!healthPoint.Dead)
        {
            if (data.isObjectPushDisperse || isDodgeDamage)
            {
                // 공격자 방향으로 회전 (분산 회전 또는 반격)
                Vector3 direction = attacker.transform.position - transform.position;
                oppositeDirection = isDodgeDamage ? -attacker.transform.forward : direction.normalized;
            }
            else
            {
                // 공격자의 반대 방향으로 회전
                oppositeDirection = -attacker.transform.forward;
            }

            Quaternion lookRotation = Quaternion.LookRotation(oppositeDirection, Vector3.up);
            transform.rotation = lookRotation;
        }

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
            aiController?.SetDamageMode();
            bossAIController?.OnDamaged();
            state.SetDamagedMode();

            animator.SetInteger("ImpactType", (int)causer.Type);
            animator.SetInteger("ImpactIndex", (int)data.HitImpactIndex);
            animator.SetTrigger("Impact");

            // 밀리기 처리 (밀리기 거리 누적은 반격 공격일 경우에만)
            if (isDodgeDamage)
            {
                knockbackDistance += data.Distance;
                knockbackAttackCount++;
            }
            else
            {
                // 일반 공격 시 물리적 밀리기 처리
                rigidbody.isKinematic = false;
                float launch = rigidbody.drag * data.Distance * 10.0f;
                rigidbody.velocity = Vector3.zero;
                rigidbody.AddForce(-transform.forward * launch);
                StartCoroutine(Change_IsKinemetics(5));
            }
            return;
        }

        // 사망 처리
        state.SetDeadMode();
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        animator.SetTrigger("Dead");

        // 사망 이벤트 호출
        DeathEvent deathEvent = GetComponent<DeathEvent>();
        if (deathEvent != null)
        {
            deathEvent.OnDeath();
            return;
        }
        Destroy(gameObject, 5f);  // 5초 후 적 제거
    }


    // 피격 시 호출되는 메서드 (일반 데미지)
    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        HandleDamage(attacker, causer, hitPoint, data, false);  // 일반 공격
    }

    // 피격 시 호출되는 메서드 (반격 데미지)
    public void OnDodgeDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        HandleDamage(attacker, causer, hitPoint, data, true);   // 반격 공격
    }

    // 밀리기 처리 후 초기화
    public void CommitAndResetKnockback()
    {
        if (healthPoint.Dead || knockbackAttackCount == 0)
            return;

        int impactIndex = (knockbackAttackCount > 3) ? 1 : 0;
        animator.SetInteger("ImpactType", (int)WeaponType.Sword);
        animator.SetInteger("ImpactIndex", impactIndex);
        animator.SetTrigger("Impact");

        rigidbody.isKinematic = false;
        float launch = rigidbody.drag * knockbackDistance * 10f;
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(-transform.forward * launch);

        StartCoroutine(Change_IsKinemetics(5));
        knockbackDistance = 0;
        knockbackAttackCount = 0;
    }

    // 애니메이터의 WakeUp 파라미터 설정
    private void WakeUpAnimatorParameter()
    {
        animator.SetTrigger("WakeUp");
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

    // 일정 프레임 후에 물리 처리 비활성화
    private IEnumerator Change_IsKinemetics(int frame)
    {
        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();
        rigidbody.isKinematic = true;
    }

    // 피격 상태 종료 처리
    protected override void End_Damaged()
    {
        base.End_Damaged();
        animator.SetInteger("ImpactIndex", 0);
        aiController?.End_Damge();
    }

    // Gizmo를 사용하여 적 회전 방향 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + new Vector3(0, 1f, 0);
        Gizmos.DrawLine(pos, pos + oppositeDirection);
    }
}