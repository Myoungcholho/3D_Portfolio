using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static DualSword;

public class Fist : Melee
{
    public enum FistType
    {
        LeftHand,RightHand,LeftFoot,RightFoot,Max
    }

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Fist;
    }

    protected override void Awake()
    {
        base.Awake();

        for(int i=0; i<(int)FistType.Max; ++i)
        {
            Transform t = colliders[i].transform;

            t.DetachChildren();

            Fist_Trigger trigger = t.GetComponent<Fist_Trigger>();
            trigger.OnTrigger += OnTriggerEnter;
            trigger.OnAttacker += Attacker;

            string partName = ((FistType)i).ToString();
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);

            t.SetParent(parent, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }
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

    public override void Begin_Collision(AnimationEvent e)
    {
        //base.Begin_Collision(e);

        colliders[e.intParameter].enabled = true;
        //Debug.Log("colliders : " + colliders[e.intParameter].name);
    }

    public override void End_Collision()
    {
        base.End_Collision();

    }

    private AudioClip audioSourceAttack01;
    private AudioMixerGroup audioMixer;
    public override void Play_Sound()
    {
        base.Play_Sound();

        if (audioSourceAttack01 == null)
            audioSourceAttack01 = SoundLibrary.Instance.fistAttack01;
        if (audioMixer == null)
            audioMixer = SoundLibrary.Instance.mixerBasic;

        if (soundComponent != null)
        {
            soundComponent.PlayLocalSound(audioSourceAttack01, audioMixer, false);
        }
    }

    #region SKill
    // Q 입력 시 호출됨.
    public override void ActivateQSkill()
    {
        // 쿨타임 판단
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // 쿨타임 적용
        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.ActivateQSkill();
    }

    public override void ActivateESkill()
    {
        // 쿨타임 처리
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;
        base.ActivateESkill();
    }

    public override void Play_QSkillParticles()
    {
        if (qSkillParticlePrefab == null)
            return;

        // 앞으로 대쉬
        // 애니메이션 이벤트가 PlayerMoving의 앞으로 움직이는 코드 실행함.

        // 내려찍고 폭팔
        Vector3 pos = rootObject.transform.position;
        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(qSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerQSkill;

        // 카메라 쉐이킹
        Play_SkillImpulse(QSkillData.abilityType);

        // 사운드
        

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

        // 앞으로 뻗는 펀치
        Vector3 pos = rootObject.transform.position;
        pos += new Vector3(0, 1.5f, 0f);
        pos += rootObject.transform.forward * 2.0f;

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ESkillDataCoolTime.ColliderDelay, ESkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerESkill;

        // 카메라 쉐이킹
        Play_SkillImpulse(ESkillData.abilityType);

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
