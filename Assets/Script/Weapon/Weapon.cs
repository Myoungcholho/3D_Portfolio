using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DualSword;

public enum AbilityType
{
    None =0,        // None의 경우 피격 시 카메라 쉐이킹
    Q,              // Q 스킬
    E               // E 스킬
}


// 공격 데이터
[Serializable]
public class DoActionData
{
    public AbilityType abilityType;                     // 스킬 Q,E 구분을 위한 변수
    public bool isObjectPushDisperse;                   // 캐릭터 뒤로만 밀리지 않고 분산해서 밀릴지 여부
    public bool bCanMove;                               // 이동 가능 여부

    public float Power;                                 // 데미지
    public float Distance;                              // 밀리는 거리
    public int StopFrame;                               // 멈추는 프레임 수

    public GameObject Particle;                         // 파티클 효과
    public Vector3 ParticlePositionOffset;              // 파티클 위치 오프셋
    public Vector3 ParticleScaleOffset = Vector3.one;   // 파티클 크기 오프셋

    public int HitImpactIndex;                          // 타격 효과 인덱스
    public GameObject HitParticle;                      // 타격 파티클
    public Vector3 HitParticlePositionOffset;           // 타격 파티클 위치 오프셋
    public Vector3 HitParticleScaleOffset = Vector3.one;// 타격 파티클 크기 오프셋

    public Vector3 ImpulseDirection;                    // 카메라 임펄스 방향
    public Cinemachine.NoiseSettings ImpulseSettings;   // 카메라 임펄스 셋팅
}

// 스킬이 가지는 추가 데이터
[Serializable]
public class DoActionSkillData
{
    public float CooldownTime;                  // 스킬 쿨타임
    public float RemainingCooldownTime;         // 스킬 현재 남은 쿨타임

    public float ColliderDelay;                 // 콜라이더 딜레이 시간
    public float ColliderDuration;              // 콜라이더 유지 시간
    public float MultiHitInterval;              // 다단히트 시간
    public int MultiHitCount;                   // 다단히트 횟수
}

[Serializable]
public class HandActionData
{
    public PartType partType;                 // 왼손/오른손 구분
    public List<DoActionData> actionData;     // 해당 손의 액션 데이터
}

// Weapon 추상 클래스: 무기와 관련된 기본 기능을 정의
public abstract class Weapon : MonoBehaviour , IWeaponCoolTime
{
    // 무기 타입
    [SerializeField]
    protected WeaponType type;                      

    // 공격 데이터 배열 (콤보)
    [SerializeField]
    protected DoActionData[] doActionDatas;

    // 공격 데이터 배열 (콤보, 양손)
    [SerializeField]
    protected bool IsBothHandAction = false;
    [SerializeField]
    protected List<HandActionData> handActionDataList;
    protected Dictionary<PartType, List<DoActionData>> handActionData;

    // 무기의 장착 여부
    protected bool bEquipped;                       
    protected bool bEquipping;                      
    public bool Equipping { get => bEquipping; }

    // 무기 타입 변환
    public WeaponType Type { get => type; }

    // 영향을 받는 컴포넌트
    protected GameObject rootObject;
    protected StateComponent state;
    protected Animator animator;
    protected SoundComponent soundComponent;
    protected DecalComponent decalComponent;
    protected PlayerMovingComponent playerMoving;
    protected Player playerInput;
    protected TargetComponent target;

    // 스킬 관련 컴포넌트
    [Header("-Skill-")]
    [SerializeField]
    protected GameObject qSkillParticlePrefab;
    [SerializeField]
    protected GameObject eSkillParticlePrefab;
    [SerializeField]
    protected GameObject ThreeSkillPrefab;
    [SerializeField]
    protected GameObject FourSkillPrefab;

    protected CinemachineVirtualCamera skill01Camera;
    protected CinemachineVirtualCamera skill02Camera;

    // Skill One
    [SerializeField]
    protected DoActionData QSkillData;
    [SerializeField]
    protected DoActionSkillData QSkillDataCoolTime;

    // Skill Two
    [SerializeField]
    protected DoActionData ESkillData;
    [SerializeField]
    protected DoActionSkillData ESkillDataCoolTime;

    // Skill Three
    [SerializeField]
    protected DoActionData ThreeSkillData;
    [SerializeField]
    protected DoActionSkillData ThreeSkillDataCoolTime;

    // Skill Four
    [SerializeField]
    protected DoActionData FourSkillData;
    [SerializeField]
    protected DoActionSkillData FourSkillDataCoolTime;


    protected CinemachineBrain brain;
    protected CinemachineImpulseSource impulse;

    protected virtual void Reset()
    {
        // 상속 받은 클래스에서 구체화
    }

