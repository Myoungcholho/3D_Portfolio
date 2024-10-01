using Cinemachine;
using System;
using UnityEngine;

public enum AbilityType
{
    None =0,        // None의 경우 피격 시 카메라 쉐이킹
    Q,
    E
}


[Serializable]
public class DoActionData
{
    public AbilityType abilityType;             // 스킬 Q,E 구분을 위한 변수
    public bool isObjectPushDisperse;           // 캐릭터 뒤로만 밀리는게 아니라 분산해서 밀릴지
    public bool bCanMove;

    public float Power;
    public float Distance;
    public int StopFrame;

    public GameObject Particle;
    public Vector3 ParticlePositionOffset;
    public Vector3 ParticleScaleOffset = Vector3.one;

    public int HitImpactIndex;
    public GameObject HitParticle;
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleScaleOffset = Vector3.one;

    public Vector3 ImpulseDirection;
    public Cinemachine.NoiseSettings ImpulseSettings;
}

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

public abstract class Weapon : MonoBehaviour , IWeaponCoolTime
{
    [SerializeField]
    protected WeaponType type;

    [SerializeField]
    protected DoActionData[] doActionDatas;


    protected bool bEquipped;                       // 장착을 하고 있는지
    protected bool bEquipping;                      // 장착이 진행 중인지
    public bool Equipping { get => bEquipping; }


    public WeaponType Type { get => type; }

    protected GameObject rootObject;
    
    protected StateComponent state;
    protected Animator animator;
    protected SoundComponent soundComponent;
    protected DecalComponent decalComponent;

    [Header("-Skill-")]
    [SerializeField]
    protected GameObject qSkillParticlePrefab;
    [SerializeField]
    protected GameObject eSkillParticlePrefab;

    protected CinemachineVirtualCamera skill01Camera;
    protected CinemachineVirtualCamera skill02Camera;

    [SerializeField]
    protected DoActionData QSkillData;
    [SerializeField]
    protected DoActionSkillData QSkillDataCoolTime;

    [SerializeField]
    protected DoActionData ESkillData;
    [SerializeField]
    protected DoActionSkillData ESkillDataCoolTime;

    protected CinemachineBrain brain;
    protected CinemachineImpulseSource impulse;

    protected virtual void Reset()
    {

    }

    protected virtual void Awake()
    {
        rootObject = transform.parent.gameObject;
        Debug.Assert(rootObject != null);

        state = rootObject.GetComponent<StateComponent>();
        animator = rootObject.GetComponent<Animator>();
        soundComponent = rootObject.GetComponent<SoundComponent>();
        decalComponent = rootObject.GetComponent<DecalComponent>();

        // 카메라
        brain = Camera.main.GetComponent<CinemachineBrain>();
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    public void Equip()
    {
        state.SetEquipMode();
    }

    public virtual void Begin_Equip()
    {
        bEquipping = true;
    }

    public virtual void End_Equip()
    {
        bEquipping = false;
        bEquipped = true;

        state.SetIdleMode();
    }

    public virtual void UnEquip()
    {
        bEquipped = false;
    }

    public virtual void DoAction()
    {
        state.SetActionMode();

        CheckStop(0);
    }

    public virtual void Begin_DoAction()
    {

    }

    public virtual void End_DoAction()
    {
        Move();

        // 공격 중 데미지를 받으면 Damage로 변경되는 동시에
        // 공격 애니메이션이 끝나서 Damage상태임에도 Idle모드로 전환되는 문제로
        // return함.
        if (state.DamagedMode == true)
            return;

        state.SetIdleMode();
    }

    // 불 쏠떄 쓸라고 만듬
    public virtual void Play_Particle()
    {

    }

    //파티클 인덱스 처리용
    public virtual void Play_Particle_Index(AnimationEvent e)
    {

    }

    //무기 사운드 실행용
    public virtual void Play_Sound()
    {

    }

    #region Skill
    public virtual void ActivateQSkill()
    {
        animator.SetBool("IsSkillAction", true);
        animator.SetInteger("SkillType", 0);

        state.SetUsingSkillMode();
        state.SetSuperArmorMode();
    }

    public virtual void ActivateESkill()
    {
        animator.SetBool("IsSkillAction", true);
        animator.SetInteger("SkillType", 1);

        state.SetUsingSkillMode();
        state.SetSuperArmorMode();
    }
    // 스킬이 끝날 때 호출
    public virtual void End_SkillAction()
    {
        state.SetIdleMode();
        state.SetNormalMode();
    }
    public virtual void Play_QSkillParticles()
    {

    }
    public virtual void Play_ESkillParticles()
    {

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

    protected void Move()
    {
        PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

        if (moving != null)
            moving.Move();
    }

    protected void CheckStop(int index)
    {
        if (doActionDatas[index].bCanMove == false)
        {
            PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

            if (moving != null)
                moving.Stop();
        }
    }

    #region Interface
    public float GetQSkillCooldown()
    {
        return QSkillDataCoolTime.CooldownTime;
    }

    public float GetQSkillCoolRemaining()
    {
        return QSkillDataCoolTime.RemainingCooldownTime;
    }

    public float GetESkillCooldown()
    {
        return ESkillDataCoolTime.CooldownTime;
    }

    public float GetESkillCoolRemaining()
    {
        return ESkillDataCoolTime.RemainingCooldownTime;
    }
    #endregion

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
}