using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

// Melee Ŭ����: Weapon Ŭ���� ���. ���� ���� ���� ���� ����
public class Melee : Weapon
{
    private bool bEnable;       // �޺� Ȱ��ȭ ����
    private bool bExist;        // �޺� �� ����
    protected int index;        // �޺� �ε���

    // ��ų ���� �� �޺� ������ ����
    public int Index
    {
        set { index = value; }
    }
    protected Collider[] colliders;             // ���⿡ ����� Collider �迭
    protected List<string> hittedList;            // ��Ʈ�� ��ü�� �ؽ� ����Ʈ (�ߺ� ��Ʈ ����)
    protected GameObject attacker;                // ������ ������Ʈ

    // ������ ����
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

    // �ݶ��̴� Ȱ��ȭ (���� ����)
    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true;
    }

    // �ݶ��̴� ��Ȱ��ȭ (���� ����)
    public virtual void End_Collision()
    {
        foreach (Collider collider in colliders)
            collider.enabled = false;

        hittedList.Clear();
    }

    // �޺� ����
    public void Begin_Combo()
    {
        bEnable = true;
    }

    // �޺� ����
    public void End_Combo()
    {
        bEnable = false;
    }

    // ���� �׼� ó��
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

    // ���� ���� ó��
    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        // �޺� ���°� �ƴϰų� ������ �����̸� �ߴ�
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

    // ���� ���� ó��
    public override void End_DoAction()
    {
        base.End_DoAction();

        index = 0;                          // �޺� �ε��� �ʱ�ȭ
        bEnable = false;                    // �޺� ����
    }

    // ��Ʈ �� �ؽ� �ڵ� ���� (�ߺ� ������)
    public string CreateHashCode(Collider other, GameObject obj)
    {
        if (other == null)
            return string.Empty;

        if (obj == null)
            obj = gameObject;

        return $"{other.name}_{obj.name}";
    }

    // ī�޶� ���޽� ȿ�� ����
    public virtual void Play_Impulse()
    {
        if (impulse == null)
            return;

        if (doActionDatas[index].ImpulseSettings == null)
            return;

        if (doActionDatas[index].ImpulseDirection.magnitude <= 0.0f)
            return;

        //brain.m_CameraCutEvent ī�޶� Ȱ��ȭ �ɶ����� ��
        CinemachineVirtualCamera camera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        if (camera != null)
        {
            CinemachineImpulseListener listener = camera.GetComponent<CinemachineImpulseListener>();
            listener.m_ReactionSettings.m_SecondaryNoise = doActionDatas[index].ImpulseSettings;

        }

        impulse.GenerateImpulse(doActionDatas[index].ImpulseDirection);
    }

    // ������ ó�� (Trigger �浹)
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        if (attacker == null)
            attacker = gameObject;
        string hashCode = CreateHashCode(other, attacker);

        if (hittedList.Contains(hashCode) == true)
            return;

        hittedList.Add(hashCode);                            // ��Ʈ�� ��ü ����


        // ȸ�� ī���� ���� ó��
        if (state != null && (state.DodgedMode || state.DodgedAttackMode))
        {
            ApplyDodgeCounterDamage(other);
            return;
        }

        ApplyDamage(other);
    }


        // �Ϲ�/�ݰ� ���� ������ ó�� ���� �޼���
        private void ApplyDamage<T>(Collider other, Action<GameObject, Weapon, Vector3, DoActionData> damageAction) where T : class
        {
            T damageHandler = other.gameObject.GetComponent<T>();

            if (damageHandler == null)
                return;

            Vector3 hitPoint = Vector3.zero;

            // �����ڿ� ��ġ�ϴ� �ݶ��̴��� �浹 ���� ���
            Collider enabledCollider = null;
            foreach (Collider collider in colliders)
            {
                if (collider.name.Equals(attacker.name) == true)
                {
                    enabledCollider = collider;
                    break;
                }
            }

            // �� ���� ��ġ�� ���� ��ġ�� ���� ����� ���� ��ȯ
            // 
            hitPoint = enabledCollider.ClosestPoint(other.transform.position);
            // 
            hitPoint = other.transform.InverseTransformPoint(hitPoint);

            // ȣ���� �޼��带 �Ķ���ͷ� �޾� ó��
            damageAction(rootObject, this, hitPoint, doActionDatas[index]);
        }

        // ������ �Ϲ� ���� ������ ó��
        private void ApplyDamage(Collider other)
        {
            ApplyDamage<IDamagable>(other, (rootObj, weapon, hitPoint, data) =>
            {
                var damage = other.gameObject.GetComponent<IDamagable>();
                damage?.OnDamage(rootObj, weapon, hitPoint, data);
            });
        }

        // ������ �ݰ� ���� ������ ó��
        private void ApplyDodgeCounterDamage(Collider other)
        {
            ApplyDamage<IDodgeDamageHandler>(other, (rootObj, weapon, hitPoint, data) =>
            {
                var damage = other.gameObject.GetComponent<IDodgeDamageHandler>();
                damage?.OnDodgeDamage(rootObj, weapon, hitPoint, data);
            });
        }
}