    protected virtual void Awake()
    {
        rootObject = transform.parent.gameObject;
        Debug.Assert(rootObject != null);

        // 컴포넌트 연결
        state = rootObject.GetComponent<StateComponent>();
        animator = rootObject.GetComponent<Animator>();
        soundComponent = rootObject.GetComponent<SoundComponent>();
        decalComponent = rootObject.GetComponent<DecalComponent>();

        // 카메라 관련 컴포넌트 초기화
        brain = Camera.main.GetComponent<CinemachineBrain>();
        impulse = GetComponent<CinemachineImpulseSource>();

        // 플레이어 이동 및 타겟 컴포넌트 초기화
        playerMoving = rootObject.GetComponent<PlayerMovingComponent>();
        playerInput = rootObject.GetComponent<Player>();
        target = rootObject.GetComponent<TargetComponent>();
    }

    protected virtual void Start()
    {
        // 상속받은 클래스에서 구체화
    }

    protected virtual void Update()
    {
        // 상속받은 클래스에서 구체화
    }

    #region Weapon Actions
    // 무기 장착
    public void Equip()
    {
        state.SetEquipMode();
    }

    // 무기 장착 시작
    public virtual void Begin_Equip()
    {
        bEquipping = true;
    }

    // 무기 장착 완료
    public virtual void End_Equip()
    {
        bEquipping = false;
        bEquipped = true;

        state.SetIdleMode();
    }

    // 무기 해제
    public virtual void UnEquip()
    {
        bEquipped = false;
    }

    // 액션(마우스 좌클릭 시 호출)
    public virtual void DoAction()
    {
        state.SetActionMode();                          // 액션 상태로 전환
        target?.TargetSearch();                         // 타겟팅 검색

        CheckStop(0);                                   // 정지 여부 확인
    }

    // 회피 상대의 액션(마우스 좌클릭 시 호출)
    public virtual void DodgedDoAction()
    {
        // 상속받은 클래스에서 구체화
    }

    // 액션 시작
    public virtual void Begin_DoAction()
    {
        // 상속받은 클래스에서 구체화
    }

    // 액션 종료
    public virtual void End_DoAction()
    {
        Move();                                         // 이동 처리
        target?.EndTargeting();                         // 타켓팅 종료
        
        // 공격 중 데미지를 받았을 때 Idle 모드로 전환 방지
        if (state.DamagedMode == true && state.DeadMode == true)
            return;

        state.SetIdleMode();                            // Idle로 상태 변경
    }

    // 회피 공격 액션 종료
    public virtual void End_DodgedDoAction()
    {

    }

    // 파티클 재생 (불 공격 등)
    public virtual void Play_Particle()
    {
        // 상속받은 클래스에서 구체화
    }

    // 파티클 인덱스 처리
    public virtual void Play_Particle_Index(AnimationEvent e)
    {
        // 상속받은 클래스에서 구체화
    }

    // 무기 사운드 재생
    public virtual void Play_Sound()
    {
        // 상속받은 클래스에서 구체화
    }
    #endregion

    #region Skill Actions


    // one 스킬
    public virtual void Activate01Skill()
    {
        animator.SetBool("IsSkillAction", true);        // 스킬 애니메이션 상태 설정
        animator.SetInteger("SkillType", 0);            // 스킬 타입 Q 설정

        state.SetUsingSkillMode();                      // 스킬 사용 상태로 전환
        state.SetSuperArmorMode();                      // 슈퍼아머 상태로 전환
    }

    // two 스킬
    public virtual void Activate02Skill()
    {
        animator.SetBool("IsSkillAction", true);        // 스킬 애니메이션 상태 설정
        animator.SetInteger("SkillType", 1);            // 스킬 타입 E 설정
        
        state.SetUsingSkillMode();                      // 스킬 사용 상태로 전환
        state.SetSuperArmorMode();                      // 슈퍼아머 상태로 전환
    }

    // three 스킬
    public virtual void Activate03Skill()
    {
        animator.SetBool("IsSkillAction", true);        // 스킬 애니메이션 상태 설정
        animator.SetInteger("SkillType", 2);            // 스킬 타입 E 설정

        state.SetUsingSkillMode();                      // 스킬 사용 상태로 전환
        state.SetSuperArmorMode();                      // 슈퍼아머 상태로 전환
    }

    // four 스킬
    public virtual void Activate04Skill()
    {
        animator.SetBool("IsSkillAction", true);        // 스킬 애니메이션 상태 설정
        animator.SetInteger("SkillType", 3);            // 스킬 타입 E 설정

        state.SetUsingSkillMode();                      // 스킬 사용 상태로 전환
        state.SetSuperArmorMode();                      // 슈퍼아머 상태로 전환
    }

    // 스킬 액션 종료. 애니메이션 이벤트에 의해 호출
    public virtual void End_SkillAction()
    {
        state.SetIdleMode();                            // Idle 상태 변환
        state.SetNormalMode();                          // 일반 피격 상태로 전환
    }

