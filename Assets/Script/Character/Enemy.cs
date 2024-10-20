using System.Collections;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

// Enemy Ŭ����: �� ĳ������ �ǰ� ó���� ������ ������ �����ϴ� Ŭ����
public class Enemy : Character, IDamagable, IDodgeDamageHandler
{
    [Header("Animation�� ���� �� ���� offset")]
    [SerializeField]
    private Vector3 footOffset = new Vector3(0f, -0.08f, 0f);  // �ִϸ��̼� �� ��ġ ���� ����

    [SerializeField]
    private Color damageColor;  // �ǰ� �� ���� ����
    [SerializeField]
    private float changeColorTime = 0.15f;  // ���� ���� ���� �ð�

    private Color originColor;  // ���� ���� ����
    [SerializeField]
    private string surfaceText = "Alpha_Surface";  // �޽� �̸�
    private Material skinMaterial;  // ��Ų ���͸���

    private AIController aiController;  // AI ����
    private BossAIController bossAIController;  // ���� AI ����

    // �ݰ� ������ ó��
    private float knockbackDistance;  // �и��� �Ÿ� ����
    private int knockbackAttackCount;  // �ݰ� ���� Ƚ�� ����

    // ������Ʈ �ʱ�ȭ
    protected override void Awake()
    {
        base.Awake();
        aiController = GetComponent<AIController>();
        bossAIController = GetComponent<BossAIController>();
        Transform surface = transform.FindChildByName(surfaceText);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;  // �⺻ ���� ����
    }

    Vector3 oppositeDirection;  // �� ȸ�� ���� ����

    // �ǰ� �� ȣ��Ǵ� �޼��� (�Ϲ�/�ݰ� ������ ���� ó��)
    public void HandleDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data, bool isDodgeDamage)
    {
        // �Ʊ��̶�� �ǰ� ó������ ����
        if (attacker.CompareTag("Enemy"))
            return;

        // ü�� ���� ó��
        healthPoint.Damage(data.Power);

        // ī�޶� ��鸲 ó��
        if (data.abilityType == AbilityType.None)
            HitCameraShake(causer);

        // �ǰ� �� ���� ����
        StartCoroutine(Change_Color(changeColorTime));

        // ������� �ʾ��� �� ���� ȸ�� ó��
        if (!healthPoint.Dead)
        {
            if (data.isObjectPushDisperse || isDodgeDamage)
            {
                // ������ �������� ȸ�� (�л� ȸ�� �Ǵ� �ݰ�)
                Vector3 direction = attacker.transform.position - transform.position;
                oppositeDirection = isDodgeDamage ? -attacker.transform.forward : direction.normalized;
            }
            else
            {
                // �������� �ݴ� �������� ȸ��
                oppositeDirection = -attacker.transform.forward;
            }

            Quaternion lookRotation = Quaternion.LookRotation(oppositeDirection, Vector3.up);
            transform.rotation = lookRotation;
        }

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
            aiController?.SetDamageMode();
            bossAIController?.OnDamaged();
            state.SetDamagedMode();

            animator.SetInteger("ImpactType", (int)causer.Type);
            animator.SetInteger("ImpactIndex", (int)data.HitImpactIndex);
            animator.SetTrigger("Impact");

            // �и��� ó�� (�и��� �Ÿ� ������ �ݰ� ������ ��쿡��)
            if (isDodgeDamage)
            {
                knockbackDistance += data.Distance;
                knockbackAttackCount++;
            }
            else
            {
                // �Ϲ� ���� �� ������ �и��� ó��
                rigidbody.isKinematic = false;
                float launch = rigidbody.drag * data.Distance * 10.0f;
                rigidbody.velocity = Vector3.zero;
                rigidbody.AddForce(-transform.forward * launch);
                StartCoroutine(Change_IsKinemetics(5));
            }
            return;
        }

        // ��� ó��
        state.SetDeadMode();
        Collider collider = GetComponent<Collider>();
        collider.enabled = false;
        animator.SetTrigger("Dead");

        // ��� �̺�Ʈ ȣ��
        DeathEvent deathEvent = GetComponent<DeathEvent>();
        if (deathEvent != null)
        {
            deathEvent.OnDeath();
            return;
        }
        Destroy(gameObject, 5f);  // 5�� �� �� ����
    }


    // �ǰ� �� ȣ��Ǵ� �޼��� (�Ϲ� ������)
    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        HandleDamage(attacker, causer, hitPoint, data, false);  // �Ϲ� ����
    }

    // �ǰ� �� ȣ��Ǵ� �޼��� (�ݰ� ������)
    public void OnDodgeDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        HandleDamage(attacker, causer, hitPoint, data, true);   // �ݰ� ����
    }

    // �и��� ó�� �� �ʱ�ȭ
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

    // �ִϸ������� WakeUp �Ķ���� ����
    private void WakeUpAnimatorParameter()
    {
        animator.SetTrigger("WakeUp");
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

    // ���� ������ �Ŀ� ���� ó�� ��Ȱ��ȭ
    private IEnumerator Change_IsKinemetics(int frame)
    {
        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();
        rigidbody.isKinematic = true;
    }

    // �ǰ� ���� ���� ó��
    protected override void End_Damaged()
    {
        base.End_Damaged();
        animator.SetInteger("ImpactIndex", 0);
        aiController?.End_Damge();
    }

    // Gizmo�� ����Ͽ� �� ȸ�� ���� �ð�ȭ
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + new Vector3(0, 1f, 0);
        Gizmos.DrawLine(pos, pos + oppositeDirection);
    }
}