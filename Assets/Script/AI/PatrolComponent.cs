using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;


public class PatrolComponent : MonoBehaviour
{
    [SerializeField]
    private float radius = 10;              // ���� �ݰ�

    [SerializeField]
    private float goalDelay = 1.0f;         // ���� �� ���ð�

    [SerializeField]
    private float goalDelayRandom = 0.25f;  // 0.75~1.25 RandomDelay

    [SerializeField]
    private PatrolPoints patrolPoints;      // ���� Ž�� ��ũ��Ʈ
    public bool HasPatrolPoints { get => patrolPoints != null; }

    
    private Vector3 initPosition;           // �ʱ� ��ġ
    private Vector3 goalPosition;           // ��ǥ ��ġ

    private NavMeshAgent navMeshAgent;
    private NavMeshPath navMeshPath;
    private bool bArrived;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        initPosition = transform.position;
        goalPosition = transform.position;
    }

    /// <summary>
    /// ��ǥ �������� �����ߴ��� �˻��մϴ�.
    /// </summary>
    private void Update()
    {
        if (navMeshPath == null)
            return;

        if (bArrived == true)
            return;

        float distance = Vector3.Distance(transform.position, goalPosition);
        if (distance >= navMeshAgent.stoppingDistance)
            return;

        bArrived = true;

        float waitTime = goalDelay + Random.Range(-goalDelayRandom, +goalDelayRandom);
        StartCoroutine(WaitDelay(waitTime));
    }

    /// <summary>
    /// ��� ����ϰ� ��θ� �ٽ� �����մϴ�.
    /// </summary>
    private IEnumerator WaitDelay(float time)
    {
        yield return new WaitForSeconds(time);

        navMeshPath = CreateNavMeshPath();
        navMeshAgent.SetPath(navMeshPath);
        bArrived = false;
    }

    public void StartMove()
    {
        if (navMeshPath == null)
            navMeshPath = CreateNavMeshPath();      // �ʱ� �������κ��� ������ ��ǥ������ ��θ� ������ ��ü ����
            

        navMeshAgent.SetPath(navMeshPath);      // ��δ�� �̵�
    }

    /// <summary>
    /// ��ǥ ������ ��� NavMeshPath�� �����ϰ� ��ȯ�մϴ�.
    /// </summary>
    private NavMeshPath CreateNavMeshPath()
    {
        NavMeshPath path = null;

        if(HasPatrolPoints)
        {
            goalPosition = patrolPoints.GetMoveToPosition();
            path = new NavMeshPath();
            bool bCheck = navMeshAgent.CalculatePath(goalPosition, path);
            Debug.Assert(bCheck);

            patrolPoints.UpdateNextIndex();
            return path;
        }

        Vector3 prevGoalPostion = goalPosition;

        // Coroutine or Thread �� ����
        while (true)
        {
            while (true)
            {
                float x = Random.Range(-radius * 0.5f, +radius * 0.5f);
                float z = Random.Range(-radius * 0.5f, +radius * 0.5f);

                goalPosition = new Vector3(x, 0, z) + initPosition;

                // ���� ���� �ƴ϶�� �ٽ� ������ �ʰ� 25%
                if (Vector3.Distance(goalPosition, prevGoalPostion) > radius * 0.25f)
                    break;
            }

            path = new NavMeshPath();

            // path ��ü�� ���� ����� ���� ������ �����ϰ�
            // �� �����ߴٸ� true �ƴ϶�� false�� ��ȯ��.
            if (navMeshAgent.CalculatePath(goalPosition, path))
                return path;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Vector3 from = transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        Vector3 to = goalPosition + new Vector3(0.0f, 0.1f, 0.0f);

        // ��ǥ ���������� �� �׸���
        Gizmos.color = Color.red;
        Gizmos.DrawLine(from, to);

        // ��ǥ���������� �� �׸���
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(goalPosition, 0.5f);

        // �ʱ� ��ġ�� ���� �׸���
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(initPosition, 0.25f);
    }
#endif
}
