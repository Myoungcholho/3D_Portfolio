using UnityEngine;

public class IKComponent : MonoBehaviour
{
    [SerializeField, Range(0, 1)]
    private float distance = 0;

    [SerializeField, Range(0, 1)]
    private float weight = 1.0f;

    [SerializeField]
    private LayerMask layerMask;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        SetFootIK(AvatarIKGoal.LeftFoot);
        SetFootIK(AvatarIKGoal.RightFoot);
    }

    private void SetFootIK(AvatarIKGoal goal)
    {
        animator.SetIKPositionWeight(goal, weight);
        animator.SetIKRotationWeight(goal, weight);


        Ray ray = new Ray(animator.GetIKPosition(goal) + Vector3.up, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * (distance + 1), Color.red);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance + 1, layerMask))
        {
            Vector3 foot = hit.point;
            foot.y += distance;
            animator.SetIKPosition(goal, foot);
            animator.SetIKRotation(goal, Quaternion.LookRotation(transform.forward, hit.normal));
        }
    }
}
