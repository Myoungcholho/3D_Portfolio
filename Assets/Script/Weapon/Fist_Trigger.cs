using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist_Trigger : MonoBehaviour
{
    public event Action<GameObject> OnAttacker;
    public Action<Collider> OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnAttacker?.Invoke(gameObject);
        OnTrigger?.Invoke(other);
    }
}