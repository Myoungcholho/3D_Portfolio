using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tiny;
using UnityEngine;
using UnityEngine.Audio;

// Sword 클래스: Melee 클래스를 상속받아 검 무기의 동작을 구현
public class Sword : Melee
{

    // 무기 보관 위치와 손 위치의 오브젝트 이름
    [SerializeField]
    private string holsterName = "Holster_Sword";
    [SerializeField]
    private string handName = "Hand_Sword";
    // 무기 보관 위치와 손 위치의 오브젝트 위치
    private Transform holsterTransform;
    private Transform handTransform;

    // 검의 잔상 효과
    private Trail trail;

    // 사운드
    private AudioClip audioSourceAttack01;
    private AudioMixerGroup audioMixer;

    // 재생할 가상 카메라 이름
    [SerializeField]
    private string skill01VCamName = "VCamSkill01";
    [SerializeField]
    private string skill02VCamName = "VCamSkill02";


    // 클래스 초기화 시 기본값 설정
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

        // 검 보관 위치와 손 위치의 Transform 설정
        holsterTransform = rootObject.transform.FindChildByName(holsterName);
        Debug.Assert(holsterTransform != null);

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        // 검을 보관 위치에 배치
        transform.SetParent(holsterTransform, false);
        trail.enabled = false;
    }

    // 매 프레임 호출되어 스킬 쿨타임 감소 처리
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

        // 3번째 스킬 쿨타임 감소
        if(ThreeSkillDataCoolTime.RemainingCooldownTime >0)
        {
            ThreeSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ThreeSkillDataCoolTime.RemainingCooldownTime < 0)
                ThreeSkillDataCoolTime.RemainingCooldownTime = 0;
        }
    }

    #region Weapon Equip/Unequip
    // 무기 장착 시작
    public override void Begin_Equip()
    {
        base.Begin_Equip();

        // 현재 부모로부터 분리 후 손에 장착
        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(handTransform, false);
    }
    // 무기 해제
    public override void UnEquip()
    {
        base.UnEquip();

        // 현재 부모로부터 분리 후 보관 위치에 장착
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
        // 쿨타임 체크
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // 쿨타임 판단
        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.Activate01Skill();
    }
    // Two Skill
    public override void Activate02Skill()
    {
        // 쿨타임 체크
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // 쿨타임 판단
        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;
        base.Activate02Skill();
    }
    // Three Skill
    public override void Activate03Skill()
    {
        // 쿨타임 체크
        if (ThreeSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        // 쿨타임 판단
       ThreeSkillDataCoolTime.RemainingCooldownTime = ThreeSkillDataCoolTime.CooldownTime;

        base.Activate03Skill();
    }


    // Q 스킬 파티클 재생
    public override void Play_01SkillParticles()
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

    // E 스킬 파티클 재생
    public override void Play_02SkillParticles()
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

    // 3번째 스킬 파티클 재생
    public override void Play_03Skill()
    {
        if (ThreeSkillPrefab == null)
            return;

        // 찌르기
        Vector3 pos = rootObject.transform.position;
        pos += new Vector3(0, 0.9f, 0f);

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(ThreeSkillPrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ThreeSkillDataCoolTime.ColliderDelay, ThreeSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerSkill;

        StartCoroutine(ClearHitListRoutine(SkillHitList, ThreeSkillDataCoolTime.MultiHitInterval));  // 다단히트 처리
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

        SkillHitList.Add(target);  // 히트 리스트에 추가

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ThreeSkillData);  // 데미지 처리
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

    // 스킬 시네머신 카메라 기능
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

    #region Skill Collision Handling
    // Q 스킬 히트 처리 리스트
    private List<(GameObject target, float hitTime,int hitCount)> qSkillHitList = new List<(GameObject, float,int)>();
    // Q 스킬 히트 시 호출되는 메서드
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
    // Q 스킬 히트 리스트 초기화
    private void OnTriggerListClear()
    {
        qSkillHitList.Clear();
    }

    // E 스킬 히트 처리 리스트
    private List<GameObject> eSkillHitList = new List<GameObject>();
    // E 스킬 히트 시 호출되는 메서드
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

    // E 스킬 히트 리스트를 주기적으로 초기화하는 코루틴 (동일 초기화 방법)
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
        playerMoving.CancelMove();                                                              // 회피 이동 코루틴 정지
        StartCoroutine(playerMoving.MoveToTarget(playerInput.lastAttacker.transform, 1.4f));    // 대상 접근   
        target?.TargetSearch();                                                                 // 타겟팅 기능 호출
        MovableStopper.Instance.AttackCount++;                                                  // 반격중 공격 1회 추가

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
    public override void End_DodgedDoAction()
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