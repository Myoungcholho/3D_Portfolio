using UnityEngine;

interface IInteractable
{
    int GetPriority();      // 상호작용 우선순위를 반환
    void Interact();        // 상호작용 동작을 정의

}
