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
    // Q �Է� �� ȣ���.
    public override void ActivateQSkill()
    {
        // ��Ÿ�� �Ǵ�
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // ��Ÿ�� ����
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

        // ������ �뽬
        // �ִϸ��̼� �̺�Ʈ�� PlayerMoving�� ������ �����̴� �ڵ� ������.

        // ������� ����
        Vector3 pos = rootObject.transform.position;
        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(qSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerQSkill;

        // ī�޶� ����ŷ
        Play_SkillImpulse(QSkillData.abilityType);

        // ����
        

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
