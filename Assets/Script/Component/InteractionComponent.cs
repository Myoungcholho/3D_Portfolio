using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class InteractionComponent : MonoBehaviour
{
    public TimelineAsset instantKillTimeline; // ���� Ÿ�Ӷ��� Inspector���� �Ҵ�
    public GameObject targetingUI;            // Canvas���ִ� Target �� UI

    // �켱������ ������.
    private SortedSet<IInteractable> interactables = new SortedSet<IInteractable>(new InteractableComparer());
    private RaycastShooter raycastShooter;
    private Animator animator;
    private StateComponent state;
    private PlayableDirector playableDirector;
    private BrainController brainController;
    
    private void Awake()
    {
        raycastShooter = GetComponent<RaycastShooter>();
        Debug.Assert(raycastShooter != null);
        
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        playableDirector = GetComponent<PlayableDirector>();
        brainController = Camera.main.GetComponent<BrainController>();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        actionMap.FindAction("Interaction").started += context =>
        {
            //animator.SetTrigger("IsStealAction");
            //animator.SetTrigger("StealDead");

            AddInteractablesToPriorityList();       // ��ȣ�ۿ� ������ ������Ʈ���� ����Ʈ�� ���
            ExecuteTopInteraction();                // ���� �켱������ ���� ������Ʈ�� ��ȣ�ۿ� ����� ����
        };
    }

    private void Update()
    {
        if (raycastShooter.hitForward.collider != null && targetingUI != null)
        {
            // 1. ���� �ش� ��ü�� InstantKillInteraction ������Ʈ�� ������ �ִ��� �Ǻ� ���ٸ� return
            InstantKillInteraction instantKill = raycastShooter.hitForward.collider.gameObject.GetComponent<InstantKillInteraction>();
            if (instantKill == null)
            {
                targetingUI.SetActive(false);
                return;
            }

            // 2. ���� ���� 0.9�̻����� �Ǻ�, (�÷��̾ ���� �ڿ��� ����� �������� �Ǻ�) �ƴ϶�� return
            float dotProduct = Vector3.Dot(transform.forward, raycastShooter.hitForward.transform.forward);
            if (dotProduct < 0.9f)
            {
                targetingUI.SetActive(false);
                return;
            }

            // 3. Ÿ���� 3D ��ġ�� 2D ��ũ�� ��ǥ�� ��ȯ
            Transform hitObjectPosition = raycastShooter.hitForward.collider.transform;
            Vector3 backPosition = hitObjectPosition.position + Vector3.up;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(backPosition);

            // 4. UI�� �ش� ��ũ�� ��ǥ�� �̵�
            targetingUI.transform.position = screenPosition;

            // 5. UI�� Ȱ��ȭ
            targetingUI.SetActive(true);
        }
        else
        {
            // Ÿ���� �����Ƿ� UI ��Ȱ��ȭ
            targetingUI.SetActive(false);
        }
    }

    // ��ȣ�ۿ� ����Ʈ�� �߰��ϴ� �޼���
    public void AddInteractablesToPriorityList()
    {
        interactables.Clear();

        // �켱������ ���� �߰�
        AddToInteractableList(raycastShooter.hitForward.collider);
        AddToInteractableList(raycastShooter.sphereHitCollider);
    }
    // ���� �켱������ ���� ��ȣ�ۿ��� �����ϴ� �޼���
    public void ExecuteTopInteraction()
    {
        if (interactables.Count > 0)
        {
            IInteractable topInteraction = interactables.Max;

            // ���� ���� ��ȣ�ۿ� �޼��� ����
            if(topInteraction is InstantKillInteraction instantKill)
            {
                // 0. ����� ���ϴ� ĳ������ �������� �Ǻ� 25���������� ���� dot() = 0.906
                Vector3 enemyForward = instantKill.transform.forward;
                float dotProduct = Vector3.Dot(enemyForward, transform.forward);
                if (dotProduct < 0.9) // ��25�����
                    return;

                // 1. ���¸� �ٲ����� �ٸ��� �ൿ ����
                state.SetInstantKillMode();

                // 2. ���� ��ġ�� ��� ���� �ڷ� �̵�
                Vector3 enemyPosition = instantKill.transform.position;  // ���� ��ġ
                Vector3 behindEnemyPosition = enemyPosition - instantKill.transform.forward * 1f;  // ���� �� 1f ��ġ�� �̵�
                behindEnemyPosition -= instantKill.transform.right * 0.3f;  // �¿츦 ������ ������
                transform.position = behindEnemyPosition;  // �÷��̾��� ��ġ�� ���� �ڷ� �̵�

                // 3. ĳ���Ͱ� ���� �ٶ󺸵��� ȸ��
                StartCoroutine(RotateTowardsTarget(instantKill.transform, 10f));

                // 4. �ִϸ������� Trigger�� �����Ͽ� ��� �ִϸ��̼� ����
                animator.SetTrigger("IsStealAction");

                // 5. PlayableDirector�� ����Ͽ� ��� Ÿ�Ӷ��� ���� �� ����
                PlayInstantKillTimeline();

                // 6. ī�޶� CUT���� ���� (Ÿ�Ӷ����� ������ ����)
                brainController.SetDefaultBlend("Cut", 0f);

                //����� ��ȣ�ۿ� �޼��� ����
                topInteraction.Interact();
            }

            if(topInteraction is PotalComponent potal)
            {
                //����� ��ȣ�ۿ� �޼��� ����
                SoundManager.Instance.PlaySound(SoundLibrary.Instance.teleport01, SoundLibrary.Instance.mixerBasic, false);
                topInteraction.Interact();
            }

        }
        else
        {
            Debug.Log("No interaction available.");
        }
    }

    // Ÿ�Ӷ����� ������ �� ȣ�����
    public void End_InstantTimeLine()
    {
        brainController.RollBackBlend();
    }

    // ����� õõ�� ȸ��
    IEnumerator RotateTowardsTarget(Transform target,float rotationSpeed)
    {
        // ��ǥ ������ ������ ������ ��� �ݺ��մϴ�.
        while (true)
        {
            // ���� forward ������ ����ϴ�.
            Vector3 targetForward = target.forward;

            // ���� forward ������ �������� ĳ������ ��ǥ ȸ������ ����մϴ�.
            Quaternion lookRotation = Quaternion.LookRotation(targetForward);

            // ���� ȸ���� ��ǥ ȸ�� ���̸� �ε巴�� �����մϴ�.
            while (Quaternion.Angle(transform.rotation, lookRotation) > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                yield return null; // ���� �����ӱ��� ���
            }

            yield break;
        }
    }

    // ��� Ÿ�Ӷ��� �޼���
    private void PlayInstantKillTimeline()
    {
        if (playableDirector != null && instantKillTimeline != null)
        {
            // PlayableDirector�� Ÿ�Ӷ����� ����
            playableDirector.playableAsset = instantKillTimeline;

            // Ÿ�Ӷ��� ��� ����
            playableDirector.Play();
        }
        else
        {
            Debug.LogError("PlayableDirector �Ǵ� TimelineAsset�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    // �ִϸ��̼ǿ� �̺�Ʈ ���� ȣ��Ǿ� ���� ���� ȸ��
    private void End_InstantKill()
    {
        state.SetIdleMode();
    }

    // Collider���� IInteractable�� ã�� List�� �߰��ϴ� �޼���
    private void AddToInteractableList(Collider collider)
    {
        if (collider != null)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactables.Add(interactable);
            }
        }
    }

    private class InteractableComparer : IComparer<IInteractable>
    {
        public int Compare(IInteractable x, IInteractable y)
        {
            // �켱������ ���� �ͺ��� ���� �� ������ ����
            return x.GetPriority().CompareTo(y.GetPriority());
        }
    }
}
