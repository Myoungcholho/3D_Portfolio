using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

        // ���� �տ� ����ġ
        for (int i = 0; i < (int)PartType.Max; i++)
        {
            Transform t = colliders[i].transform;
            swords[i] = t.gameObject;

            t.DetachChildren();
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;

            DualSwordTrigger trigger = t.GetComponent<DualSwordTrigger>();
            trigger.OnTrigger += DualTrigger;
            trigger.OnAttacker += Attacker;

            string partName = ((PartType)i).ToString();
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);

            t.SetParent(parent, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }


        // ���� ��Ȱ��ȭ
        for (int i=0; i<(int)PartType.Max; i++)
        {
            swords[i].SetActive(false);
        }

        // ������ ������ �и�
        handActionData = new Dictionary<PartType, List<DoActionData>>();

        foreach (var entry in handActionDataList)
        {
            if (!handActionData.ContainsKey(entry.partType))
            {
                handActionData.Add(entry.partType, entry.actionData);
            }
        }

    }

    protected override void Update()
    {
        // Q ��ų ��Ÿ�� ����
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
        {
            QSkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (QSkillDataCoolTime.RemainingCooldownTime < 0)
            {
                QSkillDataCoolTime.RemainingCooldownTime = 0;  // ��Ÿ���� 0���� �۾����� �ʵ��� ����
            }
        }

        // E ��ų ��Ÿ�� ����
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
        {
            ESkillDataCoolTime.RemainingCooldownTime -= Time.deltaTime;
            if (ESkillDataCoolTime.RemainingCooldownTime < 0)
            {
                ESkillDataCoolTime.RemainingCooldownTime = 0;  // ��Ÿ���� 0���� �۾����� �ʵ��� ����
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

    public override void Activate01Skill()
    {
        // ��Ÿ�� �Ǵ�
        if (QSkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        QSkillDataCoolTime.RemainingCooldownTime = QSkillDataCoolTime.CooldownTime;
        base.Activate01Skill();
    }

    public override void Activate02Skill()
    {
        // ��Ÿ�� �Ǵ�
        if (ESkillDataCoolTime.RemainingCooldownTime > 0)
            return;

        ESkillDataCoolTime.RemainingCooldownTime = ESkillDataCoolTime.CooldownTime;

        
        base.Activate02Skill();
    }

    public override void Play_01SkillParticles()
    {
        if (qSkillParticlePrefab == null)
            return;

        // ���� ȸ���� ��ų
        Vector3 pos = rootObject.transform.position;
        
        Quaternion quaternion = rootObject.transform.rotation;

        // ������Ʈ�� rootObject�� �ڽ����� ����
        GameObject obj = Instantiate(qSkillParticlePrefab, pos, quaternion, rootObject.transform);

        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(QSkillDataCoolTime.ColliderDelay, QSkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerQSkill;


        // ����Ʈ data�ʸ��� Clear , �ٴ���Ʈ��
        StartCoroutine(ClearHitListRoutine(qSkillHitList, QSkillDataCoolTime.MultiHitInterval));
    }

    // E ��ų ��Ÿ�� ���� ���� ����
    private bool isSecondary;
    public GameObject DualSkill02_PlusPrefab;

    public override void Play_02SkillParticles()
    {
        if (eSkillParticlePrefab == null)
            return;

        if(isSecondary)
        {
            // ��Ÿ ��ų
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

        // ��?�� ��ų
        Vector3 pos = rootObject.transform.position;
        pos += rootObject.transform.forward;
        pos += new Vector3(0, 1.5f, 0);

        Quaternion quaternion = rootObject.transform.rotation;

        GameObject obj = Instantiate<GameObject>(eSkillParticlePrefab, pos, quaternion);
        TriggerInvoker weaponTrigger = obj.GetComponent<TriggerInvoker>();
        weaponTrigger.Initialize(ESkillDataCoolTime.ColliderDelay, ESkillDataCoolTime.ColliderDuration);
        weaponTrigger.OnTriggerHit += OnTriggerESkill;

        // ����Ʈ data�ʸ��� Clear , �ٴ���Ʈ��
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

    public GameObject attack4CollisionPrefab;
    public override void Create_Attack_Collision()
    {
        Vector3 pos = transform.position;
        Quaternion quaternion = transform.rotation;

        GameObject obj = Instantiate<GameObject>(attack4CollisionPrefab, pos,quaternion);
        CollisionAttackHandler handler = obj.GetComponent<CollisionAttackHandler>();
        handler.InitData(rootObject, this, 0.2f);
    }

    // �ְ˿����� �������̵�
    private void DualTrigger(Collider other, PartType partType)
    {
        if (other.gameObject == rootObject)
            return;

        if (attacker == null)
            attacker = gameObject;
        string hashCode = CreateHashCode(other, attacker);

        if (hittedList.Contains(hashCode) == true)
            return;

        hittedList.Add(hashCode);                            // ��Ʈ�� ��ü ����

        Vector3 hitPoint = Vector3.zero;

        // �����ڿ� ��ġ�ϴ� �ݶ��̴��� �浹 ���� ���
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
        hitPoint = other.transform.InverseTransformPoint(hitPoint);


        var damage = other.gameObject.GetComponent<IDamagable>();

        List<DoActionData> HandData = handActionData[partType];
        damage?.OnDamage(rootObject, this, hitPoint, HandData[index]);
    }
}