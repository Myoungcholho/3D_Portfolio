using System;
using UnityEngine;

public class Sword : Melee
{
    [SerializeField]
    private string holsterName = "Holster_Sword";

    [SerializeField]
    private string handName = "Hand_Sword";

    public GameObject slashParicle;

    private Transform holsterTransform;
    private Transform handTransform;
    
    private Transform ss1;
    private Transform ss2;
    private Transform ss3;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Sword;
    }

    protected override void Awake()
    {
        base.Awake();
        
        ss1 = rootObject.transform.FindChildByName("SwordSlash01").GetComponent<Transform>();
        ss2 = rootObject.transform.FindChildByName("SwordSlash02").GetComponent<Transform>();
        ss3 = rootObject.transform.FindChildByName("SwordSlash03").GetComponent<Transform>();

        Debug.Assert(ss1 != null);
        Debug.Assert(ss2 != null);
        Debug.Assert(ss3 != null);
    }

    protected override void Start()
    {
        base.Start();

        holsterTransform = rootObject.transform.FindChildByName(holsterName);
        Debug.Assert(holsterTransform != null);

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(holsterTransform, false);
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(handTransform, false);
    }

    public override void UnEquip()
    {
        base.UnEquip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(holsterTransform, false);
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);


        ActivateSlash();
    }

    void ActivateSlash()
    {
        GameObject obj = null;
        if (index == 0)
        {
            obj = Instantiate<GameObject>(slashParicle, ss1);
        }
        else if(index == 1)
        {
            obj = Instantiate<GameObject>(slashParicle, ss2);
        }
        else if(index == 2)
        {
            obj = Instantiate<GameObject>(slashParicle, ss3);
        }
        
        if(obj != null) 
        {
            obj.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Destroy(obj, 2f);
        }
    }

    private int prevEvadeAction;
    public void DodgedDoAction()
    {
        state.SetDodgedAttackMode();
        Player player = rootObject.GetComponent<Player>();
        player.SetAnimationSpeed(1.0f);

        int evadeAction = UnityEngine.Random.Range(0, 3);
        if (evadeAction == prevEvadeAction)
            evadeAction = (evadeAction + 1) % 3;

        index = 3;

        animator.SetBool("IsDodgedAction", true);
        animator.SetInteger("DodgedPattern", evadeAction);
        animator.SetTrigger("DodgeAttack");

        prevEvadeAction = evadeAction; // ���� ���� ������ ���� ����
    }

    // �ִϸ��̼��� ���⸦ ȣ���ϴ� �κб���
    // �ִϸ��̼��� �߸��ξ Idle�� ���� �̽��� �־���.
    // ���� �ؾ� �ϴ� ����
    // 1. Dodged �� ��� ���콺�� ī�޶� ������ �Ұ����ϰ� �ϱ�
    // 2. DodgedAttack�� ��� ���ͷ� ȸ���� ����
    // 3. �� ������ MoveToWard�� �̵�
    public void End_DodgedDoAction()
    {
        // �߰��� Idle���� ��ȭ�ص� �ִϸ��̼ǿ� ���� �ʾ����� ȣ���
        // �ǵ�ġ ���� state������ ���� ����.
        if(state.DodgedAttackMode == true)
            state.SetDodgedMode();
    }
}