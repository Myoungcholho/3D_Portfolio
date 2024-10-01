using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostFieldSkill : MonoBehaviour
{
    private float enableDelay; // �ݶ��̴��� ����������� ���� �ð�
    private float colliderDuration; // �ݶ��̴� ���� �ð�

    public event Action<Collider, Collider, Vector3> OnFrostHit;
    private new Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public void Initialize(float enableDelay, float colliderDuration)
    {
        this.enableDelay = enableDelay;
        this.colliderDuration = colliderDuration;

        Destroy(gameObject, colliderDuration);
    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        OnFrostHit?.Invoke(collider, other, transform.position);
    }
}
