using System;
using UnityEngine;

[Serializable]
public class DoActionData
{
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

public abstract class Weapon : MonoBehaviour
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

    protected virtual void Reset()
    {

    }

    protected virtual void Awake()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);

        state = rootObject.GetComponent<StateComponent>();
        animator = rootObject.GetComponent<Animator>();
        soundComponent = rootObject.GetComponent<SoundComponent>();
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
}