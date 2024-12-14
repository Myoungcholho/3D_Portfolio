using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

// Melee 클래스: Weapon 클래스 상속. 근접 무기 관련 동작 구현
public class Melee : Weapon
{
    private bool bEnable;       // 콤보 활성화 여부
    private bool bExist;        // 콤보 중 여부
    protected int index;        // 콤보 인덱스

    // 스킬 공격 시 콤보 정보를 저장
    public int Index
    {
        set { index = value; }
    }
    protected Collider[] colliders;             // 무기에 적용된 Collider 배열
    protected List<string> hittedList;            // 히트한 객체의 해시 리스트 (중복 히트 방지)
    protected GameObject attacker;                // 공격자 오브젝트

    // 공격자 설정
    public void Attacker(GameObject attacker)
    {
        this.attacker = attacker;
    }

    protected override void Awake()
    {
        base.Awake();

        colliders = GetComponentsInChildren<Collider>();
        hittedList = new List<string>();
    }

    protected override void Start()
    {
        base.Start();

        End_Collision();
    }

    // 콜라이더 활성화 (공격 시작)
    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true;
    }

    // 콜라이더 비활성화 (공격 종료)
    public virtual void End_Collision()
    {
        foreach (Collider collider in colliders)
            collider.enabled = false;

        hittedList.Clear();
    }

    // 콤보 시작
    public void Begin_Combo()
    {
        bEnable = true;
    }

    // 콤보 종료
    public void End_Combo()
    {
        bEnable = false;
    }

    // 공격 액션 처리
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

    // 공격 시작 처리
    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        // 콤보 상태가 아니거나 데미지 상태이면 중단
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

    // 공격 종료 처리
    public override void End_DoAction()
    {
        base.End_DoAction();

        index = 0;                          // 콤보 인덱스 초기화
        bEnable = false;                    // 콤보 종료
    }

    // 히트 시 해시 코드 생성 (중복 방지용)
    public string CreateHashCode(Collider other, GameObject obj)
    {
        if (other == null)
            return string.Empty;

        if (obj == null)
            obj = gameObject;

        return $"{other.name}_{obj.name}";
    }

    // 카메라 임펄스 효과 실행
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

    // 데미지 처리 (Trigger 충돌)
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        if (attacker == null)
            attacker = gameObject;
        string hashCode = CreateHashCode(other, attacker);

        if (hittedList.Contains(hashCode) == true)
            return;

        hittedList.Add(hashCode);                            // 히트한 객체 저장


        // 회피 카운터 공격 처리
        if (state != null && (state.DodgedMode || state.DodgedAttackMode))
        {
            ApplyDodgeCounterDamage(other);
            return;
        }

        ApplyDamage(other);
    }


        // 일반/반격 공격 데미지 처리 공통 메서드
        private void ApplyDamage<T>(Collider other, Action<GameObject, Weapon, Vector3, DoActionData> damageAction) where T : class
        {
            T damageHandler = other.gameObject.GetComponent<T>();

            if (damageHandler == null)
                return;

            Vector3 hitPoint = Vector3.zero;

            // 공격자와 일치하는 콜라이더로 충돌 지점 계산
            Collider enabledCollider = null;
            foreach (Collider collider in colliders)
            {
                if (collider.name.Equals(attacker.name) == true)
                {
                    enabledCollider = collider;
                    break;
                }
            }

            // 내 검의 위치와 적의 위치의 가장 가까운 지점 반환
            // 
            hitPoint = enabledCollider.ClosestPoint(other.transform.position);
            // 
            hitPoint = other.transform.InverseTransformPoint(hitPoint);

            // 호출할 메서드를 파라미터로 받아 처리
            damageAction(rootObject, this, hitPoint, doActionDatas[index]);
        }

        // 기존의 일반 공격 데미지 처리
        private void ApplyDamage(Collider other)
        {
            ApplyDamage<IDamagable>(other, (rootObj, weapon, hitPoint, data) =>
            {
                var damage = other.gameObject.GetComponent<IDamagable>();
                damage?.OnDamage(rootObj, weapon, hitPoint, data);
            });
        }

        // 기존의 반격 공격 데미지 처리
        private void ApplyDodgeCounterDamage(Collider other)
        {
            ApplyDamage<IDodgeDamageHandler>(other, (rootObj, weapon, hitPoint, data) =>
            {
                var damage = other.gameObject.GetComponent<IDodgeDamageHandler>();
                damage?.OnDodgeDamage(rootObj, weapon, hitPoint, data);
            });
        }
}