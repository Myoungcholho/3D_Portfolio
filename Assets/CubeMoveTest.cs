using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class CubeMoveTest : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + new Vector3(1, 0, 0));
    }
}
