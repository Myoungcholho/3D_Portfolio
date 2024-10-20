using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillInteraction : MonoBehaviour,IInteractable
{
    // 기습시 실행할 파티클
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
        // 1. 기습 당한 상태로 변경 다른 로직은 처리하지 않기 위함.
        state.SetInstantKilledMode();

        // 2. 애니메이션 실행
        Invoke("TriggerAnimation", 0.4f); // 0.5초 지연

        // 3. 파티클 생성(피격 위치는 항상 같으므로 동일한 위치에
        GameObject obj = Instantiate(instantKillParticlePrefab, instantKillParitcleTransform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // 4. 객체 파괴
        Destroy(gameObject, 4f);
    }
    #endregion

    private void TriggerAnimation()
    {
        animator.SetTrigger("StealDead");
    }
}
