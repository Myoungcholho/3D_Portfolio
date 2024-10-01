using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionController : MonoBehaviour
{
    private Animator animator;
    private StateComponent state;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
    }


    private void Update()
    {
        if (animator == null)
            return;

        // ���� ���� ���� RootMotion ����
        bool bCheck = false;
        bCheck |= state.ActionMode;
        bCheck |= state.UsingSkillMode;
        bCheck |= state.EvadeMode;

        if (bCheck)
            animator.applyRootMotion = true;
        else
            animator.applyRootMotion = false;

    }

}
