using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomEdgeHandler : MonoBehaviour
{
    [SerializeField]
    private float force = 1100.0f;

    private new Rigidbody rigidbody;
    private new Collider collider;

    private float enableDelay; // �ݶ��̴��� ����������� ���� �ð�
    private float colliderDuration; // �ݶ��̴� ���� �ð� , 0.6f

    public event Action<Collider, Collider, Vector3> OnEdgeHit;

    private void Awake()
    {
        rigidbody = GetComponentInChildren<Rigidbody>();
        collider = GetComponentInChildren<Collider>();
    }

    public void Initialize(float enableDelay,float colliderDuration)
    {
        this.enableDelay = enableDelay;
        this.colliderDuration = colliderDuration; 

        Destroy(gameObject, enableDelay+colliderDuration);
    }

    void Start()
    {
        rigidbody.AddForce(transform.forward * force);
    }

    private void OnTriggerStay(Collider other)
    {
        OnEdgeHit?.Invoke(collider, other, transform.position);
    }
}
