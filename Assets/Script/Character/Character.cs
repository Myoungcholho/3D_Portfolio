using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// Character Ŭ����: ĳ������ �⺻ ������ ó���ϴ� �߻� Ŭ����, �پ��� ������Ʈ�� �������̽��� ����
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(HealthPointComponent))]
public abstract class Character : MonoBehaviour, IStoppable
{
    // ĳ������ �ֿ� ������Ʈ��
    protected Animator animator;
    protected new Rigidbody rigidbody;
    protected StateComponent state;
    protected HealthPointComponent healthPoint;
    protected WeaponComponent weapon;

#if UNITY_EDITOR
    // �����Ϳ��� Reset �� ȣ��
    protected virtual void Reset()
    {

    }
#endif

    // Awake: ������Ʈ �ʱ�ȭ
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        state = GetComponent<StateComponent>();
        healthPoint = GetComponent<HealthPointComponent>();
        weapon = GetComponent<WeaponComponent>();
    }

    // Start: MovableStopper�� ĳ���� ���
    protected virtual void Start()
    {
        Regist_MovableStopper();
    }

    protected virtual void Update()
    {
        // ��ӹ޴� Ŭ�������� ���� ����
    }

    protected virtual void FixedUpdate()
    {
        // ���� ������Ʈ (��ӹ޴� Ŭ�������� ����)
    }

    // MovableStopper�� ĳ���� ���
    public void Regist_MovableStopper()
    {
        MovableStopper.Instance.Regist(this);
    }

    // MovableStopper���� ĳ���� ����
    public void Remove_MovableStopper()
    {
        MovableStopper.Instance.Remove(this);
    }

    // �ִϸ��̼� �ӵ��� ���߰� �־��� ������ �Ŀ� �ٽ� ����
    public IEnumerator Start_FrameDelay(int frame)
    {
        animator.speed = 0.0f;

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        animator.speed = 1.0f;
    }

    // �ӵ��� ������ �� �־��� ������ �Ŀ� �⺻ �ӵ��� ����
    public IEnumerator Start_FrameDelay(int frame, float speed)
    {
        animator.speed = speed;

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        animator.speed = 1.0f;
    }

    // �ִϸ��̼� �ӵ��� ����
    public void SetAnimationSpeed(float speed)
    {
        animator.speed = speed;
    }

    // Ư���� ������Ʈ���� Ȯ�� (��ӹ޴� Ŭ�������� �������̵� ����)
    // �÷��̾��� TRUE ���̶�� FALSE�� ��ȯ�� �����̸� �ٸ��� ó��
    public virtual bool IsSpecialObject()
    {
        return false;
    }

    // ������ ó�� �� Idle ���� ��ȯ
    protected virtual void End_Damaged()
    {
        // ĳ���Ͱ� ��� ���¶�� Idle ���� ��ȯ���� ����
        if (state.DeadMode == true)
            return;

        state.SetIdleMode();
    }

    // �ı� �� MovableStopper���� ����
    protected virtual void OnDestroy()
    {
        Remove_MovableStopper();
    }
}