    public virtual void Play_01SkillParticles()
    {

    }
    public virtual void Play_02SkillParticles()
    {

    }
    public virtual void Play_03Skill()
    {
        /*3번째 스킬 구현*/
    }
    public virtual void Begin_Skill01VCam()
    {

    }
    public virtual void End_Skill01VCam()
    {

    }
    public virtual void Begin_Skill02VCam()
    {

    }
    public virtual void End_Skill02VCam()
    {

    }
    #endregion

    // 캐릭터 이동 처리
    protected void Move()
    {
        PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

        if (moving != null)
            moving.Move();
    }

    // 정지 여부 확인 및 처리
    protected void CheckStop(int index)
    {
        if (doActionDatas[index].bCanMove == false)
        {
            PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

            if (moving != null)
                moving.Stop();
        }
    }

    #region IWeaponCoolTime Interface Implementation
    // Q스킬 쿨타임 반환
    public float GetQSkillCooldown()
    {
        return QSkillDataCoolTime.CooldownTime;
    }

    // Q스킬 남은 쿨타임 반환
    public float GetQSkillCoolRemaining()
    {
        return QSkillDataCoolTime.RemainingCooldownTime;
    }

    // E스킬 쿨타임 반환
    public float GetESkillCooldown()
    {
        return ESkillDataCoolTime.CooldownTime;
    }

    // E스킬 남은 쿨타임 반환
    public float GetESkillCoolRemaining()
    {
        return ESkillDataCoolTime.RemainingCooldownTime;
    }

    public float GetSkillCooldown(SkillType2 skillType)
    {
        float coolTime = 0f;
        switch(skillType)
        {
            case SkillType2.one:
                coolTime = QSkillDataCoolTime.CooldownTime;
                break;
            case SkillType2.two:
                coolTime = ESkillDataCoolTime.CooldownTime;
                break;
            case SkillType2.three:
                coolTime = ThreeSkillDataCoolTime.CooldownTime;
                break;
            case SkillType2.four:
                coolTime = FourSkillDataCoolTime.CooldownTime;
                break;
        }

        return coolTime;
    }
    public float GetSkillCoolRemaining(SkillType2 skillType)
    {
        float coolTime = 0f;
        switch (skillType)
        {
            case SkillType2.one:
                coolTime = QSkillDataCoolTime.RemainingCooldownTime;
                break;
            case SkillType2.two:
                coolTime = ESkillDataCoolTime.RemainingCooldownTime;
                break;
            case SkillType2.three:
                coolTime = ThreeSkillDataCoolTime.RemainingCooldownTime;
                break;
            case SkillType2.four:
                coolTime = FourSkillDataCoolTime.RemainingCooldownTime;
                break;
        }

        return coolTime;
    }
    #endregion

    // 스킬 사용 시 카메라 임펄스 효과 재생
    public virtual void Play_SkillImpulse(AbilityType abilityType)
    {
        // Q스킬의 Impluse
        if (abilityType == AbilityType.Q)
        {
            if (QSkillData == null && impulse == null)
                return;

            if (QSkillData.ImpulseSettings == null)
                return;

            if (QSkillData.ImpulseDirection.magnitude <= 0.0f)
                return;

            CinemachineVirtualCamera camera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;

            if (camera != null)
            {
                CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
                listener.m_ReactionSettings.m_SecondaryNoise = QSkillData.ImpulseSettings;
            }

            impulse.GenerateImpulse(QSkillData.ImpulseDirection);
        }

        // E스킬의 Impluse
        if (abilityType == AbilityType.E) 
        {
            if (ESkillData == null && impulse == null)
                return;

            if (ESkillData.ImpulseSettings == null)
                return;

            if (ESkillData.ImpulseDirection.magnitude <= 0.0f)
                return;

            CinemachineVirtualCamera camera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;

            if (camera != null)
            {
                CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
                listener.m_ReactionSettings.m_SecondaryNoise = ESkillData.ImpulseSettings;
            }

            impulse.GenerateImpulse(ESkillData.ImpulseDirection);
        }
    }

    // 추가 공격 충돌 처리(임의로 피격 공간을 만들 때 사용)
    public virtual void Create_Attack_Collision()
    {
        // 상속받은 클래스에서 구체화
    }

    private int GetSkillTypeToInt(SkillType2 skillType)
    {
        int idx = -1;
        switch(skillType)
        {
            case SkillType2.None: idx = -1;
                break;
            case SkillType2.one: idx = 0;
                break;
            case SkillType2.two: idx = 1;
                break;
            case SkillType2.three: idx = 2;
                break;
            case SkillType2.four: idx = 3;
                break;
        }

        return idx;
    }
}