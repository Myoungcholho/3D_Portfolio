using Cinemachine;
using System.Collections;
using System.Collections.Generic;
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

    [Header("-Skill Name-")]
    [SerializeField]
    private string skill01VCamName = "VCamStaff_Skill01";
    [SerializeField]
    private string skill02VCamName = "VCamStaff_Skill02";


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

        skill01Camera = rootObject.transform.FindChildByName(skill01VCamName)?.GetComponent<CinemachineVirtualCamera>();
        skill02Camera = rootObject.transform.FindChildByName(skill02VCamName)?.GetComponent<CinemachineVirtualCamera>();

        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        // Q 스킬 쿨타임 감소
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
        {
            QSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (QSkillDataCoolTime.RemainingCooldownTime < 0)
            {
                QSkillDataCoolTime.RemainingCooldownTime = 0;  // 쿨타임이 0보다 작아지지 않도록 제한
            }
        }

        // E 스킬 쿨타임 감소
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ESkillDataCoolTime.RemainingCooldownTime < 0)
            {
                ESkillDataCoolTime.RemainingCooldownTime = 0;  // 쿨타임이 0보다 작아지지 않도록 제한
            }
        }

    }

    #region Basic

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
    #endregion

    #region Skill
    // 데칼 용 위치 저장 변수
    private Vector3 skill01Position;
    private Vector3 skill02Position;
    public override void Activate01Skill()
    {
        // 쿨타임 판단
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // Skill Range Draw
        if(decalComponent.SkillType != SkillType.Staff01)
        {
            state.SetSkillCastMode();
            decalComponent.ActivateDecalForTargeting(SkillType.Staff01,10f);
            return;
        }

        // 스킬 생성할 위치 지정
        skill01Position = decalComponent.Position;

        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.Activate01Skill();
    }

    public override void Activate02Skill()
    {
        // 쿨타임 처리
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;
        // Skill Range Draw
        if (decalComponent.SkillType != SkillType.Staff02)
        {
            state.SetSkillCastMode();
            decalComponent.ActivateDecalForTargeting(SkillType.Staff02, 10f);
            return;
        }

        // 스킬 생성할 위치 지정
        skill02Position = decalComponent.Position;

        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;
        base.Activate02Skill();
    }
    public override void Play_01SkillParticles()
    {
        if (qSkillParticlePrefab == null)
            return;

        // 썬더볼트 스킬
        Vector3 pos = skill01Position;

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(qSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerQSkill;

        // 리스트 data초마다 Clear , 다단히트용
        StartCoroutine(ClearHitListRoutine(qSkillHitList, QSkillDataCoolTime.MultiHitInterval));
    }
    public override void Play_02SkillParticles()
    {
        if (eSkillParticlePrefab == null)
            return;

        // 얼음 광역 스킬
        Vector3 pos = skill02Position;

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ESkillDataCoolTime.ColliderDelay, ESkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerESkill;

        // 리스트 data초마다 Clear , 다단히트용
        StartCoroutine(ClearHitListRoutine(eSkillHitList, ESkillDataCoolTime.MultiHitInterval));

    }

    #region Skill_VirtualCam
    public override void Begin_Skill01VCam()
    {
        base.Begin_Skill01VCam();

        BrainController brain = Camera.main.transform.GetComponent<BrainController>();
        brain.SetDefaultBlend("Cut", 0f);

        skill01Camera.Priority = 15;
    }
    public override void End_Skill01VCam()
    {
        base.End_Skill01VCam();
        skill01Camera.Priority = 0;

        BrainController brain = Camera.main.transform.GetComponent<BrainController>();
        brain.RollBackBlend();  // ease in out 으로 변경
    }
    public override void Begin_Skill02VCam()
    {
        base.Begin_Skill02VCam();

        BrainController brain = Camera.main.transform.GetComponent<BrainController>();
        brain.SetDefaultBlend("Cut", 0f);

        skill02Camera.Priority = 15;

    }
    public override void End_Skill02VCam()
    {
        base.End_Skill02VCam();

        skill02Camera.Priority = 0;

        BrainController brain = Camera.main.transform.GetComponent<BrainController>();
        brain.RollBackBlend();  // ease in out 으로 변경
    }
    #endregion

    private List<GameObject> qSkillHitList = new List<GameObject>();
    private void OnTriggerQSkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;

        if (qSkillHitList.Contains(target) == true)
            return;

        qSkillHitList.Add(target);

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ESkillData);
    }

    private List<GameObject> eSkillHitList = new List<GameObject>();
    private void OnTriggerESkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;

        if (eSkillHitList.Contains(target) == true)
            return;

        eSkillHitList.Add(target);

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ESkillData);
    }

    private IEnumerator ClearHitListRoutine(List<GameObject> list , float interval)
    {
        float duration = ESkillDataCoolTime.ColliderDelay + ESkillDataCoolTime.ColliderDuration;
        float _time = 0f;

        while (_time < duration)
        {
            yield return new WaitForSeconds(interval);
            list.Clear();

            _time += interval;
        }
    }

    #endregion
}