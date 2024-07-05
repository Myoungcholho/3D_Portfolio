using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorScan90 : MonoBehaviour
{
    public float radius = 5.0f;
    public LayerMask layerMask;
    public GameObject target;
    public bool bReTarget = false;
    public float rotateSpeed = 5.0f;
    public float moveSpeed = 3.0f;

    private RandomWalk randomWalk;
    private Animator animator;
    private Vector3 direction = Vector3.zero;
    private CharacterState characterState;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        randomWalk = GetComponent<RandomWalk>();
        characterState = GetComponent<CharacterState>();
    }

    void Update()
    {
        if (characterState.GetStateType() != TestStateType.Idle)
            return;


        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);

        if (target == null && bReTarget == true)
        {
            bReTarget = false;
            randomWalk.bCanMove = true;
            randomWalk.GetRandomPos();
        }

        if (colliders.Length == 0)
            target = null;
        foreach (Collider collider in colliders)
        {
            direction = collider.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, direction.normalized);
            if (angle < 45f)
            {
                target = collider.gameObject;
                break;
            }
            else
                target = null;
        }

        if (target != null)
        {
            bReTarget = true;
            randomWalk.bCanMove = false;

            animator.SetFloat("SpeedX", 1);
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.blue;
        
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 rightAngle45 = Quaternion.Euler(0,45,0) * forward;
        Vector3 leftAngle45 = Quaternion.Euler(0,-45, 0) * forward;

        Gizmos.DrawLine(pos, pos+ rightAngle45 * radius);
        Gizmos.DrawLine(pos, pos+ leftAngle45 * radius);
    }
}
