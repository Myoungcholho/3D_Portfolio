using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillInteraction : MonoBehaviour,IInteractable
{
    // ����� ������ ��ƼŬ
    public GameObject instantKillParticlePrefab;
    public Transform instantKillParitcleTransform;

    private StateComponent state;
    private Animator animator;

    private void Awake()
    {
        state = GetComponent<StateComponent>();
        animator = GetComponent<Animator>();
    }

    #region Iteraction
    public int GetPriority()
    {
        return (int)InteractionPriority.InstantKillI;
    }

    public void Interact()
    {
        // 1. ��� ���� ���·� ���� �ٸ� ������ ó������ �ʱ� ����.
        state.SetInstantKilledMode();

        // 2. �ִϸ��̼� ����
        Invoke("TriggerAnimation", 0.4f); // 0.5�� ����

        // 3. ��ƼŬ ����(�ǰ� ��ġ�� �׻� �����Ƿ� ������ ��ġ��
        GameObject obj = Instantiate(instantKillParticlePrefab, instantKillParitcleTransform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // 4. ��ü �ı�
        Destroy(gameObject, 4f);
    }
    #endregion

    private void TriggerAnimation()
    {
        animator.SetTrigger("StealDead");
    }
}
