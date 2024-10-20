using UnityEngine;
using UnityEngine.AI;

// PatrolAIController 클래스: AIController를 상속받아 순찰, 후퇴, 원형 이동 등을 제어하는 AI 클래스
public class PatrolAIController : AIController
{
    private PatrolComponent patrol; // 순찰 관련 기능을 처리하는 컴포넌트

    // 초기화 시 순찰 컴포넌트 초기화
    protected override void Awake()
    {
        base.Awake();
        patrol = GetComponent<PatrolComponent>();
    }

    // 물리 업데이트: AI의 상태에 따라 행동 결정
    protected override void FixedUpdate()
    {
        if (state.DeadMode)
        {
            Dead();  // 사망 상태 처리
            return;
        }

        // 쿨타임이 진행 중이면 행동하지 않음
        if (CheckCoolTime())
            return;

        // 특정 모드(EquipMode, ActionMode, DamagedMode)일 경우 다른 행동을 하지 않음
        if (CheckMode())
            return;

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
        {
            if (!weapon.UnarmedMode)
                weapon.SetUnarmedMode();

            // 순찰 기능이 없다면 대기 모드로 전환
            if (patrol == null)
            {
                SetWaitMode();
                return;
            }

            SetPatrolMode();  // 순찰 모드로 전환
            return;
        }

        // 무기가 없을 때 무기 장착
        if (weapon.UnarmedMode)
        {
            SetEquipMode(WeaponType.Sword);
            return;
        }

        // 후퇴 모드일 경우 후퇴 처리
        if (FallBackMode)
        {
            EnemyFallBack(player);
            return;
        }

        // 원형 순찰 모드일 경우 원형 이동 처리
        if (CirclePatrolMode)
        {
            EnemyCirclePatrol(player);
            return;
        }

        // 플레이어와의 거리가 공격 범위 이내일 경우 공격 실행
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp < attackRange && weapon.SwordMode)
        {
            SetActionMode();  // 공격 모드 전환
            return;
        }

        SetApproachMode();  // 플레이어 추적 모드 전환
    }

    // 후퇴 처리
    private void EnemyFallBack(GameObject player)
    {
        fallbackTime += Time.fixedDeltaTime;

        // 플레이어로부터 반대 방향으로 후퇴
        Vector3 directionToPlayer = (transform.position - player.transform.position).normalized;
        fallbackDirection = directionToPlayer;

        if (fallbackTime < 1.5f)
        {
            // 1.5초 동안 후퇴
            nav.Move(fallbackDirection * nav.speed * Time.fixedDeltaTime);

            // 플레이어를 바라보도록 회전
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 360f);
            }
        }
        else
        {
            // 원형 이동으로 전환
            SetCirclePatrolMode();
        }
    }

    // 원형 이동 처리
    private void EnemyCirclePatrol(GameObject player)
    {
        circlePatrolTime += Time.fixedDeltaTime;
        if (circlePatrolTime < 4.0f)
        {
            // 왼쪽/오른쪽으로 이동
            Vector3 direction1 = Quaternion.AngleAxis(85f, Vector3.up) * transform.forward * 2f;  // 왼쪽
            Vector3 direction2 = Quaternion.AngleAxis(-85f, Vector3.up) * transform.forward * 2f; // 오른쪽

            NavMeshHit hit;
            Vector3 nextPosition = transform.position;

            // 방향에 따라 이동
            if (randomDirection == 1)
                nextPosition += direction1;
            else if (randomDirection == 2)
                nextPosition += direction2;

            // 유효한 NavMesh 상의 위치로 이동
            if (NavMesh.SamplePosition(nextPosition, out hit, 2.0f, NavMesh.AllAreas))
                nextPosition = hit.position;

            nav.SetDestination(nextPosition);  // 이동 설정

            // 플레이어를 바라보도록 회전
            Vector3 lookDirection = player.transform.position - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 360f);
            }
        }
        else
        {
            SetWaitMode();  // 대기 모드로 전환
        }
    }

    // 사망 시 처리
    private void Dead()
    {
        nav.isStopped = true;  // 네비게이션 정지
    }

    // 공격이 끝나면 후퇴 모드로 전환
    protected override void OnEndDoAction()
    {
        base.OnEndDoAction();
        SetFallBackMode();
    }

    // 순찰 모드로 전환
    private void SetPatrolMode()
    {
        if (PatrolMode)
            return;

        ChangeType(Type.Patrol);
        nav.updateRotation = true;  // 이동 방향을 바라보도록 설정
        nav.isStopped = false;
        patrol.StartMove();  // 순찰 시작
    }

    // 특정 모드인지 체크 (장착, 공격, 피격 중인지)
    protected bool CheckMode()
    {
        bool bCheck = false;
        bCheck |= EquipMode;
        bCheck |= ActionMode;
        bCheck |= DamagedMode;

        return bCheck;
    }
}