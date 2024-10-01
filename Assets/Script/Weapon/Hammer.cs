using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Hammer : Melee
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


    private AudioClip audioSourceAttack01;
    private AudioMixerGroup audioMixer;



    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Hammer;
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

    protected override void Update()
    {
        // Q ��ų ��Ÿ�� ����
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
        {
            QSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (QSkillDataCoolTime.RemainingCooldownTime < 0)
            {
                QSkillDataCoolTime.RemainingCooldownTime = 0;  // ��Ÿ���� 0���� �۾����� �ʵ��� ����
            }
        }

        // E ��ų ��Ÿ�� ����
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ESkillDataCoolTime.RemainingCooldownTime < 0)
            {
                ESkillDataCoolTime.RemainingCooldownTime = 0;  // ��Ÿ���� 0���� �۾����� �ʵ��� ����
            }
        }
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


    public override void Play_Sound()
    {
        base.Play_Sound();

        if (audioSourceAttack01 == null)
            audioSourceAttack01 = SoundLibrary.Instance.hammerAttack01;
        if (audioMixer == null)
            audioMixer = SoundLibrary.Instance.mixerBasic;

        if (soundComponent != null)
        {
            soundComponent.PlayLocalSound(audioSourceAttack01, audioMixer, false);
        }
    }

    #region SKill
    public override void ActivateQSkill()
    {
        // ��Ÿ�� �Ǵ�
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.ActivateQSkill();
    }

    public override void ActivateESkill()
    {
        // ��Ÿ�� ó��
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;
        base.ActivateESkill();
    }
    public override void Play_QSkillParticles()
    {
        if (qSkillParticlePrefab == null)
            return;

        // ������� �ٴ� �ر�
        Vector3 pos = rootObject.transform.position;
        pos += rootObject.transform.forward * 3.0f;
        pos += new Vector3(0, 0.01f, 0);

        Quaternion quaternion = rootObject.transform.rotation;



        GameObject obj = Instantiate<GameObject>(qSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerQSkill;

        // ī�޶� ����ŷ
        Play_SkillImpulse(QSkillData.abilityType);

        // ����

        // Hit List �ʱ�ȭ
        qSkillHitList.Clear();
        StartCoroutine(ClearHitListRoutine(qSkillHitList, QSkillDataCoolTime.MultiHitInterval));
    }

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
        damage.OnDamage(rootObject, this, hitPointNew, QSkillData);
    }

    public override void Play_ESkillParticles()
    {
        if (eSkillParticlePrefab == null)
            return;

        // ������ ���� ��ġ
        Vector3 pos = rootObject.transform.position;
        pos += new Vector3(0, 1.5f, 0f);
        pos += rootObject.transform.forward * 2.0f;

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ESkillDataCoolTime.ColliderDelay, ESkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerESkill;

        // ī�޶� ����ŷ
        Play_SkillImpulse(ESkillData.abilityType);

        // Hit List �ʱ�ȭ
        eSkillHitList.Clear();
        StartCoroutine(ClearHitListRoutine(eSkillHitList, ESkillDataCoolTime.MultiHitInterval));
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

    private IEnumerator ClearHitListRoutine(List<GameObject> list, float interval)
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