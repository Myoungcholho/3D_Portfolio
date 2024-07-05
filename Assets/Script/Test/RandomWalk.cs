using UnityEngine;
public class RandomWalk : MonoBehaviour
{
    public float speed = 3f;
    public bool bCanMove = true;

    private Vector3 destPos;
    private float destTime;
    private float delayTime;
    private CharacterState characterState;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterState = GetComponent<CharacterState>();
    }

    private void Start()
	{
        GetRandomPos();
    }

    private void Update()
    {
        if (characterState.GetStateType() != TestStateType.Idle)
            return;

        if (bCanMove == false)
            return;

        if (Time.time >= destTime + delayTime)
        {
            animator.SetFloat("SpeedX", 1);
            transform.position = Vector3.MoveTowards(transform.position, destPos, speed * Time.deltaTime);
            Vector3 direction = destPos - transform.position;
            Quaternion quaternion = Quaternion.LookRotation(direction.normalized);
            transform.rotation = quaternion;

            if (Vector3.Distance(transform.position, destPos) < 0.5f)
            {
                destTime = Time.time;
                delayTime = Random.Range(1.0f, 2.0f);

                GetRandomPos();
            }
        }
        else
        {
            animator.SetFloat("SpeedX", 0);
        }
    }

    public void GetRandomPos()
    {
        destPos = new Vector3(Random.Range(-10, 10), transform.position.y, Random.Range(-10, 10));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, destPos);
    }
}
