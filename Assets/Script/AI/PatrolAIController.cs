using UnityEngine;
using UnityEngine.AI;

// PatrolAIController Ŭ����: AIController�� ��ӹ޾� ����, ����, ���� �̵� ���� �����ϴ� AI Ŭ����
public class PatrolAIController : AIController
{
    private PatrolComponent patrol; // ���� ���� ����� ó���ϴ� ������Ʈ

    // �ʱ�ȭ �� ���� ������Ʈ �ʱ�ȭ
    protected override void Awake()
    {
        base.Awake();
        patrol = GetComponent<PatrolComponent>();
    }

    // ���� ������Ʈ: AI�� ���¿� ���� �ൿ ����
    protected override void FixedUpdate()
    {
        if (state.DeadMode)
        {
            Dead();  // ��� ���� ó��
            return;
        }

        // ��Ÿ���� ���� ���̸� �ൿ���� ����
        if (CheckCoolTime())
            return;

        // Ư�� ���(EquipMode, ActionMode, DamagedMode)�� ��� �ٸ� �ൿ�� ���� ����
        if (CheckMode())
            return;

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
        {
            if (!weapon.UnarmedMode)
                weapon.SetUnarmedMode();

            // ���� ����� ���ٸ� ��� ���� ��ȯ
            if (patrol == null)
            {
                SetWaitMode();
                return;
            }

            SetPatrolMode();  // ���� ���� ��ȯ
            return;
        }

        // ���Ⱑ ���� �� ���� ����
        if (weapon.UnarmedMode)
        {
            SetEquipMode(WeaponType.Sword);
            return;
        }

        // ���� ����� ��� ���� ó��
        if (FallBackMode)
        {
            EnemyFallBack(player);
            return;
        }

        // ���� ���� ����� ��� ���� �̵� ó��
        if (CirclePatrolMode)
        {
            EnemyCirclePatrol(player);
            return;
        }

        // �÷��̾���� �Ÿ��� ���� ���� �̳��� ��� ���� ����
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp < attackRange && weapon.SwordMode)
        {
            SetActionMode();  // ���� ��� ��ȯ
            return;
        }

        SetApproachMode();  // �÷��̾� ���� ��� ��ȯ
    }

    // ���� ó��
    private void EnemyFallBack(GameObject player)
    {
        fallbackTime += Time.fixedDeltaTime;

        // �÷��̾�κ��� �ݴ� �������� ����
        Vector3 directionToPlayer = (transform.position - player.transform.position).normalized;
        fallbackDirection = directionToPlayer;

        if (fallbackTime < 1.5f)
        {
            // 1.5�� ���� ����
            nav.Move(fallbackDirection * nav.speed * Time.fixedDeltaTime);

            // �÷��̾ �ٶ󺸵��� ȸ��
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
            // ���� �̵����� ��ȯ
            SetCirclePatrolMode();
        }
    }

    // ���� �̵� ó��
    private void EnemyCirclePatrol(GameObject player)
    {
        circlePatrolTime += Time.fixedDeltaTime;
        if (circlePatrolTime < 4.0f)
        {
            // ����/���������� �̵�
            Vector3 direction1 = Quaternion.AngleAxis(85f, Vector3.up) * transform.forward * 2f;  // ����
            Vector3 direction2 = Quaternion.AngleAxis(-85f, Vector3.up) * transform.forward * 2f; // ������

            NavMeshHit hit;
            Vector3 nextPosition = transform.position;

            // ���⿡ ���� �̵�
            if (randomDirection == 1)
                nextPosition += direction1;
            else if (randomDirection == 2)
                nextPosition += direction2;

            // ��ȿ�� NavMesh ���� ��ġ�� �̵�
            if (NavMesh.SamplePosition(nextPosition, out hit, 2.0f, NavMesh.AllAreas))
                nextPosition = hit.position;

            nav.SetDestination(nextPosition);  // �̵� ����

            // �÷��̾ �ٶ󺸵��� ȸ��
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
            SetWaitMode();  // ��� ���� ��ȯ
        }
    }

    // ��� �� ó��
    private void Dead()
    {
        nav.isStopped = true;  // �׺���̼� ����
    }

    // ������ ������ ���� ���� ��ȯ
    protected override void OnEndDoAction()
    {
        base.OnEndDoAction();
        SetFallBackMode();
    }

    // ���� ���� ��ȯ
    private void SetPatrolMode()
    {
        if (PatrolMode)
            return;

        ChangeType(Type.Patrol);
        nav.updateRotation = true;  // �̵� ������ �ٶ󺸵��� ����
        nav.isStopped = false;
        patrol.StartMove();  // ���� ����
    }

    // Ư�� ������� üũ (����, ����, �ǰ� ������)
    protected bool CheckMode()
    {
        bool bCheck = false;
        bCheck |= EquipMode;
        bCheck |= ActionMode;
        bCheck |= DamagedMode;

        return bCheck;
    }
}