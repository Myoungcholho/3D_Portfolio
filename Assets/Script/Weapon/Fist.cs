using UnityEngine;
using static DualSword;

public class Fist : Melee
{
    public enum FistType
    {
        LeftHand,RightHand,LeftFoot,RightFoot,Max
    }

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Fist;
    }

    protected override void Awake()
    {
        base.Awake();

        for(int i=0; i<(int)FistType.Max; ++i)
        {
            Transform t = colliders[i].transform;

            t.DetachChildren();

            Fist_Trigger trigger = t.GetComponent<Fist_Trigger>();
            trigger.OnTrigger += OnTriggerEnter;

            string partName = ((FistType)i).ToString();
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);

            t.SetParent(parent, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        //base.Begin_Collision(e);

        colliders[e.intParameter].enabled = true;
    }

    public override void End_Collision()
    {
        base.End_Collision();

    }
}