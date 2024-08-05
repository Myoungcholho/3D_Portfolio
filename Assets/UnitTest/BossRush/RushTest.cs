using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushTest : MonoBehaviour
{
    public Animator animator;
    public Transform playerTransform;
    public float jumpHeight = 5f;
    public float jumpDuration = 3f;
    public float distanceInFrontOfPlayer = 2f; // �÷��̾� �տ� ������ �Ÿ�

    private Rigidbody rb;
    private Vector3 jumpVelocity;
    private bool isJumping = false;
    private float jumpStartTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.useGravity = true;
        //rb.constraints = RigidbodyConstraints.None;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartJump();
        }
    }

    void FixedUpdate()
    {
        if (isJumping)
        {
            float elapsed = Time.time - jumpStartTime;
            if (elapsed > jumpDuration)
            {
                EndJump();
            }
        }
    }

    void StartJump()
    {
        // 1. ���� �ִϸ��̼� ����
        animator.SetTrigger("IsJump");

        // 2. �÷��̾� �տ� ������ ��ǥ ���� ���
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector3 jumpTarget = playerTransform.position - directionToPlayer * distanceInFrontOfPlayer;

        // 3. �ʱ� �ӵ� ���
        jumpVelocity = CalculateJumpVelocity(transform.position, jumpTarget, jumpHeight);

        // 4. Rigidbody�� ����� �̵�
        rb.isKinematic = false;
        rb.velocity = jumpVelocity;

        isJumping = true;
        jumpStartTime = Time.time;
    }

    void EndJump()
    {
        // ���� �� �̵� �� ����
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        animator.SetTrigger("EndJump");
        isJumping = false;
    }

    Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float height)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0, endPoint.z - startPoint.z);

        float time = Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / time;

        return velocityXZ + velocityY * -Mathf.Sign(gravity);
    }
}