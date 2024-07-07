using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class DualSword : Melee
{
    [SerializeField]
    private GameObject particlePrefab;

    private GameObject[] swords;
    private List<GameObject> particleList;    

    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.DualSword;

        
    }

    public enum PartType
    {
        DualLeftHand = 0, DualRightHand, Max
    }

    protected override void Awake()
    {
        base.Awake();
        swords = new GameObject[(int)PartType.Max];
        particleList = new List<GameObject>();

        for (int i = 0; i < (int)PartType.Max; i++)
        {
            Transform t = colliders[i].transform;
            swords[i] = t.gameObject;

            t.DetachChildren();
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;

            DualSwordTrigger trigger = t.GetComponent<DualSwordTrigger>();
            trigger.OnTrigger += OnTriggerEnter;
            trigger.OnAttacker += Attacker;

            string partName = ((PartType)i).ToString();
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);

            t.SetParent(parent, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }



        for (int i=0; i<(int)PartType.Max; i++)
        {
            swords[i].SetActive(false);
        }

        
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        for (int i = 0; i < (int)PartType.Max; i++)
        {
            swords[i].SetActive(true);
        }
    }

    public override void UnEquip()
    {
        base.UnEquip();

        for (int i = 0; i < (int)PartType.Max; i++)
        {
            swords[i].SetActive(false);
        }
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);

        if (particlePrefab == null)
            return;

        for (int i = 0; i < (int)PartType.Max; ++i)
        {
            GameObject obj = Instantiate<GameObject>(particlePrefab, swords[i].transform);
            obj.transform.localPosition = new Vector3(0, -0.63f, 0);
            particleList.Add(obj);
        }
    }

    public override void End_Collision()
    {
        base.End_Collision();

        foreach(GameObject obj in particleList)
            Destroy(obj);

        particleList.Clear();    
    }
}