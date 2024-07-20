using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(HealthPointComponent))]
[RequireComponent(typeof(WeaponComponent))]
public abstract class Character : MonoBehaviour, IStoppable
{
    protected Animator animator;
    protected new Rigidbody rigidbody;
    protected StateComponent state;
    protected HealthPointComponent healthPoint;
    protected WeaponComponent weapon;

#if UNITY_EDITOR
    protected virtual void Reset()
    {
        
    }
#endif

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        state = GetComponent<StateComponent>();
        healthPoint = GetComponent<HealthPointComponent>();
        weapon = GetComponent<WeaponComponent>();
    }

    protected virtual void Start()
    {
        Regist_MovableStopper();
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    public void Regist_MovableStopper()
    {
        MovableStopper.Instance.Regist(this);
    }


    public void Remove_MovableStopper()
    {
        MovableStopper.Instance.Remove(this);
    }

    public IEnumerator Start_FrameDelay(int frame)
    {
        animator.speed = 0.0f;

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        animator.speed = 1.0f;
    }

    protected virtual void End_Damaged()
    {

    }

    protected virtual void OnDestroy()
    {
        Remove_MovableStopper();
    }

}