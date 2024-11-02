using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// �÷��̾� ĳ���� Ŭ���� (Character�� ��ӹ���)
public class Player : Character, IDamagable
{
    [SerializeField]
    private string surfaceText = "Erika_Archer_Body_Mesh";
    private Material skinMaterial;                  // ������ Ÿ�� �� material����

    [SerializeField]
    private Color damageColor;                      // ������ �� ����
    [SerializeField]
    private Color evadeColor;                       // ȸ�� ���� �� ����

    [SerializeField]
    private float changeColorTime = 0.15f;          // ���� ���� �ð�

    private Color originColor;

    // Evade ���� �� �ܻ� ȿ��
    private MeshTrail trail;
    public Action<bool> OnDodgeAttack;

    private SoundComponent soundComponent;

    // ���� ���� ����Ī UI
    private WeaponChangeUI weaponChangeUI;

    // ȭ�� Ŀ�� ���ü� ����
    private CursorComponent cursorComponent;

    // �ʱ� ���� �� �Է� ���� ó��
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
        


        // PlayerInput ���� �� �Է� ����
        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        // ���� ��� ���� �Է� ����
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

        // ���� �׼� �Է� ����
        actionMap.FindAction("Action").started += context =>
        {
            // UI ������ Ŭ���ߴ��� Ȯ��
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            if (state.DodgedMode || state.DodgedAttackMode)
            {
                // ȸ�� ���� ó��
                weapon.DodgedDoAction();
                return;
            }

            bool bCheck = false;
            bCheck |= state.EvadeMode;

            if (bCheck)
                return;

            weapon.DoAction();
        };

        // ȸ�� �׼� �Է� ����
        actionMap.FindAction("Evade").started += context =>
        {
            bool bCheck = false;
            bCheck |= state.DamagedMode == true;
            bCheck |= state.EquipMode == true;
            bCheck |= state.DodgedMode == true;
            bCheck |= state.InstantKillMode == true;
            bCheck |= state.ActionMode == true;                 // ���� �� ȸ�� ����

            if (bCheck)
                return;

            state.SetEvadeMode();
        };

        // ��ų �Է� ����
        actionMap.FindAction("Skill01").started += context =>
        {
            weapon.ActivateQSkill();
        };

        actionMap.FindAction("Skill02").started += context =>
        {
            weapon.ActivateESkill();
        };

        // �ǳ� Ű��
        actionMap.FindAction("FastEquip").started += context =>
        {
            weaponChangeUI.ToggleUIPanel(true);
            cursorComponent.ShowCursorForUI();
        };

        // �ǳ� ����
        actionMap.FindAction("FastEquip").canceled += context =>
        {
            weaponChangeUI.ToggleUIPanel(false);
            cursorComponent.HideCursorForUI();
        };

        // ��Ų ��Ƽ���� ����
        Transform surface = transform.FindChildByName(surfaceText);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;

    }

    public GameObject lastAttacker;                // ������ �����ڸ� ���� (�ݰ� �� ���)

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        lastAttacker = attacker;        // ������ �����ڸ� ���, ������� ���󰡱� ����

        // ȸ�� ����� ��� ȸ�� ó��
        if (state.EvadeMode == true)
        {
            trail.ActivateMeshTrail();      // �ܻ� ȿ�� Ȱ��ȭ
            state.SetDodgedMode();          // ȸ�� ��� ��ȯ
            soundComponent.PlayLocalSound(SoundLibrary.Instance.evadeDodage01, SoundLibrary.Instance.mixerBasic, false);
            StartCoroutine(MovableStopper.Instance.EvadeDelay());
            OnDodgeAttack?.Invoke(true);    
            return;
        }

        // ���� ����� ��� ������ ó������ ����
        if (state.InvincibleMode == true)
            return;

        // ü�� ���� ó��
        healthPoint.Damage(data.Power);

        // ���� �Ƹ� ����� ��� ������ �� ó������ ����
        if (state.SuperArmorMode == true)
            return;

        // ī�޶� ��鸲 ó��
        HitCameraShake(causer);

        // ���� ���� ó�� (�ǰ� ��)
        StartCoroutine(Change_Color(changeColorTime,damageColor));
        // �ǰ����� ���� �Ͻ��� ����
        MovableStopper.Instance.Start_Delay(data.StopFrame);

        // ������� �ʾҴٸ� �ǰ� ���� ����
        if (healthPoint.Dead == false)
        {
            state.SetDamagedMode();

            animator.SetInteger("ImpactType", (int)causer.Type);
            animator.SetInteger("ImpactIndex", (int)data.HitImpactIndex);
            animator.SetTrigger("Impact");

            return;
        }
        // ��� ó��
        state.SetDeadMode();

        Collider collider = GetComponent<Collider>();
        collider.enabled = false;

        animator.SetTrigger("Dead");
        Destroy(gameObject, 5f);
    }

    // ī�޶� ��鸲 ó��
    private void HitCameraShake(Weapon causer)
    {
        Melee melee = causer as Melee;
        melee?.Play_Impulse();
    }

    // ���� ���� ó�� (Ÿ�̸� ���)
    private IEnumerator Change_Color(float time, Color changeColor)
    {
        skinMaterial.color = changeColor;
        yield return new WaitForSeconds(time);
        skinMaterial.color = originColor;
    }

    // ȸ�� ���� �� ���� ����
    private void EvadeSuccessColor()
    {
        StartCoroutine(Change_Color(changeColorTime,evadeColor));
    }

    // Ư�� ��ü ���� Ȯ��
    public override bool IsSpecialObject()
    {
        return true;
    }

    // ���� ���·� ����
    public void ResetToNormal()
    {
        state.SetIdleMode();
        animator.SetBool("IsDodgedAction", false);
    }
}
