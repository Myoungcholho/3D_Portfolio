using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DualSword;

public enum AbilityType
{
    None =0,        // None�� ��� �ǰ� �� ī�޶� ����ŷ
    Q,              // Q ��ų
    E               // E ��ų
}


// ���� ������
[Serializable]
public class DoActionData
{
    public AbilityType abilityType;                     // ��ų Q,E ������ ���� ����
    public bool isObjectPushDisperse;                   // ĳ���� �ڷθ� �и��� �ʰ� �л��ؼ� �и��� ����
    public bool bCanMove;                               // �̵� ���� ����

    public float Power;                                 // ������
    public float Distance;                              // �и��� �Ÿ�
    public int StopFrame;                               // ���ߴ� ������ ��

    public GameObject Particle;                         // ��ƼŬ ȿ��
    public Vector3 ParticlePositionOffset;              // ��ƼŬ ��ġ ������
    public Vector3 ParticleScaleOffset = Vector3.one;   // ��ƼŬ ũ�� ������

    public int HitImpactIndex;                          // Ÿ�� ȿ�� �ε���
    public GameObject HitParticle;                      // Ÿ�� ��ƼŬ
    public Vector3 HitParticlePositionOffset;           // Ÿ�� ��ƼŬ ��ġ ������
    public Vector3 HitParticleScaleOffset = Vector3.one;// Ÿ�� ��ƼŬ ũ�� ������

    public Vector3 ImpulseDirection;                    // ī�޶� ���޽� ����
    public Cinemachine.NoiseSettings ImpulseSettings;   // ī�޶� ���޽� ����
}

// ��ų�� ������ �߰� ������
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

[Serializable]
public class HandActionData
{
    public PartType partType;                 // �޼�/������ ����
    public List<DoActionData> actionData;     // �ش� ���� �׼� ������
}

// Weapon �߻� Ŭ����: ����� ���õ� �⺻ ����� ����
public abstract class Weapon : MonoBehaviour , IWeaponCoolTime
{
    // ���� Ÿ��
    [SerializeField]
    protected WeaponType type;                      

    // ���� ������ �迭 (�޺�)
    [SerializeField]
    protected DoActionData[] doActionDatas;

    // ���� ������ �迭 (�޺�, ���)
    [SerializeField]
    protected bool IsBothHandAction = false;
    [SerializeField]
    protected List<HandActionData> handActionDataList;
    protected Dictionary<PartType, List<DoActionData>> handActionData;

    // ������ ���� ����
    protected bool bEquipped;                       
    protected bool bEquipping;                      
    public bool Equipping { get => bEquipping; }

    // ���� Ÿ�� ��ȯ
    public WeaponType Type { get => type; }

    // ������ �޴� ������Ʈ
    protected GameObject rootObject;
    protected StateComponent state;
    protected Animator animator;
    protected SoundComponent soundComponent;
    protected DecalComponent decalComponent;
    protected PlayerMovingComponent playerMoving;
    protected Player playerInput;
    protected TargetComponent target;

