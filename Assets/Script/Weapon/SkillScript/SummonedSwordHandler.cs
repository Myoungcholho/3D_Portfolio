using System;
using System.Collections;
using UnityEngine;

public class SummonedSwordHandler : MonoBehaviour
{
    private float enableDelay; // 콜라이더가 켜지기까지의 지연 시간
    private float colliderDuration; // 콜라이더 지속 시간

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

    // 이 부분을 팩토리 패턴으로 변경할 수 있지 않을까?
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
