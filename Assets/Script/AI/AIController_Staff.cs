using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController_Staff : AIController
{
    [SerializeField]
    private float avoidRange = 5.0f;
    [SerializeField]
    private Vector2 backDistance = new Vector2(3, 10);

    [SerializeField]
    private GameObject telpoParticlePrefab;

    private Transform lookAtTarget;         // 바라볼 상대
    private PatrolComponent patrol;             //순찰 관련 기능 컴포넌트

    protected override void Awake()
    {
        base.Awake();
        patrol = GetComponent<PatrolComponent>();
    }

    protected override void Update()
    {
        base.Update();

        // 바라볼 대상이 있다면 바라보기
        if(lookAtTarget != null)
        {
            Vector3 directionToLookAtTarget = lookAtTarget.position - transform.position;
            directionToLookAtTarget.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToLookAtTarget);
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
            transform.rotation = lookRotation;
        }

        // 경로에 도착했는지 판단
        // 경로를 계산 중인지 && 목적지까지 남은 거리 <= 도착했다고 간주하는 거리
        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            // 현재 경로를 가지고 있는지 || 속도가 0인지
            if (!nav.hasPath || nav.velocity.sqrMagnitude == 0f)
            {
                bRetreat = false;
            }
        }

    }

    protected override void FixedUpdate()
    {
        // Check 상태에 걸린다면 return
        if (CheckMode())
            return;

        GameObject player = perception.GetPercievedPlayer();
        if(player ==null)
        {
            if (weapon.UnarmedMode == false)
                weapon.SetUnarmedMode();

            lookAtTarget = null;
            SetPatrolMode();
            return;
        }
        lookAtTarget = player.transform;              // 바라볼 상대 저장

        if (weapon.FireBallMode == false)
        {
            SetEquipMode(WeaponType.FireBall);
            return;
        }

        if (weapon.FireBallMode == true && !CheckCoolTime())
            SetActionMode();

        // 거리가 가깝다면 뒤로 or 텔포
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp <= avoidRange)
        {
            SetFallBackMode();
            if (SetNavMeshPath(player.transform))
                return;
            
            DoActionWarp(player.transform);
        }
    }

    public float backIsDistance = 5.0f;
    private bool SetNavMeshPath(Transform targetTransform)
    {
        NavMeshPath path = new NavMeshPath();

        Vector3 direction = targetTransform.position - transform.position;
        Vector3 backWord = -(direction.normalized * backIsDistance);
        Vector3 position = transform.position + backWord;

        if (nav.CalculatePath(position, path))
        {
            nav.SetPath(path);
            nav.isStopped = false;                                      // nav 풀기
            bRetreat = true;                                            // 후퇴 중인지
            Debug.DrawLine(transform.position, position,Color.yellow);
            return true;
        }

        return false;
    }

    protected override void SetActionMode()
    {
        if (ActionMode == true)
            return;

        ChangeType(Type.Action);
        weapon.DoAction();
    }

    private void DoActionWarp(Transform targetTransform)
    {
        Instantiate<GameObject>(telpoParticlePrefab, transform.position,Quaternion.identity);
        Vector3 rearPosition = targetTransform.position - targetTransform.forward * backIsDistance;
        transform.position = rearPosition;
        Instantiate<GameObject>(telpoParticlePrefab, transform.position, Quaternion.identity);
    }

    private void SetPatrolMode()
    {
        if (PatrolMode == true)
            return;

        nav.updateRotation = true;      // 자동 회전 활성
        ChangeType(Type.Patrol);
        nav.isStopped = false;
        patrol.StartMove();
    }


    protected bool CheckMode()
    {
        bool bCheck = false;
        bCheck |= (EquipMode == true);
        bCheck |= (DamagedMode == true);

        return bCheck;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, avoidRange);
    }

#endif

}
