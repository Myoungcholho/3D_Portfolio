using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionComponent : MonoBehaviour
{
    // �켱������ ������.
    private SortedSet<IInteractable> interactables = new SortedSet<IInteractable>(new InteractableComparer());
    private RaycastShooter raycastShooter;


    private void Awake()
    {
        raycastShooter = GetComponent<RaycastShooter>();
        Debug.Assert(raycastShooter != null);

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        actionMap.FindAction("Interaction").started += context =>
        {
            AddInteractablesToPriorityList();       // ��ȣ�ۿ� ������ ������Ʈ���� ����Ʈ�� ���
            ExecuteTopInteraction();                // ���� �켱������ ���� ������Ʈ�� ��ȣ�ۿ� ����� ����
        };
    }

    // ���� �켱������ ���� ��ȣ�ۿ��� �����ϴ� �޼���
    public void ExecuteTopInteraction()
    {
        if (interactables.Count > 0)
        {
            IInteractable topInteraction = interactables.Max;
            topInteraction.Interact();
        }
        else
        {
            Debug.Log("No interaction available.");
        }
    }

    // ��ȣ�ۿ� ����Ʈ�� �߰��ϴ� �޼���
    public void AddInteractablesToPriorityList()
    {
        interactables.Clear();

        // �켱������ ���� �߰�
        AddToInteractableList(raycastShooter.hitBackward.collider);
        AddToInteractableList(raycastShooter.hitForward.collider);
        AddToInteractableList(raycastShooter.hitLeft.collider);
        AddToInteractableList(raycastShooter.hitRight.collider);
        AddToInteractableList(raycastShooter.sphereHitCollider);
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
