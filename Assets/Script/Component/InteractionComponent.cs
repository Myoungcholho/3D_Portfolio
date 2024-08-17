using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionComponent : MonoBehaviour
{
    // 우선순위로 정렬함.
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
            AddInteractablesToPriorityList();       // 상호작용 가능한 오브젝트들을 리스트에 담고
            ExecuteTopInteraction();                // 가장 우선순위가 높은 오브젝트의 상호작용 기능을 실행
        };
    }

    // 가장 우선순위가 높은 상호작용을 실행하는 메서드
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

    // 상호작용 리스트에 추가하는 메서드
    public void AddInteractablesToPriorityList()
    {
        interactables.Clear();

        // 우선순위에 따라 추가
        AddToInteractableList(raycastShooter.hitBackward.collider);
        AddToInteractableList(raycastShooter.hitForward.collider);
        AddToInteractableList(raycastShooter.hitLeft.collider);
        AddToInteractableList(raycastShooter.hitRight.collider);
        AddToInteractableList(raycastShooter.sphereHitCollider);
    }


    // Collider에서 IInteractable을 찾아 List에 추가하는 메서드
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
            // 우선순위가 낮은 것부터 높은 것 순으로 정렬
            return x.GetPriority().CompareTo(y.GetPriority());
        }
    }
}
