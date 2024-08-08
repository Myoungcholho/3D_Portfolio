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

    // ������ ������ƮƮ���� �ٸ� �ӵ��� �ִϸ��̼� �ӵ��� ����
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
        // ���� speed 0.1�� ���� (��: �÷��̾ ���� �ӵ� ����)
        SetSpeed(0.1f);

        // 1�� ���
        yield return new WaitForSeconds(3.0f);

        // attackCount�� ���� 0.5�ʾ� �߰� ���
        for (int i = 0; i < attackCount; ++i)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // ���� speed 1�� ���� (��: �÷��̾ ���� �ӵ� ����)
        SetSpeed(1.0f);

        // ���� Ƚ�� �ʱ�ȭ �� �÷��̾� ���� idle�� ���� �۾�
        // ������ force ó��
        attackCount = 0;
        foreach(IStoppable stoppable in stoppers)
        {
            Player player = stoppable as Player;
            if (player != null)
            {
                // �÷��̾ ���� ���·� ����
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
