using System.Collections.Generic;
using UnityEngine;

public class CollisionAttackHandler : MonoBehaviour
{
    private GameObject attacker;     // 공격자
    private Weapon weapon;          // 공격한 무기

    private List<GameObject> hitList = new List<GameObject>();
    [SerializeField]
    private DoActionData actionData;
    private Collider t;

    private void Awake()
    {
        t = GetComponent<Collider>();
    }

    public void InitData(GameObject obj, Weapon weapon, float delay)
    {
        attacker = obj;
        this.weapon = weapon;
        Destroy(gameObject, delay);
    }

    public void SetDoActionData(DoActionData data)
    {
        actionData = data;
    }

    private void OnTriggerStay(Collider other)
    {
        if (attacker == null)
            return;



        IDamagable damage = other.gameObject.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;
        if (attacker == target)
            return;


        if (hitList.Contains(target) == true)
            return;

        hitList.Add(target);

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(attacker, weapon, hitPointNew, actionData);
    }

}