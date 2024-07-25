using UnityEngine;

public class BossHammer : Melee
{
    [SerializeField]
    private string handName = "Hand_Hammer";

    [SerializeField]
    private GameObject particlePrefab;

    [SerializeField]
    private string particleTransformName = "warhammer_head_low";

    private Transform handTransform;
    private Transform particleTransform;
    private GameObject trailParticle;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.BossHammer;
    }

    protected override void Awake()
    {
        base.Awake();

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(handTransform, false);
        gameObject.SetActive(false);


        particleTransform = transform.FindChildByName(particleTransformName);
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();

        gameObject.SetActive(false);
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);

        if (particleTransform == null)
            return;

        if (particlePrefab == null)
            return;

        trailParticle = Instantiate<GameObject>(particlePrefab, particleTransform);
    }

    public override void End_Collision()
    {
        base.End_Collision();

        Destroy(trailParticle);
    }

    // 애니메이션 Event 호출함.
    private int animationEventInt;
    public override void Play_Particle_Index(AnimationEvent e)
    {
        base.Play_Particle();

        animationEventInt = e.intParameter;
        if (doActionDatas[animationEventInt].Particle == null)
            return;

        GameObject obj = Instantiate<GameObject>(doActionDatas[animationEventInt].Particle, rootObject.transform.position,rootObject.transform.rotation);
        ProjectileHammer projectile = obj.GetComponentInChildren<ProjectileHammer>();

        if (projectile == null)
            return;

        projectile.OnProjectileHit += OnProjectileHit;
    }

    // 맞은 적, 파티클 transform.position 들어옴.
    private void OnProjectileHit(Collider other, Vector3 point)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage != null)
        {
            /*Vector3 hitPoint = t.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);*/

            damage?.OnDamage(rootObject, this, other.transform.position , doActionDatas[animationEventInt]);
            return;
        }

        if (doActionDatas[0].HitParticle != null)
            Instantiate<GameObject>(doActionDatas[animationEventInt].HitParticle, point, rootObject.transform.rotation);
    }
}