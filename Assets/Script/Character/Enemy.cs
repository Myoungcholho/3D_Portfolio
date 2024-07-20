using System.Collections;
using UnityEngine;

public class Enemy : Character, IDamagable
{
    [Header("Animation에 의한 발 오차 offset")]
    [SerializeField]
    private Vector3 footOffset = new Vector3(0f, -0.08f, 0f);

    [SerializeField]
    private Color damageColor;
    [SerializeField]
    private float changeColorTime = 0.15f;

    private Color originColor;
    private Material skinMaterial;

    private AIController aiController;

    protected override void Awake()
    {
        base.Awake();

        aiController = GetComponent<AIController>();
        Transform surface = transform.FindChildByName("Alpha_Surface");
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;
    }
    Vector3 oppositeDirection;
    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        // Damage 처리
        healthPoint.Damage(data.Power);
        // CameraSahking
        HitCameraShake(causer);

        StartCoroutine(Change_Color(changeColorTime));
        MovableStopper.Instance.Start_Delay(data.StopFrame);

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

    protected override void End_Damaged()
    {
        base.End_Damaged();

        animator.SetInteger("ImpactIndex", 0);
        state.SetIdleMode();

    }

    // Enemy의 Animator RootMotion을 직접 적용
    private void OnAnimatorMove()
    {
        Vector3 pos = transform.position + footOffset;
        transform.position = pos + animator.deltaPosition;

        transform.rotation *= animator.deltaRotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 pos = transform.position + new Vector3(0, 1f, 0);
        

        // Scene에 Gizmos를 사용하여 선 그리기
        Gizmos.DrawLine(pos, pos+ oppositeDirection);
    }
}
