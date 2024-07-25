using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHammer : MonoBehaviour
{
    [SerializeField]
    private float destoryTime =3f;

    public event Action< Collider, Vector3> OnProjectileHit;
    private List<GameObject> hittedList = new List<GameObject>();
    private GameObject rootGameObject;

    private void Awake()
    {

    }

    private void Start()
    {
        rootGameObject = transform.root.gameObject;
        Destroy(rootGameObject, destoryTime);
    }

    public void OnParticleCollision(GameObject other)
    {
        if (hittedList.Contains(other.gameObject) == true)
            return;

        hittedList.Add(other.gameObject);

        Collider otherCollider = other.GetComponent<Collider>();

        OnProjectileHit?.Invoke(otherCollider, transform.position);
    }
}
