using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 플레이어 캐릭터 클래스 (Character를 상속받음)
public class Player : Character, IDamagable
{
    [SerializeField]
    private string surfaceText = "Erika_Archer_Body_Mesh";
    private Material skinMaterial;                  // 데미지 타격 시 material변경

    [SerializeField]
    private Color damageColor;                      // 데미지 시 색상
    [SerializeField]
    private Color evadeColor;                       // 회피 성공 시 색상

    [SerializeField]
    private float changeColorTime = 0.15f;          // 색상 변경 시간

    private Color originColor;

    // Evade 성공 시 잔상 효과
    private MeshTrail trail;
    public Action<bool> OnDodgeAttack;

    private SoundComponent soundComponent;

    // 무기 빠른 스위칭 UI
    private WeaponChangeUI weaponChangeUI;

    // 화면 커서 가시성 여부
    private CursorComponent cursorComponent;

    // 초기 설정 및 입력 매핑 처리
    protected override void Awake()
    {
        base.Awake();

        trail = GetComponent<MeshTrail>();
        soundComponent = GetComponent<SoundComponent>();
        cursorComponent = GetComponent<CursorComponent>();
        weaponChangeUI = FindObjectOfType<WeaponChangeUI>();
        if(weaponChangeUI == null )
        {
            Debug.Log("WeaponChangeUI is null");
        }
        


        // PlayerInput 설정 및 입력 매핑
        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        // 무기 모드 변경 입력 설정
        actionMap.FindAction("Sword").started += context =>
        {
            weapon.SetSwordMode();
        };

        actionMap.FindAction("FireBall").started += context =>
        {
            weapon.SetFireBallMode();
        };

        actionMap.FindAction("DualSword").started += context =>
        {
            weapon.SetDualSwordMode();
        };

        actionMap.FindAction("Fist").started += context =>
        {
            weapon.SetFistMode();
        };

        actionMap.FindAction("Hammer").started += context =>
        {
            weapon.SetHammerMode();
        };

        // 공격 액션 입력 설정
        actionMap.FindAction("Action").started += context =>
        {
            // UI 위에서 클릭했는지 확인
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            if (state.DodgedMode || state.DodgedAttackMode)
            {
                // 회피 공격 처리
                weapon.DodgedDoAction();
                return;
            }

            bool bCheck = false;
            bCheck |= state.EvadeMode;

            if (bCheck)
                return;

            weapon.DoAction();
        };

        // 회피 액션 입력 설정
        actionMap.FindAction("Evade").started += context =>
        {
            bool bCheck = false;
            bCheck |= state.DamagedMode == true;
            bCheck |= state.EquipMode == true;
            bCheck |= state.DodgedMode == true;
            bCheck |= state.InstantKillMode == true;
            bCheck |= state.ActionMode == true;                 // 공격 중 회피 금지

            if (bCheck)
                return;

            state.SetEvadeMode();
        };

        // 스킬 입력 설정
        actionMap.FindAction("Skill01").started += context =>
        {
            weapon.ActivateQSkill();
        };

        actionMap.FindAction("Skill02").started += context =>
        {
            weapon.ActivateESkill();
        };

        // 판넬 키기
        actionMap.FindAction("FastEquip").started += context =>
        {
            weaponChangeUI.ToggleUIPanel(true);
            cursorComponent.ShowCursorForUI();
        };

        // 판넬 끄기
        actionMap.FindAction("FastEquip").canceled += context =>
        {
            weaponChangeUI.ToggleUIPanel(false);
            cursorComponent.HideCursorForUI();
        };

        // 스킨 머티리얼 설정
        Transform surface = transform.FindChildByName(surfaceText);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;

    }

    public GameObject lastAttacker;                // 마지막 공격자를 저장 (반격 시 사용)

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        lastAttacker = attacker;        // 마지막 공격자를 기록, 대상으로 날라가기 위해

        // 회피 모드일 경우 회피 처리
        if (state.EvadeMode == true)
        {
            trail.ActivateMeshTrail();      // 잔상 효과 활성화
            state.SetDodgedMode();          // 회피 모드 전환
            soundComponent.PlayLocalSound(SoundLibrary.Instance.evadeDodage01, SoundLibrary.Instance.mixerBasic, false);
            StartCoroutine(MovableStopper.Instance.EvadeDelay());
            OnDodgeAttack?.Invoke(true);    
            return;
        }

        // 무적 모드일 경우 데미지 처리하지 않음
        if (state.InvincibleMode == true)
            return;

        // 체력 감소 처리
        healthPoint.Damage(data.Power);

        // 슈퍼 아머 모드일 경우 데미지 후 처리하지 않음
        if (state.SuperArmorMode == true)
            return;

        // 카메라 흔들림 처리
        HitCameraShake(causer);

        // 색상 변경 처리 (피격 시)
        StartCoroutine(Change_Color(changeColorTime,damageColor));
        // 피격으로 인한 일시적 정지
        MovableStopper.Instance.Start_Delay(data.StopFrame);

        // 사망하지 않았다면 피격 상태 설정
        if (healthPoint.Dead == false)
        {
            state.SetDamagedMode();

            animator.SetInteger("ImpactType", (int)causer.Type);
            animator.SetInteger("ImpactIndex", (int)data.HitImpactIndex);
            animator.SetTrigger("Impact");

            return;
        }
        // 사망 처리
        state.SetDeadMode();

        Collider collider = GetComponent<Collider>();
        collider.enabled = false;

        animator.SetTrigger("Dead");
        Destroy(gameObject, 5f);
    }

    // 카메라 흔들림 처리
    private void HitCameraShake(Weapon causer)
    {
        Melee melee = causer as Melee;
        melee?.Play_Impulse();
    }

    // 색상 변경 처리 (타이머 기반)
    private IEnumerator Change_Color(float time, Color changeColor)
    {
        skinMaterial.color = changeColor;
        yield return new WaitForSeconds(time);
        skinMaterial.color = originColor;
    }

    // 회피 성공 시 색상 변경
    private void EvadeSuccessColor()
    {
        StartCoroutine(Change_Color(changeColorTime,evadeColor));
    }

    // 특수 객체 여부 확인
    public override bool IsSpecialObject()
    {
        return true;
    }

    // 정상 상태로 복귀
    public void ResetToNormal()
    {
        state.SetIdleMode();
        animator.SetBool("IsDodgedAction", false);
    }
}
