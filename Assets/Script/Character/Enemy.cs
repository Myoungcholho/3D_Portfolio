using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Enemy : Character, IDamagable, IDodgeDamageHandler
{
    [Header("Animation에 의한 발 오차 offset")]
    [SerializeField]
    private Vector3 footOffset = new Vector3(0f, -0.08f, 0f);

    [SerializeField]
    private Color damageColor;
    [SerializeField]
    private float changeColorTime = 0.15f;

    private Color originColor;
    [SerializeField]
    private string surfaceText = "Alpha_Surface";
    private Material skinMaterial;

    private AIController aiController;
    private BossAIController bossAIController;

    protected override void Awake()
    {
        base.Awake();

        aiController = GetComponent<AIController>();
        bossAIController = GetComponent<BossAIController>();
        Transform surface = transform.FindChildByName(surfaceText);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;
    }
    Vector3 oppositeDirection;

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        // Damage 처리
        healthPoint.Damage(data.Power);
        // CameraSahking
        if (data.abilityType == AbilityType.None)
            HitCameraShake(causer);

        StartCoroutine(Change_Color(changeColorTime));

        MovableStopper.Instance.Start_Delay(data.StopFrame);
       
        if (healthPoint.Dead == false)
        {
            // 적의 회전을 설정
            // 1) True , 공격자와 방향벡터를 구해 회전
            if (data.isObjectPushDisperse == true)
            {
                Vector3 Direction = attacker.transform.position - transform.position;
                oppositeDirection = Direction.normalized;

                Quaternion lookRotation = Quaternion.LookRotation(oppositeDirection, Vector3.up);
                transform.rotation = lookRotation;
            }
            // 2) False, 공격자의 -Forward 방향으로 회전
            else
            {
                oppositeDirection = -attacker.transform.forward;

                Quaternion lookRotation = Quaternion.LookRotation(oppositeDirection, Vector3.up);
                transform.rotation = lookRotation;
            }
        }

        if (data.HitParticle != null)
        {
            GameObject obj = Instantiate<GameObject>(data.HitParticle, transform, false);
            obj.transform.localPosition = hitPoint + data.HitParticlePositionOffset;
            obj.transform.localScale = data.HitParticleScaleOffset;
        }


        if (healthPoint.Dead == false)
        {
            aiController?.SetDamageMode();
            bossAIController?.OnDamaged();

            state.SetDamagedMode(); // 이 부분이 변경되고 바로 애니 이벤트로 변경되어 씹힘.

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

        DeathEvent deathEvent = GetComponent<DeathEvent>();
        if(deathEvent != null)
        {
            deathEvent.OnDeath();
            return;
        }
        Destroy(gameObject, 5f);
    }

    //반격 시 데미지 막타에 한번에 처리하기 위한 값
    private float knockbackDistance;
    private int knockbackAttackCount;
    public void OnDodgeDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        healthPoint.Damage(data.Power);
        HitCameraShake(causer);
        StartCoroutine(Change_Color(changeColorTime));

        if (healthPoint.Dead == false)
        {
            oppositeDirection = -attacker.transform.forward;

            // 적의 회전을 설정
            Quaternion lookRotation = Quaternion.LookRotation(oppositeDirection, Vector3.up);
            transform.rotation = lookRotation;
        }

        if (data.HitParticle != null)
        {
            GameObject obj = Instantiate<GameObject>(data.HitParticle, transform, false);
            obj.transform.localPosition = hitPoint + data.HitParticlePositionOffset;
            obj.transform.localScale = data.HitParticleScaleOffset;
        }

        if (healthPoint.Dead == false)
        {
            aiController?.SetDamageMode();
            bossAIController?.OnDamaged();

            state.SetDamagedMode();

            animator.SetInteger("ImpactType", (int)causer.Type);
            animator.SetInteger("ImpactIndex", (int)data.HitImpactIndex);
            animator.SetTrigger("Impact");

            // 밀리기 처리를 위한 값 누적
            knockbackDistance += data.Distance;
            ++knockbackAttackCount;

            return;
        }

        state.SetDeadMode();
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        animator.SetTrigger("Dead");
        Destroy(gameObject, 5f);
    }

    // 누적된 밀리는 값을 적용 및 초기화
    public void CommitAndResetKnockback()
    {
        if (healthPoint.Dead == true)
            return;

        if (knockbackAttackCount == 0)
            return;

        Debug.Log("knockbackAttackCount :" + knockbackAttackCount);
        int impactIndex;
        if (3 < knockbackAttackCount)
        {
            impactIndex = 1;
            Invoke("WakeUpAnimatorParameter", 2f);
        }
        else
            impactIndex = 0;

        animator.SetInteger("ImpactType", (int)WeaponType.Sword);
        animator.SetInteger("ImpactIndex", impactIndex);
        animator.SetTrigger("Impact");

        rigidbody.isKinematic = false;
        float launch = rigidbody.drag * knockbackDistance * 10f;
        Debug.Log($"KnockBack Lauch :" + launch);
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(-transform.forward * launch);

        StartCoroutine(Change_IsKinemetics(5));
        knockbackDistance = 0;
        knockbackAttackCount = 0;
    }

    private void WakeUpAnimatorParameter()
    {
        animator.SetTrigger("WakeUp");
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

    protected override void End_Damaged()
    {
        base.End_Damaged();

        animator.SetInteger("ImpactIndex", 0);
        aiController?.End_Damge();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + new Vector3(0, 1f, 0);
        Gizmos.DrawLine(pos, pos+ oppositeDirection);
    }
}
