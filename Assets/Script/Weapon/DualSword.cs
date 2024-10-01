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

    protected override void Update()
    {
        // Q 스킬 쿨타임 감소
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
        {
            QSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (QSkillDataCoolTime.RemainingCooldownTime < 0)
            {
                QSkillDataCoolTime.RemainingCooldownTime = 0;  // 쿨타임이 0보다 작아지지 않도록 제한
            }
        }

        // E 스킬 쿨타임 감소
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ESkillDataCoolTime.RemainingCooldownTime < 0)
            {
                ESkillDataCoolTime.RemainingCooldownTime = 0;  // 쿨타임이 0보다 작아지지 않도록 제한
            }
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

    #region Skill
    private List<GameObject> qSkillHitList = new List<GameObject>();
    private List<GameObject> eSkillHitList = new List<GameObject>();

    public override void ActivateQSkill()
    {
        // 쿨타임 판단
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.ActivateQSkill();
    }

    public override void ActivateESkill()
    {
        // 쿨타임 판단
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;

        
        base.ActivateESkill();
    }

    public override void Play_QSkillParticles()
    {
        if (qSkillParticlePrefab == null)
            return;

        // 회오리 전진 스킬
        

        // 리스트 data초마다 Clear , 다단히트용
        StartCoroutine(ClearHitListRoutine(qSkillHitList, QSkillDataCoolTime.MultiHitInterval));
    }

    // E 스킬 막타를 위한 정보 저장
    private bool isSecondary;
    public GameObject DualSkill02_PlusPrefab;

    public override void Play_ESkillParticles()
    {
        if (eSkillParticlePrefab == null)
            return;

        if(isSecondary)
        {
            // 막타 스킬
            Vector3 nPos = rootObject.transform.position;
            nPos += rootObject.transform.forward;


            Quaternion nQuaternion = rootObject.transform.rotation;
            nQuaternion *= Quaternion.Euler(0, 90, 0);

            GameObject nObj = Instantiate<GameObject>(DualSkill02_PlusPrefab, nPos, nQuaternion);
            CollisionAttackHandler collision = nObj.GetComponent<CollisionAttackHandler>();
            collision.InitData(rootObject, this,1.5f);

            isSecondary = false;
            return;
        }

        isSecondary = true;

        // 암?살 스킬
        Vector3 pos = rootObject.transform.position;
        pos += rootObject.transform.forward;
        pos += new Vector3(0, 1.5f, 0);

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ESkillDataCoolTime.ColliderDelay, ESkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerESkill;

        // 리스트 data초마다 Clear , 다단히트용
        StartCoroutine(ClearHitListRoutine(eSkillHitList, ESkillDataCoolTime.MultiHitInterval));
    }



    private void OnTriggerQSkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;

        if (qSkillHitList.Contains(target) == true)
            return;

        qSkillHitList.Add(target);

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ESkillData);
    }

    private void OnTriggerESkill(Collider t, Collider other, Vector3 hitPos)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage == null)
            return;

        GameObject target = other.transform.gameObject;

        if (eSkillHitList.Contains(target) == true)
            return;

        eSkillHitList.Add(target);

        Vector3 hitPointNew = t.ClosestPoint(other.transform.position);
        hitPointNew = other.transform.InverseTransformPoint(hitPointNew);
        damage.OnDamage(rootObject, this, hitPointNew, ESkillData);
    }

    private IEnumerator ClearHitListRoutine(List<GameObject> list, float interval)
    {
        float duration = ESkillDataCoolTime.ColliderDelay + ESkillDataCoolTime.ColliderDuration;
        float _time = 0f;

        while (_time < duration)
        {
            yield return new WaitForSeconds(interval);
            list.Clear();

            _time += interval;
        }
    }

    #endregion
}