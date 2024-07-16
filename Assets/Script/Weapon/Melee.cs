using System.Collections.Generic;
using UnityEngine;

public class Melee : Weapon
{
    private bool bEnable;
    private bool bExist;
    protected int index;

    protected Collider[] colliders;
    private List<string> hittedList;
    private GameObject attacker;
    public void Attacker(GameObject attacker)
    {
        this.attacker = attacker;
    }

    protected override void Awake()
    {
        base.Awake();

        colliders = GetComponentsInChildren<Collider>();
        hittedList = new List<string>();
    }

    protected override void Start()
    {
        base.Start();

        End_Collision();
    }

    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true;


    }

    public virtual void End_Collision()
    {
        foreach (Collider collider in colliders)
            collider.enabled = false;



        hittedList.Clear();
    }

    public void Begin_Combo()
    {
        bEnable = true;
    }

    public void End_Combo()
    {
        bEnable = false;
    }

    public override void DoAction()
    {
        if (bEnable)
        {
            bEnable = false;
            bExist = true;

            return;
        }


        if (state.IdleMode == false)
            return;

        base.DoAction();
    }

    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        if (bExist == false)
            return;


        bExist = false;

        index++;
        animator.SetTrigger("NextCombo");


        if (doActionDatas[index].bCanMove)
        {
            Move();

            return;
        }

        CheckStop(index);
    }

    public override void End_DoAction()
    {
        base.End_DoAction();

        index = 0;
        bEnable = false;
    }

    public string CreateHashCode(Collider other,GameObject obj)
    {
        if (other == null)
            return string.Empty;

        if (obj == null)
            obj = gameObject;

        return $"{other.name}_{obj.name}";
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        if (attacker == null)
            attacker = gameObject;
        string hashCode = CreateHashCode(other, attacker);

        if (hittedList.Contains(hashCode) == true)
            return;

        hittedList.Add(hashCode);


        IDamagable damage = other.gameObject.GetComponent<IDamagable>();

        if (damage == null)
            return;


        Vector3 hitPoint = Vector3.zero;

        Collider enabledCollider = null;
        foreach (Collider collider in colliders)
        {
            if (collider.name.Equals(attacker.name) == true)
            {
                enabledCollider = collider;

                break;
            }
        }

        hitPoint = enabledCollider.ClosestPoint(other.transform.position);
        Debug.Log(hitPoint + " " + enabledCollider);
        hitPoint = other.transform.InverseTransformPoint(hitPoint);
        Debug.Log(hitPoint + " " + enabledCollider);
        damage.OnDamage(rootObject, this, hitPoint, doActionDatas[index]);
    }
}