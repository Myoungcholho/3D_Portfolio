using System;
using System.Collections;
using UnityEngine;

public class TriggerInvoker : MonoBehaviour
{
    public event Action<Collider, Collider, Vector3> OnTriggerHit;
    private new Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        collider.enabled = false;
    }

    /// <summary>
    /// Duration 0주면 바로 삭제됨!!
    /// </summary>
    public void Initialize(float enableDelay, float colliderDuration)
    {
        StartCoroutine(ActivateColliderAfterDelay(enableDelay));

        float destroyTime = enableDelay + colliderDuration;
        Destroy(gameObject, destroyTime);
    }

    private IEnumerator ActivateColliderAfterDelay(float enableDelay)
    {
        yield return new WaitForSeconds(enableDelay);
        collider.enabled = true;
    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        OnTriggerHit?.Invoke(collider, other, transform.position);
    }
}
