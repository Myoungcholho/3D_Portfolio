using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class Melee : Weapon
{
    private bool bEnable;
    private bool bExist;
    protected int index;        // comboIndex확인용
    public int Index
    {
        set { index = value; }
    }
    protected Collider[] colliders;
    private List<string> hittedList;
    private GameObject attacker;

    protected CinemachineImpulseSource impulse;
    protected CinemachineBrain brain;

    public void Attacker(GameObject attacker)
    {
        this.attacker = attacker;
    }

    protected override void Awake()
    {
        base.Awake();

        colliders = GetComponentsInChildren<Collider>();
        hittedList = new List<string>();
        impulse = GetComponent<CinemachineImpulseSource>();
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    protected override void Start()
    {
        base.Start();

        End_Collision();
    }

    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true;


    }

    public virtual void End_Collision()
    {
        foreach (Collider collider in colliders)
            collider.enabled = false;



        hittedList.Clear();
    }

    public void Begin_Combo()
    {
        bEnable = true;
    }

    public void End_Combo()
    {
        bEnable = false;
    }

    public override void DoAction()
    {
        if (bEnable)
        {
            bEnable = false;
            bExist = true;

            return;
        }


        if (state.IdleMode == false)
            return;

        base.DoAction();
    }

    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        if (bExist == false)
            return;
        if (state.DamagedMode == true)
            return;

        bExist = false;

        index++;
        animator.SetTrigger("NextCombo");


        if (doActionDatas[index].bCanMove)
        {
            Move();

            return;
        }

        CheckStop(index);
    }

    public override void End_DoAction()
    {
        base.End_DoAction();

        index = 0;
        bEnable = false;
    }

    public string CreateHashCode(Collider other,GameObject obj)
    {
        if (other == null)
            return string.Empty;

        if (obj == null)
            obj = gameObject;

        return $"{other.name}_{obj.name}";
    }

    public virtual void Play_Impulse()
    {
        if (impulse == null)
            return;

        if (doActionDatas[index].ImpulseSettings == null)
            return;

        if (doActionDatas[index].ImpulseDirection.magnitude <= 0.0f)
            return;

        //brain.m_CameraCutEvent 카메라 활성화 될때마다 콜
        CinemachineVirtualCamera camera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        if (camera != null)
        {
            CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
            listener.m_ReactionSettings.m_SecondaryNoise = doActionDatas[index].ImpulseSettings;

        }

        impulse.GenerateImpulse(doActionDatas[index].ImpulseDirection);
    }

    // 데미지 처리
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        if (attacker == null)
            attacker = gameObject;
        string hashCode = CreateHashCode(other, attacker);

        if (hittedList.Contains(hashCode) == true)
            return;

        hittedList.Add(hashCode);

        
        if(state != null && (state.DodgedMode || state.DodgedAttackMode))
        {
            ApplyDodgeCounterDamage(other);
            return;
        }

        ApplyDamage(other);
    }

    // 일반 공격 데미지 저리
    private void ApplyDamage(Collider other)
    {
        IDamagable damage = other.gameObject.GetComponent<IDamagable>();

        if (damage == null)
            return;


        Vector3 hitPoint = Vector3.zero;

        Collider enabledCollider = null;
        foreach (Collider collider in colliders)
        {
            if (collider.name.Equals(attacker.name) == true)
            {
                enabledCollider = collider;

                break;
            }
        }

        hitPoint = enabledCollider.ClosestPoint(other.transform.position);
        hitPoint = other.transform.InverseTransformPoint(hitPoint);

        damage.OnDamage(rootObject, this, hitPoint, doActionDatas[index]);
    }

    // 반격 공격 데미지 처리
    private void ApplyDodgeCounterDamage(Collider other)
    {
        IDodgeDamageHandler damage = other.gameObject.GetComponent<IDodgeDamageHandler>();

        if(damage == null) 
            return;

        Vector3 hitPoint = Vector3.zero;

        Collider enabledCollider = null;
        foreach (Collider collider in colliders)
        {
            if (collider.name.Equals(attacker.name) == true)
            {
                enabledCollider = collider;

                break;
            }
        }

        hitPoint = enabledCollider.ClosestPoint(other.transform.position);
        hitPoint = other.transform.InverseTransformPoint(hitPoint);

        damage.OnDodgeDamage(rootObject, this, hitPoint, doActionDatas[index]);
    }
}