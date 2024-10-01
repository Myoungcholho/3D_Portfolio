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

    // 검기 Transform
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
        if(ESkillDataCoolTime.RemainingCooldownTime >0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if(ESkillDataCoolTime.RemainingCooldownTime <0)
            {
                ESkillDataCoolTime.RemainingCooldownTime = 0;  // 쿨타임이 0보다 작아지지 않도록 제한
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
        // 쿨타임 판단
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

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

        // 검 생성 후 내려치는 스킬.
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

        // 검기 발사
        Vector3 pos = rootObject.transform.position;
        pos += new Vector3(0, 1.5f, 0f);
        pos += rootObject.transform.forward * 2.0f;

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        PhantomEdgeHandler handler = obj.GetComponentInChildren<PhantomEdgeHandler>();
        handler.Initialize(ESkillDataCoolTime.ColliderDelay,ESkillDataCoolTime.ColliderDuration);
        handler.OnEdgeHit += OnTriggerESkill;

        // 리스트 data초마다 Clear , 다단히트용
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

    private List<(GameObject target, float hitTime,int hitCount)> qSkillHitList = new List<(GameObject, float,int)>();
    private void OnTriggerQSkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;

        float currentTime = Time.time;

        // 타겟이 이미 리스트에 존재하는지 확인하고, 시간이 지났는지 확인
        var existingHit = qSkillHitList.FirstOrDefault(item => item.target == target);

        if (existingHit.target != null)
        {
            // 타격 시점으로부터 2초가 지났는지 확인
            if (currentTime - existingHit.hitTime > QSkillDataCoolTime.MultiHitInterval)
            {
                // 히트 카운트를 확인하고 5회를 초과하면 더 이상 히트하지 않음
                if (existingHit.hitCount >= QSkillDataCoolTime.MultiHitCount)
                    return;

                // 기존 항목 업데이트
                qSkillHitList.Remove(existingHit);
                qSkillHitList.Add((target, currentTime, existingHit.hitCount + 1));
                Vector3 hitPoint = t.ClosestPoint(other.transform.position);
                hitPoint = other.transform.InverseTransformPoint(hitPoint);
                damage.OnDamage(rootObject, this, hitPoint, QSkillData);
                return;
            }
            return;
        }

        // 리스트에 타겟과 현재 시간을 추가
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

    // 검기 호출 함수
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

        prevEvadeAction = evadeAction; // 같은 공격 방지를 위해 저장
    }

    // 애니메이션이 여기를 호출하는 부분까지
    // 애니메이션을 잘못두어서 Idle로 가는 이슈가 있었다.
    // 지금 해야 하는 것은
    // 1. Dodged 인 경우 마우스로 카메라 조정이 불가능하게 하기
    // 2. DodgedAttack인 경우 몬스터로 회전을 조정
    // 3. 적 앞으로 MoveToWard로 이동
    public void End_DodgedDoAction()
    {
        // 중간에 Idle모드로 변화해도 애니메이션에 의한 늦어지는 호출로
        // 의도치 않은 state변경을 막기 위함.
        if (state.DodgedAttackMode == true)
            state.SetDodgedMode();
    }

    // 애니메이션 이벤트에 의해 호출함.
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