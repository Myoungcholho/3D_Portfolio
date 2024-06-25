using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAttack : MonoBehaviour
{
    private CharacterState characterState;
    private SectorScan90 sectorScan;
    private Animator animator;

    private void Awake()
    {
        characterState = GetComponent<CharacterState>();
        sectorScan = GetComponent<SectorScan90>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (sectorScan.target == null)
            return;

        // 거리가 좁아졌다면
        if(Vector3.Distance(transform.position, sectorScan.target.transform.position) < 1.0f)
        {
            if (characterState.GetType() == StateType.Attack)
                return;

            int attackType = Random.Range(0, 3);
            characterState.SetAttack();
            animator.SetInteger("AttackType", attackType);
            animator.SetTrigger("IsAttack");
        }
    }

    private void End_Attack()
    {
        characterState.SetIdle();
    }
}
