using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTongTong : MonoBehaviour
{
    private float timer;
    private new Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 5f)
        {
            rigidbody.AddForce(Vector3.up * 15f,ForceMode.Impulse);
            timer = 0;
        }
    }
}
