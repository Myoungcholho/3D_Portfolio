using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist_Trigger : MonoBehaviour
{
    public Action<Collider> OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger?.Invoke(other);
    }
}
