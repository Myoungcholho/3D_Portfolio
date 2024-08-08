using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableStopper : MonoBehaviour
{
    private static MovableStopper m_instance;
    public static MovableStopper Instance
    {
        get
        {
            return m_instance;
        }
    }

    private List<IStoppable> stoppers = new List<IStoppable>();
    private List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        if(m_instance == null)
        {
            m_instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Regist(IStoppable stopper)
    {
        stoppers.Add(stopper);
    }

    public void Remove(IStoppable stopper) 
    {
        stoppers.Remove(stopper);
    }

    // Set animation speed to 0 for `frame` frames, then restore to 1
    public void Start_Delay(int frame)
    {
        if (frame < 1)
            return;

        foreach(IStoppable stopper in stoppers)
        {
            StartCoroutine(stopper.Start_FrameDelay(frame));
        }
    }

    // 지정된 오브젝트트만을 다른 속도의 애니메이션 속도로 제어
    // frame : 
    public void Start_Delay_Specific(int frame, float specialSpeed, float normalSpeed)
    {
        if (frame < 1)
            return;

        foreach (IStoppable stopper in stoppers)
        {
            if (stopper.IsSpecialObject())
            {
                StartCoroutine(stopper.Start_FrameDelay(frame, specialSpeed));
            }
            else
            {
                StartCoroutine(stopper.Start_FrameDelay(frame, normalSpeed));
            }
        }
    }

    private int attackCount = 0;
    public int AttackCount 
    {  
        set => attackCount = value;
        get => attackCount;
    }

    public IEnumerator EvadeDelay()
    {
        // 전부 speed 0.1로 설정 (예: 플레이어나 적의 속도 조정)
        SetSpeed(0.1f);

        // 1초 대기
        yield return new WaitForSeconds(3.0f);

        // attackCount에 따라 0.5초씩 추가 대기
        for (int i = 0; i < attackCount; ++i)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 전부 speed 1로 설정 (예: 플레이어나 적의 속도 조정)
        SetSpeed(1.0f);

        // 공격 횟수 초기화 및 플레이어 상태 idle로 변경 작업
        // 누적된 force 처리
        attackCount = 0;
        foreach(IStoppable stoppable in stoppers)
        {
            Player player = stoppable as Player;
            if (player != null)
            {
                // 플레이어를 원래 상태로 복구
                player.ResetToNormal();
                WeaponComponent weaponComponent = player.GetComponent<WeaponComponent>();
                weaponComponent.End_DoAction();
            }
            Enemy enemy = stoppable as Enemy;
            if (enemy != null) 
            {
                enemy.CommitAndResetKnockback();
            }
        }
    }

    private void SetSpeed(float speed)
    {
        foreach(IStoppable stopper in stoppers)
        {
            stopper.SetAnimationSpeed(speed);
        }
    }
}
