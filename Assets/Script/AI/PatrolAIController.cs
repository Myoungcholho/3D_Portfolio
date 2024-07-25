using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolAIController : AIController
{


    private PatrolComponent patrol;             //순찰 관련 기능 컴포넌트

    protected override void Awake()
    {
        base.Awake();
        patrol = GetComponent<PatrolComponent>();
    }

    protected override void FixedUpdate()
    {
        // 쿨타임이라면 return
        if (CheckCoolTime())
            return;

        //(EquipMode, ActionMode, DamageMode) 라면 return
        if (CheckMode())
            return;

        GameObject player = perception.GetPercievedPlayer();
        if(player ==null)
        {
            if (weapon.UnarmedMode == false)
                weapon.SetUnarmedMode();

            // 순찰 기능이 없다면 대기
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
            // 칼을 장착하고 있었다면
            if (weapon.SwordMode == true)
                SetActionMode();

            return;
        }
        SetApproachMode();
    }


    // 순찰 모드로 변경
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
