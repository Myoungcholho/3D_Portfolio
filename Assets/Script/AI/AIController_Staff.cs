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

    private Transform lookAtTarget;         // �ٶ� ���
    private PatrolComponent patrol;             //���� ���� ��� ������Ʈ

    protected override void Awake()
    {
        base.Awake();
        patrol = GetComponent<PatrolComponent>();
    }

    protected override void Update()
    {
        base.Update();

        // �ٶ� ����� �ִٸ� �ٶ󺸱�
        if(lookAtTarget != null)
        {
            Vector3 directionToLookAtTarget = lookAtTarget.position - transform.position;
            directionToLookAtTarget.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToLookAtTarget);
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5.0f);
            transform.rotation = lookRotation;
        }

        // ��ο� �����ߴ��� �Ǵ�
        // ��θ� ��� ������ && ���������� ���� �Ÿ� <= �����ߴٰ� �����ϴ� �Ÿ�
        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            // ���� ��θ� ������ �ִ��� || �ӵ��� 0����
            if (!nav.hasPath || nav.velocity.sqrMagnitude == 0f)
            {
                bRetreat = false;
            }
        }

    }

    protected override void FixedUpdate()
    {
        // Check ���¿� �ɸ��ٸ� return
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
        lookAtTarget = player.transform;              // �ٶ� ��� ����

        if (weapon.FireBallMode == false)
        {
            SetEquipMode(WeaponType.FireBall);
            return;
        }

        if (weapon.FireBallMode == true && !CheckCoolTime())
            SetActionMode();

        // �Ÿ��� �����ٸ� �ڷ� or ����
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
            nav.isStopped = false;                                      // nav Ǯ��
            bRetreat = true;                                            // ���� ������
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

        nav.updateRotation = true;      // �ڵ� ȸ�� Ȱ��
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
