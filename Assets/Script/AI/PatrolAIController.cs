using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolAIController : AIController
{


    private PatrolComponent patrol;             //���� ���� ��� ������Ʈ

    protected override void Awake()
    {
        base.Awake();
        patrol = GetComponent<PatrolComponent>();
    }

    protected override void FixedUpdate()
    {
        // ��Ÿ���̶�� return
        if (CheckCoolTime())
            return;

        //(EquipMode, ActionMode, DamageMode) ��� return
        if (CheckMode())
            return;

        GameObject player = perception.GetPercievedPlayer();
        if(player ==null)
        {
            if (weapon.UnarmedMode == false)
                weapon.SetUnarmedMode();

            // ���� ����� ���ٸ� ���
            if(patrol == null)
            {
                SetWaitMode();

                return;
            }

            SetPatrolMode();
            return;
        }

        if (weapon.UnarmedMode == true)
        {
            SetEquipMode(WeaponType.Sword);
            return;
        }

        float temp = Vector3.Distance(transform.position, player.transform.position);
        if(temp < attackRange)
        {
            // Į�� �����ϰ� �־��ٸ�
            if (weapon.SwordMode == true)
                SetActionMode();

            return;
        }
        SetApproachMode();
    }


    // ���� ���� ����
    private void SetPatrolMode()
    {
        if (PatrolMode == true)
            return;

        ChangeType(Type.Patrol);
        nav.isStopped = false;
        patrol.StartMove();
    }

    protected bool CheckMode()
    {
        bool bCheck = false;
        bCheck |= (EquipMode == true);
        bCheck |= (ActionMode == true);
        bCheck |= (DamagedMode == true);

        return bCheck;
    }

}
