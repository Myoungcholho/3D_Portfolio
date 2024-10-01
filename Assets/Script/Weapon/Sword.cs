using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tiny;
using UnityEngine;
using UnityEngine.Audio;

public class Sword : Melee
{
    [SerializeField]
    private string holsterName = "Holster_Sword";

    [SerializeField]
    private string handName = "Hand_Sword";

    public GameObject slashParicle;

    private Transform holsterTransform;
    private Transform handTransform;

    // �˱� Transform
    /*private Transform ss1;
    private Transform ss2;
    private Transform ss3;*/

    private Trail trail;

    private AudioClip audioSourceAttack01;
    private AudioMixerGroup audioMixer;

    [Header("-Skill Name-")]
    [SerializeField]
    private string skill01VCamName = "VCamSkill01";
    [SerializeField]
    private string skill02VCamName = "VCamSkill02";



    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Sword;
    }

    protected override void Awake()
    {
        base.Awake();

        //ss1 = rootObject.transform.FindChildByName("SwordSlash01").GetComponent<Transform>();
        //ss2 = rootObject.transform.FindChildByName("SwordSlash02").GetComponent<Transform>();
        //ss3 = rootObject.transform.FindChildByName("SwordSlash03").GetComponent<Transform>();

        /*Debug.Assert(ss1 != null);
        Debug.Assert(ss2 != null);
        Debug.Assert(ss3 != null);*/

        trail = GetComponent<Trail>();

        skill01Camera = rootObject.transform.FindChildByName(skill01VCamName)?.GetComponent<CinemachineVirtualCamera>();
        skill02Camera = rootObject.transform.FindChildByName(skill02VCamName)?.GetComponent<CinemachineVirtualCamera>();
    }

    protected override void Start()
    {
        base.Start();

        holsterTransform = rootObject.transform.FindChildByName(holsterName);
        Debug.Assert(holsterTransform != null);

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(holsterTransform, false);

        trail.enabled = false;
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
        if(ESkillDataCoolTime.RemainingCooldownTime >0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if(ESkillDataCoolTime.RemainingCooldownTime <0)
            {
                ESkillDataCoolTime.RemainingCooldownTime = 0;  // ��Ÿ���� 0���� �۾����� �ʵ��� ����
            }
        }

    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(handTransform, false);
    }

    public override void UnEquip()
    {
        base.UnEquip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(holsterTransform, false);
    }

    #region Skill
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

        // �� ���� �� ����ġ�� ��ų.
        Vector3 pos = rootObject.transform.position;
        pos += rootObject.transform.forward * 7.5f;

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(qSkillParticlePrefab, pos, quaternion);
        SummonedSwordHandler handler = obj.GetComponent<SummonedSwordHandler>();
        handler.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        handler.OnSwordSkill01HitTrigger += OnTriggerQSkill;
        handler.OnColliderDisabled += OnTriggerListClear;
    }
    public override void Play_ESkillParticles()
    {
        if (eSkillParticlePrefab == null)
            return;

        // �˱� �߻�
        Vector3 pos = rootObject.transform.position;
        pos += new Vector3(0, 1.5f, 0f);
        pos += rootObject.transform.forward * 2.0f;

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        PhantomEdgeHandler handler = obj.GetComponentInChildren<PhantomEdgeHandler>();
        handler.Initialize(ESkillDataCoolTime.ColliderDelay,ESkillDataCoolTime.ColliderDuration);
        handler.OnEdgeHit += OnTriggerESkill;

        // ����Ʈ data�ʸ��� Clear , �ٴ���Ʈ��
        StartCoroutine(ClearHitListRoutine(ESkillDataCoolTime.MultiHitInterval));

    }

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
        brain.RollBackBlend();  // ease in out ���� ����
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
        brain.RollBackBlend();  // ease in out ���� ����
    }

    private List<(GameObject target, float hitTime,int hitCount)> qSkillHitList = new List<(GameObject, float,int)>();
    private void OnTriggerQSkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;

        float currentTime = Time.time;

        // Ÿ���� �̹� ����Ʈ�� �����ϴ��� Ȯ���ϰ�, �ð��� �������� Ȯ��
        var existingHit = qSkillHitList.FirstOrDefault(item => item.target == target);

        if (existingHit.target != null)
        {
            // Ÿ�� �������κ��� 2�ʰ� �������� Ȯ��
            if (currentTime - existingHit.hitTime > QSkillDataCoolTime.MultiHitInterval)
            {
                // ��Ʈ ī��Ʈ�� Ȯ���ϰ� 5ȸ�� �ʰ��ϸ� �� �̻� ��Ʈ���� ����
                if (existingHit.hitCount >= QSkillDataCoolTime.MultiHitCount)
                    return;

                // ���� �׸� ������Ʈ
                qSkillHitList.Remove(existingHit);
                qSkillHitList.Add((target, currentTime, existingHit.hitCount + 1));
                Vector3 hitPoint = t.ClosestPoint(other.transform.position);
                hitPoint = other.transform.InverseTransformPoint(hitPoint);
                damage.OnDamage(rootObject, this, hitPoint, QSkillData);
                return;
            }
            return;
        }

        // ����Ʈ�� Ÿ�ٰ� ���� �ð��� �߰�
        qSkillHitList.Add((target, currentTime,1));

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, QSkillData);
    }
    private void OnTriggerListClear()
    {
        qSkillHitList.Clear();
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

    private IEnumerator ClearHitListRoutine(float interval)
    {
        float duration = ESkillDataCoolTime.ColliderDelay + ESkillDataCoolTime.ColliderDuration;
        float _time = 0f;

        while (_time < duration)
        {
            yield return new WaitForSeconds(interval);
            eSkillHitList.Clear();

            _time += interval;
        }
    }

    #endregion


    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);
        trail.enabled = true;

        //ActivateSlash();
    }

    public override void End_Collision()
    {
        base.End_Collision();

        trail.enabled = false;
    }

    // �˱� ȣ�� �Լ�
    /*void ActivateSlash()
    {
        GameObject obj = null;
        if (index == 0)
        {
            obj = Instantiate<GameObject>(slashParicle, ss1);
        }
        else if(index == 1)
        {
            obj = Instantiate<GameObject>(slashParicle, ss2);
        }
        else if(index == 2)
        {
            obj = Instantiate<GameObject>(slashParicle, ss3);
        }

        if(obj != null) 
        {
            obj.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Destroy(obj, 2f);
        }
    }*/

    private int prevEvadeAction;
    public void DodgedDoAction()
    {
        state.SetDodgedAttackMode();
        Player player = rootObject.GetComponent<Player>();
        player.SetAnimationSpeed(1.0f);

        int evadeAction = UnityEngine.Random.Range(0, 3);
        if (evadeAction == prevEvadeAction)
            evadeAction = (evadeAction + 1) % 3;

        index = 3;

        animator.SetBool("IsDodgedAction", true);
        animator.SetInteger("DodgedPattern", evadeAction);
        animator.SetTrigger("DodgeAttack");

        prevEvadeAction = evadeAction; // ���� ���� ������ ���� ����
    }

    // �ִϸ��̼��� ���⸦ ȣ���ϴ� �κб���
    // �ִϸ��̼��� �߸��ξ Idle�� ���� �̽��� �־���.
    // ���� �ؾ� �ϴ� ����
    // 1. Dodged �� ��� ���콺�� ī�޶� ������ �Ұ����ϰ� �ϱ�
    // 2. DodgedAttack�� ��� ���ͷ� ȸ���� ����
    // 3. �� ������ MoveToWard�� �̵�
    public void End_DodgedDoAction()
    {
        // �߰��� Idle���� ��ȭ�ص� �ִϸ��̼ǿ� ���� �ʾ����� ȣ���
        // �ǵ�ġ ���� state������ ���� ����.
        if (state.DodgedAttackMode == true)
            state.SetDodgedMode();
    }

    // �ִϸ��̼� �̺�Ʈ�� ���� ȣ����.
    public override void Play_Sound()
    {
        base.Play_Sound();

        if (audioSourceAttack01 == null)
            audioSourceAttack01 = SoundLibrary.Instance.swordAttack01;
        if (audioMixer == null)
            audioMixer = SoundLibrary.Instance.mixerBasic;

        if (soundComponent != null)
        {
            soundComponent.PlayLocalSound(audioSourceAttack01, audioMixer, false);
        }
    }
}