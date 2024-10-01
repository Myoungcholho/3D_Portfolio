using Cinemachine;
using System;
using UnityEngine;

public enum AbilityType
{
    None =0,        // None�� ��� �ǰ� �� ī�޶� ����ŷ
    Q,
    E
}


[Serializable]
public class DoActionData
{
    public AbilityType abilityType;             // ��ų Q,E ������ ���� ����
    public bool isObjectPushDisperse;           // ĳ���� �ڷθ� �и��°� �ƴ϶� �л��ؼ� �и���
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
    public float CooldownTime;                  // ��ų ��Ÿ��
    public float RemainingCooldownTime;         // ��ų ���� ���� ��Ÿ��

    public float ColliderDelay;                 // �ݶ��̴� ������ �ð�
    public float ColliderDuration;              // �ݶ��̴� ���� �ð�
    public float MultiHitInterval;              // �ٴ���Ʈ �ð�
    public int MultiHitCount;                   // �ٴ���Ʈ Ƚ��
}

public abstract class Weapon : MonoBehaviour , IWeaponCoolTime
{
    [SerializeField]
    protected WeaponType type;

    [SerializeField]
    protected DoActionData[] doActionDatas;


    protected bool bEquipped;                       // ������ �ϰ� �ִ���
    protected bool bEquipping;                      // ������ ���� ������
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

        // ī�޶�
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

        // ���� �� �������� ������ Damage�� ����Ǵ� ���ÿ�
        // ���� �ִϸ��̼��� ������ Damage�����ӿ��� Idle���� ��ȯ�Ǵ� ������
        // return��.
        if (state.DamagedMode == true)
            return;

        state.SetIdleMode();
    }

    // �� �� ����� ����
    public virtual void Play_Particle()
    {

    }

    //��ƼŬ �ε��� ó����
    public virtual void Play_Particle_Index(AnimationEvent e)
    {

    }

    //���� ���� �����
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
    // ��ų�� ���� �� ȣ��
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
        // Q��ų�� Impluse
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

        // E��ų�� Impluse
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