using System;
using UnityEngine;
using UnityEngine.Audio;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float force = 1000.0f;

    private new Rigidbody rigidbody;
    private new Collider collider;

    // <���� �浹ü, �ε��� �浹ü, ���� ��ġ>
    public event Action<Collider, Collider, Vector3> OnProjectileHit;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        Destroy(gameObject, 10f);
        rigidbody.AddForce(transform.forward * force);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnProjectileHit?.Invoke(collider, other, transform.position);

        AudioClip clip = SoundLibrary.Instance.projectileExplosion01;
        AudioMixerGroup group = SoundLibrary.Instance.mixerBasic;

        SoundManager.Instance.PlaySound(clip, group, true, transform.position);

        Destroy(gameObject);
    }
}
