using UnityEngine;
using UnityEngine.Audio;

public class FireBall : Weapon
{
    [SerializeField]
    private string staffTransformName = "Hand_FireBall_Staff";

    [SerializeField]
    private string flameTransformName = "Hand_FireBall_Flame";

    [SerializeField]
    private string muzzleTransformName = "Hand_FireBall_Muzzle";

    [SerializeField]
    private GameObject flameParticleOrigin;

    [SerializeField]
    private GameObject projectilePrefab;

    private Transform staffTransform;
    private Transform flameTransform;
    private Transform muzzleTransform;
    private GameObject flameParticle;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.FireBall;
    }

    protected override void Awake()
    {
        base.Awake();

        staffTransform = rootObject.transform.FindChildByName(staffTransformName);
        Debug.Assert(staffTransform != null);
        transform.SetParent(staffTransform, false);

        flameTransform = rootObject.transform.FindChildByName(flameTransformName);
        Debug.Assert(flameTransform != null);

        muzzleTransform = rootObject.transform.FindChildByName(muzzleTransformName);
        Debug.Assert(muzzleTransform != null);

        if (flameParticleOrigin != null)
        {
            flameParticle = Instantiate<GameObject>(flameParticleOrigin, flameTransform);
            flameParticle.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        gameObject.SetActive(true);
        flameParticle?.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();

        gameObject.SetActive(false);
        flameParticle?.SetActive(false);
    }

    public override void Play_Particle()
    {
        base.Play_Particle();

        if (doActionDatas[0].Particle == null)
            return;

        Vector3 position = muzzleTransform.position;
        Quaternion rotation = rootObject.transform.rotation;

        Instantiate<GameObject>(doActionDatas[0].Particle, position, rotation);
    }

    public override void Begin_DoAction()
    {
        base.Begin_DoAction();
        if (projectilePrefab == null)
            return;

        Vector3 muzzlePosition = muzzleTransform.position;
        muzzlePosition += rootObject.transform.forward * 0.5f;

        GameObject obj = Instantiate<GameObject>(projectilePrefab, muzzlePosition, rootObject.transform.rotation);

        Projectile projectile = obj.GetComponent<Projectile>();
        {
            projectile.OnProjectileHit += OnProjectileHit;
        }

        obj.SetActive(true);
    }

    private AudioClip audioSourceAttack01;
    private AudioMixerGroup audioMixer;
    public override void Play_Sound()
    {
        base.Play_Sound();

        if (audioSourceAttack01 == null)
            audioSourceAttack01 = SoundLibrary.Instance.staffAttack01;
        if (audioMixer == null)
            audioMixer = SoundLibrary.Instance.mixerBasic;

        if (soundComponent != null)
        {
            soundComponent.PlayLocalSound(audioSourceAttack01, audioMixer, true);
        }
    }

    // projectile의 Action에 연결해 invoke 받을 함수
    private void OnProjectileHit(Collider t, Collider other, Vector3 point)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage != null)
        {
            Vector3 hitPoint = t.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);

            damage?.OnDamage(rootObject, this, hitPoint, doActionDatas[0]);
            return;
        }

        if (doActionDatas[0].HitParticle != null)
            Instantiate<GameObject>(doActionDatas[0].HitParticle, point, rootObject.transform.rotation);
    }
}