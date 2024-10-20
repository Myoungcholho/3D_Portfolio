using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// Character 클래스: 캐릭터의 기본 동작을 처리하는 추상 클래스, 다양한 컴포넌트와 인터페이스를 포함
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(HealthPointComponent))]
public abstract class Character : MonoBehaviour, IStoppable
{
    // 캐릭터의 주요 컴포넌트들
    protected Animator animator;
    protected new Rigidbody rigidbody;
    protected StateComponent state;
    protected HealthPointComponent healthPoint;
    protected WeaponComponent weapon;

#if UNITY_EDITOR
    // 에디터에서 Reset 시 호출
    protected virtual void Reset()
    {

    }
#endif

    // Awake: 컴포넌트 초기화
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        state = GetComponent<StateComponent>();
        healthPoint = GetComponent<HealthPointComponent>();
        weapon = GetComponent<WeaponComponent>();
    }

    // Start: MovableStopper에 캐릭터 등록
    protected virtual void Start()
    {
        Regist_MovableStopper();
    }

    protected virtual void Update()
    {
        // 상속받는 클래스에서 동작 구현
    }

    protected virtual void FixedUpdate()
    {
        // 물리 업데이트 (상속받는 클래스에서 구현)
    }

    // MovableStopper에 캐릭터 등록
    public void Regist_MovableStopper()
    {
        MovableStopper.Instance.Regist(this);
    }

    // MovableStopper에서 캐릭터 제거
    public void Remove_MovableStopper()
    {
        MovableStopper.Instance.Remove(this);
    }

    // 애니메이션 속도를 멈추고 주어진 프레임 후에 다시 시작
    public IEnumerator Start_FrameDelay(int frame)
    {
        animator.speed = 0.0f;

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        animator.speed = 1.0f;
    }

    // 속도를 설정한 후 주어진 프레임 후에 기본 속도로 복원
    public IEnumerator Start_FrameDelay(int frame, float speed)
    {
        animator.speed = speed;

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        animator.speed = 1.0f;
    }

    // 애니메이션 속도를 설정
    public void SetAnimationSpeed(float speed)
    {
        animator.speed = speed;
    }

    // 특수한 오브젝트인지 확인 (상속받는 클래스에서 오버라이드 가능)
    // 플레이어라면 TRUE 적이라면 FALSE를 반환해 딜레이를 다르게 처리
    public virtual bool IsSpecialObject()
    {
        return false;
    }

    // 데미지 처리 후 Idle 모드로 전환
    protected virtual void End_Damaged()
    {
        // 캐릭터가 사망 상태라면 Idle 모드로 전환하지 않음
        if (state.DeadMode == true)
            return;

        state.SetIdleMode();
    }

    // 파괴 시 MovableStopper에서 제거
    protected virtual void OnDestroy()
    {
        Remove_MovableStopper();
    }
}