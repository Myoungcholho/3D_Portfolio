using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PatrolComponent : MonoBehaviour
{
    [SerializeField]
    private float radius = 10;              // 순찰 반경
    [SerializeField]
    private float goalDelay = 1.0f;         // 도달 시 대기시간
    [SerializeField]
    private float goalDelayRandom = 0.25f;  // 0.75~1.25 RandomDelay
    [SerializeField]
    private PatrolPoints patrolPoints;      // 구역 탐색 스크립트

    public bool HasPatrolPoints { get => patrolPoints != null; }

    private Vector3 initPosition;           // 초기 위치
    private Vector3 goalPosition;           // 목표 위치

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

    private void Update()
    {
        CheckAtTargetUpdate();
    }

    // 경로를 새로 찾고 목표로 이동
    public void StartMove()
    {
        if (navMeshPath == null)
            StartCoroutine(CreateNavMeshPathCoroutine(path => {
                navMeshPath = path;
                navMeshAgent.SetPath(navMeshPath);
            }));
        else
            navMeshAgent.SetPath(navMeshPath);
    }

    // 목표 지점까지 도달했는지 검사합니다.
    private void CheckAtTargetUpdate()
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

    // 잠시 대기하고 경로를 다시 설정합니다.
    private IEnumerator WaitDelay(float time)
    {
        yield return new WaitForSeconds(time);

        StartCoroutine(CreateNavMeshPathCoroutine(path => {
            navMeshPath = path;
            navMeshAgent.SetPath(navMeshPath);
            bArrived = false;
        }));
    }

    // 목표 지점의 경로 NavMeshPath를 생성하고 반환합니다.
    private IEnumerator CreateNavMeshPathCoroutine(System.Action<NavMeshPath> callback)
    {
        NavMeshPath path = null;

        if (HasPatrolPoints)
        {
            goalPosition = patrolPoints.GetMoveToPosition();
            path = new NavMeshPath();
            bool bCheck = navMeshAgent.CalculatePath(goalPosition, path);
            Debug.Assert(bCheck);

            patrolPoints.UpdateNextIndex();
            callback(path);
            yield break; // Exit the coroutine
        }

        Vector3 prevGoalPosition = goalPosition;

        while (true)
        {
            while (true)
            {
                float x = Random.Range(-radius * 0.5f, radius * 0.5f);
                float z = Random.Range(-radius * 0.5f, radius * 0.5f);

                goalPosition = new Vector3(x, 0, z) + initPosition;

                if (Vector3.Distance(goalPosition, prevGoalPosition) > radius * 0.25f)
                {
                    prevGoalPosition = goalPosition; // Update the previous goal position
                    break;
                }

                yield return null; // Wait until the next frame before continuing
            }

            path = new NavMeshPath();

            if (navMeshAgent.CalculatePath(goalPosition, path))
            {
                callback(path); // Return the path using the callback
                yield break; // Exit the coroutine
            }

            yield return null; // Wait until the next frame before continuing
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        /*if (Application.isPlaying == false)
            return;*/

        Vector3 from = transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        Vector3 to = goalPosition + new Vector3(0.0f, 0.1f, 0.0f);

        // 목표 지점으로의 선 그리기
        Gizmos.color = Color.red;
        Gizmos.DrawLine(from, to);

        // 목표지점으로의 구 그리기
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(goalPosition, 0.5f);

        // 초기 위치의 범위 그리기
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(initPosition, 0.25f);
    }
#endif
}