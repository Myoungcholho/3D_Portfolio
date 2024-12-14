using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tiny;
using UnityEngine;
using UnityEngine.Audio;

// Sword Ŭ����: Melee Ŭ������ ��ӹ޾� �� ������ ������ ����
public class Sword : Melee
{

    // ���� ���� ��ġ�� �� ��ġ�� ������Ʈ �̸�
    [SerializeField]
    private string holsterName = "Holster_Sword";
    [SerializeField]
    private string handName = "Hand_Sword";
    // ���� ���� ��ġ�� �� ��ġ�� ������Ʈ ��ġ
    private Transform holsterTransform;
    private Transform handTransform;

    // ���� �ܻ� ȿ��
    private Trail trail;

    // ����
    private AudioClip audioSourceAttack01;
    private AudioMixerGroup audioMixer;

    // ����� ���� ī�޶� �̸�
    [SerializeField]
    private string skill01VCamName = "VCamSkill01";
    [SerializeField]
    private string skill02VCamName = "VCamSkill02";


    // Ŭ���� �ʱ�ȭ �� �⺻�� ����
    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Sword;
    }

    protected override void Awake()
    {
        base.Awake();

        trail = GetComponent<Trail>();
        skill01Camera = rootObject.transform.FindChildByName(skill01VCamName)?.GetComponent<CinemachineVirtualCamera>();
        skill02Camera = rootObject.transform.FindChildByName(skill02VCamName)?.GetComponent<CinemachineVirtualCamera>();
    }

    protected override void Start()
    {
        base.Start();

        // �� ���� ��ġ�� �� ��ġ�� Transform ����
        holsterTransform = rootObject.transform.FindChildByName(holsterName);
        Debug.Assert(holsterTransform != null);

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        // ���� ���� ��ġ�� ��ġ
        transform.SetParent(holsterTransform, false);
        trail.enabled = false;
    }

    // �� ������ ȣ��Ǿ� ��ų ��Ÿ�� ���� ó��
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

        // 3��° ��ų ��Ÿ�� ����
        if(ThreeSkillDataCoolTime.RemainingCooldownTime >0)
        {
            ThreeSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ThreeSkillDataCoolTime.RemainingCooldownTime < 0)
                ThreeSkillDataCoolTime.RemainingCooldownTime = 0;
        }
    }

    #region Weapon Equip/Unequip
    // ���� ���� ����
    public override void Begin_Equip()
    {
        base.Begin_Equip();

        // ���� �θ�κ��� �и� �� �տ� ����
        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(handTransform, false);
    }
    // ���� ����
    public override void UnEquip()
    {
        base.UnEquip();

        // ���� �θ�κ��� �и� �� ���� ��ġ�� ����
        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(holsterTransform, false);
    }
    #endregion

    #region Skill Implementation
    // One Skill
    public override void Activate01Skill()
    {
        // ��Ÿ�� üũ
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // ��Ÿ�� �Ǵ�
        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.Activate01Skill();
    }
    // Two Skill
    public override void Activate02Skill()
    {
        // ��Ÿ�� üũ
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // ��Ÿ�� �Ǵ�
        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;
        base.Activate02Skill();
    }
    // Three Skill
    public override void Activate03Skill()
    {
        // ��Ÿ�� üũ
        if (ThreeSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // ��Ÿ�� �Ǵ�
       ThreeSkillDataCoolTime.RemainingCooldownTime = ThreeSkillDataCoolTime.CooldownTime;

        base.Activate03Skill();
    }


    // Q ��ų ��ƼŬ ���
    public override void Play_01SkillParticles()
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

    // E ��ų ��ƼŬ ���
    public override void Play_02SkillParticles()
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

    // 3��° ��ų ��ƼŬ ���
    public override void Play_03Skill()
    {
        if (ThreeSkillPrefab == null)
            return;

        // ���
        Vector3 pos = rootObject.transform.position;
        pos += new Vector3(0, 0.9f, 0f);

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(ThreeSkillPrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ThreeSkillDataCoolTime.ColliderDelay, ThreeSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerSkill;

        StartCoroutine(ClearHitListRoutine(SkillHitList, ThreeSkillDataCoolTime.MultiHitInterval));  // �ٴ���Ʈ ó��
    }

    private List<GameObject> SkillHitList = new List<GameObject>();
    private void OnTriggerSkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;
        if (SkillHitList.Contains(target))
            return;

        SkillHitList.Add(target);  // ��Ʈ ����Ʈ�� �߰�

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ThreeSkillData);  // ������ ó��
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

    // ��ų �ó׸ӽ� ī�޶� ���
    #region Cinemachin Camera
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
    #endregion

    #region Skill Collision Handling
    // Q ��ų ��Ʈ ó�� ����Ʈ
    private List<(GameObject target, float hitTime,int hitCount)> qSkillHitList = new List<(GameObject, float,int)>();
    // Q ��ų ��Ʈ �� ȣ��Ǵ� �޼���
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
    // Q ��ų ��Ʈ ����Ʈ �ʱ�ȭ
    private void OnTriggerListClear()
    {
        qSkillHitList.Clear();
    }

    // E ��ų ��Ʈ ó�� ����Ʈ
    private List<GameObject> eSkillHitList = new List<GameObject>();
    // E ��ų ��Ʈ �� ȣ��Ǵ� �޼���
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

    // E ��ų ��Ʈ ����Ʈ�� �ֱ������� �ʱ�ȭ�ϴ� �ڷ�ƾ (���� �ʱ�ȭ ���)
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

    private int prevEvadeAction;
    public override void DodgedDoAction()
    {
        playerMoving.CancelMove();                                                              // ȸ�� �̵� �ڷ�ƾ ����
        StartCoroutine(playerMoving.MoveToTarget(playerInput.lastAttacker.transform, 1.4f));    // ��� ����   
        target?.TargetSearch();                                                                 // Ÿ���� ��� ȣ��
        MovableStopper.Instance.AttackCount++;                                                  // �ݰ��� ���� 1ȸ �߰�

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
    public override void End_DodgedDoAction()
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