using System;
using System.Collections;
using UnityEngine;

public class SummonedSwordHandler : MonoBehaviour
{
    private float enableDelay; // �ݶ��̴��� ����������� ���� �ð�
    private float colliderDuration; // �ݶ��̴� ���� �ð�

    public event Action<Collider, Collider, Vector3> OnSwordSkill01HitTrigger;
    public event Action OnColliderDisabled;

    private new Collider collider;
    

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        collider.enabled = false;
    }

    // �� �κ��� ���丮 �������� ������ �� ���� ������?
    public void Initialize(float enableDelay, float colliderDuration)
    {
        this.enableDelay = enableDelay;
        this.colliderDuration = colliderDuration;

        StartCoroutine(ActivateColliderAfterDelay());
    }

    private IEnumerator ActivateColliderAfterDelay()
    {
        yield return new WaitForSeconds(enableDelay);
        collider.enabled = true;
        yield return new WaitForSeconds(colliderDuration);
        collider.enabled = false;
        OnColliderDisabled?.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        OnSwordSkill01HitTrigger?.Invoke(collider, other, transform.position);
    }
}
