using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static DualSword;

// Fist 클래스: Melee 클래스를 상속받아 주먹 공격을 구현
public class Fist : Melee
{
    // 주먹 타입 정의 (손과 발)
    public enum FistType
    {
        LeftHand, RightHand, LeftFoot, RightFoot, Max
    }

    // 무기 타입을 주먹으로 설정
    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.Fist;
    }

    // 초기화: 콜라이더 설정 및 트리거 연결
    protected override void Awake()
    {
        base.Awake();

        // 각 주먹 및 발에 대한 트리거 설정
        for (int i = 0; i < (int)FistType.Max; ++i)
        {
            Transform t = colliders[i].transform;

            t.DetachChildren(); // 자식 오브젝트 분리

            Fist_Trigger trigger = t.GetComponent<Fist_Trigger>();
            trigger.OnTrigger += OnTriggerEnter;  // 트리거 이벤트 연결
            trigger.OnAttacker += Attacker;

            string partName = ((FistType)i).ToString(); // 주먹/발 이름 설정
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);  // 부모 오브젝트 확인

            t.SetParent(parent, false);  // 부모에 설정
            t.localPosition = Vector3.zero; // 위치 및 회전 초기화
            t.localRotation = Quaternion.identity;
        }
    }

    // 매 프레임 스킬 쿨타임 감소 처리
    protected override void Update()
    {
        // Q 스킬 쿨타임 처리
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
        {
            QSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (QSkillDataCoolTime.RemainingCooldownTime < 0)
                QSkillDataCoolTime.RemainingCooldownTime = 0;
        }

        // E 스킬 쿨타임 처리
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ESkillDataCoolTime.RemainingCooldownTime < 0)
                ESkillDataCoolTime.RemainingCooldownTime = 0;
        }
    }

    // 공격 시작 시 해당 콜라이더 활성화
    public override void Begin_Collision(AnimationEvent e)
    {
        colliders[e.intParameter].enabled = true;  // 애니메이션 이벤트에 따라 콜라이더 활성화
    }

    // 공격 종료 시 콜라이더 비활성화
    public override void End_Collision()
    {
        base.End_Collision();
    }

    // 공격 사운드 재생
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

    #region Skill

    // Q 스킬 사용 (쿨타임 적용)
    public override void ActivateQSkill()
    {
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.ActivateQSkill();
    }

    // E 스킬 사용 (쿨타임 적용)
    public override void ActivateESkill()
    {
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;
        base.ActivateESkill();
    }

    // Q 스킬 파티클 생성 및 카메라 흔들림 적용
    public override void Play_QSkillParticles()
    {
        if (qSkillParticlePrefab == null)
            return;

        Vector3 pos = rootObject.transform.position;
        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(qSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerQSkill;  // Q 스킬 히트 처리

        Play_SkillImpulse(QSkillData.abilityType);  // 카메라 임펄스

        StartCoroutine(ClearHitListRoutine(qSkillHitList, QSkillDataCoolTime.MultiHitInterval));  // 다단히트 처리
    }

    // Q 스킬 히트 처리
    private List<GameObject> qSkillHitList = new List<GameObject>();
    private void OnTriggerQSkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;
        if (qSkillHitList.Contains(target))
            return;

        qSkillHitList.Add(target);  // 히트 리스트에 추가

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, QSkillData);  // 데미지 처리
    }

    // E 스킬 파티클 생성 및 카메라 흔들림 적용
    public override void Play_ESkillParticles()
    {
        if (eSkillParticlePrefab == null)
            return;

        Vector3 pos = rootObject.transform.position + new Vector3(0, 1.5f, 0f) + rootObject.transform.forward * 2.0f;
        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ESkillDataCoolTime.ColliderDelay, ESkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerESkill;  // E 스킬 히트 처리

        Play_SkillImpulse(ESkillData.abilityType);  // 카메라 임펄스

        StartCoroutine(ClearHitListRoutine(eSkillHitList, ESkillDataCoolTime.MultiHitInterval));  // 다단히트 처리
    }

    // E 스킬 히트 처리
    private List<GameObject> eSkillHitList = new List<GameObject>();
    private void OnTriggerESkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;
        if (eSkillHitList.Contains(target))
            return;

        eSkillHitList.Add(target);  // 히트 리스트에 추가

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ESkillData);  // 데미지 처리
    }

    // 히트 리스트를 주기적으로 초기화하는 코루틴
    private IEnumerator ClearHitListRoutine(List<GameObject> list, float interval)
    {
        float duration = ESkillDataCoolTime.ColliderDelay + ESkillDataCoolTime.ColliderDuration;
        float _time = 0f;

        while (_time < duration)
        {
            yield return new WaitForSeconds(interval);
            list.Clear();  // 히트 리스트 초기화

            _time += interval;
        }
    }

    #endregion
}