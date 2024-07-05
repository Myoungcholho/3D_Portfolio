using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class TeleportAndRestoreState : StateMachineBehaviour
{
    //[SerializeField]
    //private float delayTime = 2.0f;

    //private float curTime;
    private PlayerMovingComponent playerMove;

    // 1»∏ »£√‚
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        //curTime = Time.realtimeSinceStartup;
        
        playerMove = animator.gameObject.GetComponent<PlayerMovingComponent>();
        Debug.Assert(playerMove != null);
        playerMove.Stop();
    }

    /*override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (Time.realtimeSinceStartup > curTime + delayTime)
            animator.SetTrigger("TestExit");

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        animator.ResetTrigger("TestExit");
    }*/
}