    // ��ų ���� ������Ʈ
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
        // ��� ���� Ŭ�������� ��üȭ
    }

    protected virtual void Awake()
    {
        rootObject = transform.parent.gameObject;
        Debug.Assert(rootObject != null);

        // ������Ʈ ����
        state = rootObject.GetComponent<StateComponent>();
        animator = rootObject.GetComponent<Animator>();
        soundComponent = rootObject.GetComponent<SoundComponent>();
        decalComponent = rootObject.GetComponent<DecalComponent>();

        // ī�޶� ���� ������Ʈ �ʱ�ȭ
        brain = Camera.main.GetComponent<CinemachineBrain>();
        impulse = GetComponent<CinemachineImpulseSource>();

        // �÷��̾� �̵� �� Ÿ�� ������Ʈ �ʱ�ȭ
        playerMoving = rootObject.GetComponent<PlayerMovingComponent>();
        playerInput = rootObject.GetComponent<Player>();
        target = rootObject.GetComponent<TargetComponent>();
    }

    protected virtual void Start()
    {
        // ��ӹ��� Ŭ�������� ��üȭ
    }

    protected virtual void Update()
    {
        // ��ӹ��� Ŭ�������� ��üȭ
    }

    #region Weapon Actions
    // ���� ����
    public void Equip()
    {
        state.SetEquipMode();
    }

    // ���� ���� ����
    public virtual void Begin_Equip()
    {
        bEquipping = true;
    }

    // ���� ���� �Ϸ�
    public virtual void End_Equip()
    {
        bEquipping = false;
        bEquipped = true;

        state.SetIdleMode();
    }

    // ���� ����
    public virtual void UnEquip()
    {
        bEquipped = false;
    }

    // �׼�(���콺 ��Ŭ�� �� ȣ��)
    public virtual void DoAction()
    {
        state.SetActionMode();                          // �׼� ���·� ��ȯ
        target?.TargetSearch();                         // Ÿ���� �˻�

        CheckStop(0);                                   // ���� ���� Ȯ��
    }

    // ȸ�� ����� �׼�(���콺 ��Ŭ�� �� ȣ��)
    public virtual void DodgedDoAction()
    {
        // ��ӹ��� Ŭ�������� ��üȭ
    }

    // �׼� ����
    public virtual void Begin_DoAction()
    {
        // ��ӹ��� Ŭ�������� ��üȭ
    }

    // �׼� ����
    public virtual void End_DoAction()
    {
        Move();                                         // �̵� ó��
        target?.EndTargeting();                         // Ÿ���� ����
        
        // ���� �� �������� �޾��� �� Idle ���� ��ȯ ����
        if (state.DamagedMode == true && state.DeadMode == true)
            return;

        state.SetIdleMode();                            // Idle�� ���� ����
    }

    // ȸ�� ���� �׼� ����
    public virtual void End_DodgedDoAction()
    {

    }

    // ��ƼŬ ��� (�� ���� ��)
    public virtual void Play_Particle()
    {
        // ��ӹ��� Ŭ�������� ��üȭ
    }

    // ��ƼŬ �ε��� ó��
    public virtual void Play_Particle_Index(AnimationEvent e)
    {
        // ��ӹ��� Ŭ�������� ��üȭ
    }

    // ���� ���� ���
    public virtual void Play_Sound()
    {
        // ��ӹ��� Ŭ�������� ��üȭ
    }
    #endregion

    #region Skill Actions


    // one ��ų
    public virtual void Activate01Skill()
    {
        animator.SetBool("IsSkillAction", true);        // ��ų �ִϸ��̼� ���� ����
        animator.SetInteger("SkillType", 0);            // ��ų Ÿ�� Q ����

        state.SetUsingSkillMode();                      // ��ų ��� ���·� ��ȯ
        state.SetSuperArmorMode();                      // ���۾Ƹ� ���·� ��ȯ
    }

    // two ��ų
    public virtual void Activate02Skill()
    {
        animator.SetBool("IsSkillAction", true);        // ��ų �ִϸ��̼� ���� ����
        animator.SetInteger("SkillType", 1);            // ��ų Ÿ�� E ����
        
        state.SetUsingSkillMode();                      // ��ų ��� ���·� ��ȯ
        state.SetSuperArmorMode();                      // ���۾Ƹ� ���·� ��ȯ
    }

    // three ��ų
    public virtual void Activate03Skill()
    {
        animator.SetBool("IsSkillAction", true);        // ��ų �ִϸ��̼� ���� ����
        animator.SetInteger("SkillType", 2);            // ��ų Ÿ�� E ����

        state.SetUsingSkillMode();                      // ��ų ��� ���·� ��ȯ
        state.SetSuperArmorMode();                      // ���۾Ƹ� ���·� ��ȯ
    }

    // four ��ų
    public virtual void Activate04Skill()
    {
        animator.SetBool("IsSkillAction", true);        // ��ų �ִϸ��̼� ���� ����
        animator.SetInteger("SkillType", 3);            // ��ų Ÿ�� E ����

        state.SetUsingSkillMode();                      // ��ų ��� ���·� ��ȯ
        state.SetSuperArmorMode();                      // ���۾Ƹ� ���·� ��ȯ
    }

    // ��ų �׼� ����. �ִϸ��̼� �̺�Ʈ�� ���� ȣ��
    public virtual void End_SkillAction()
    {
        state.SetIdleMode();                            // Idle ���� ��ȯ
        state.SetNormalMode();                          // �Ϲ� �ǰ� ���·� ��ȯ
    }

    public virtual void Play_01SkillParticles()
    {

    }
    public virtual void Play_02SkillParticles()
    {

    }
    public virtual void Play_03Skill()
    {
        /*3��° ��ų ����*/
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

    // ĳ���� �̵� ó��
    protected void Move()
    {
        PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

        if (moving != null)
            moving.Move();
    }

    // ���� ���� Ȯ�� �� ó��
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
    // Q��ų ��Ÿ�� ��ȯ
    public float GetQSkillCooldown()
    {
        return QSkillDataCoolTime.CooldownTime;
    }

    // Q��ų ���� ��Ÿ�� ��ȯ
    public float GetQSkillCoolRemaining()
    {
        return QSkillDataCoolTime.RemainingCooldownTime;
    }

    // E��ų ��Ÿ�� ��ȯ
    public float GetESkillCooldown()
    {
        return ESkillDataCoolTime.CooldownTime;
    }

    // E��ų ���� ��Ÿ�� ��ȯ
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

    // ��ų ��� �� ī�޶� ���޽� ȿ�� ���
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

    // �߰� ���� �浹 ó��(���Ƿ� �ǰ� ������ ���� �� ���)
    public virtual void Create_Attack_Collision()
    {
        // ��ӹ��� Ŭ�������� ��üȭ
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