using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static DualSword;

// Fist Ŭ����: Melee Ŭ������ ��ӹ޾� �ָ� ������ ����
public class Fist : Melee
{
    // �ָ� Ÿ�� ���� (�հ� ��)
    public enum FistType
    {
        LeftHand, RightHand, LeftFoot, RightFoot, Max
    }

    // ���� Ÿ���� �ָ����� ����
    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.Fist;
    }

    // �ʱ�ȭ: �ݶ��̴� ���� �� Ʈ���� ����
    protected override void Awake()
    {
        base.Awake();

        // �� �ָ� �� �߿� ���� Ʈ���� ����
        for (int i = 0; i < (int)FistType.Max; ++i)
        {
            Transform t = colliders[i].transform;

            t.DetachChildren(); // �ڽ� ������Ʈ �и�

            Fist_Trigger trigger = t.GetComponent<Fist_Trigger>();
            trigger.OnTrigger += OnTriggerEnter;  // Ʈ���� �̺�Ʈ ����
            trigger.OnAttacker += Attacker;

            string partName = ((FistType)i).ToString(); // �ָ�/�� �̸� ����
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);  // �θ� ������Ʈ Ȯ��

            t.SetParent(parent, false);  // �θ� ����
            t.localPosition = Vector3.zero; // ��ġ �� ȸ�� �ʱ�ȭ
            t.localRotation = Quaternion.identity;
        }
    }

    // �� ������ ��ų ��Ÿ�� ���� ó��
    protected override void Update()
    {
        // Q ��ų ��Ÿ�� ó��
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
        {
            QSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (QSkillDataCoolTime.RemainingCooldownTime < 0)
                QSkillDataCoolTime.RemainingCooldownTime = 0;
        }

        // E ��ų ��Ÿ�� ó��
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ESkillDataCoolTime.RemainingCooldownTime < 0)
                ESkillDataCoolTime.RemainingCooldownTime = 0;
        }
    }

    // ���� ���� �� �ش� �ݶ��̴� Ȱ��ȭ
    public override void Begin_Collision(AnimationEvent e)
    {
        colliders[e.intParameter].enabled = true;  // �ִϸ��̼� �̺�Ʈ�� ���� �ݶ��̴� Ȱ��ȭ
    }

    // ���� ���� �� �ݶ��̴� ��Ȱ��ȭ
    public override void End_Collision()
    {
        base.End_Collision();
    }

    // ���� ���� ���
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

    // Q ��ų ��� (��Ÿ�� ����)
    public override void ActivateQSkill()
    {
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.ActivateQSkill();
    }

    // E ��ų ��� (��Ÿ�� ����)
    public override void ActivateESkill()
    {
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;
        base.ActivateESkill();
    }

    // Q ��ų ��ƼŬ ���� �� ī�޶� ��鸲 ����
    public override void Play_QSkillParticles()
    {
        if (qSkillParticlePrefab == null)
            return;

        Vector3 pos = rootObject.transform.position;
        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(qSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerQSkill;  // Q ��ų ��Ʈ ó��

        Play_SkillImpulse(QSkillData.abilityType);  // ī�޶� ���޽�

        StartCoroutine(ClearHitListRoutine(qSkillHitList, QSkillDataCoolTime.MultiHitInterval));  // �ٴ���Ʈ ó��
    }

    // Q ��ų ��Ʈ ó��
    private List<GameObject> qSkillHitList = new List<GameObject>();
    private void OnTriggerQSkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;
        if (qSkillHitList.Contains(target))
            return;

        qSkillHitList.Add(target);  // ��Ʈ ����Ʈ�� �߰�

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, QSkillData);  // ������ ó��
    }

    // E ��ų ��ƼŬ ���� �� ī�޶� ��鸲 ����
    public override void Play_ESkillParticles()
    {
        if (eSkillParticlePrefab == null)
            return;

        Vector3 pos = rootObject.transform.position + new Vector3(0, 1.5f, 0f) + rootObject.transform.forward * 2.0f;
        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ESkillDataCoolTime.ColliderDelay, ESkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerESkill;  // E ��ų ��Ʈ ó��

        Play_SkillImpulse(ESkillData.abilityType);  // ī�޶� ���޽�

        StartCoroutine(ClearHitListRoutine(eSkillHitList, ESkillDataCoolTime.MultiHitInterval));  // �ٴ���Ʈ ó��
    }

    // E ��ų ��Ʈ ó��
    private List<GameObject> eSkillHitList = new List<GameObject>();
    private void OnTriggerESkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;
        if (eSkillHitList.Contains(target))
            return;

        eSkillHitList.Add(target);  // ��Ʈ ����Ʈ�� �߰�

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ESkillData);  // ������ ó��
    }

    // ��Ʈ ����Ʈ�� �ֱ������� �ʱ�ȭ�ϴ� �ڷ�ƾ
    private IEnumerator ClearHitListRoutine(List<GameObject> list, float interval)
    {
        float duration = ESkillDataCoolTime.ColliderDelay + ESkillDataCoolTime.ColliderDuration;
        float _time = 0f;

        while (_time < duration)
        {
            yield return new WaitForSeconds(interval);
            list.Clear();  // ��Ʈ ����Ʈ �ʱ�ȭ

            _time += interval;
        }
    }

    #endregion
}