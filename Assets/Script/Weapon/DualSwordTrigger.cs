using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DualSwordTrigger : MonoBehaviour
{
    public DualSword.PartType partType;

    public event Action<GameObject> OnAttacker;
    public event Action<Collider, DualSword.PartType> OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("gameObject :" + gameObject.name);
        OnAttacker?.Invoke(gameObject);
        OnTrigger?.Invoke(other,partType);
    }
}