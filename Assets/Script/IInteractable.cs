using UnityEngine;

public enum InteractionPriority
{
    InstantKillI = 2,
    NpcComunication =3,
}

interface IInteractable
{
    int GetPriority();      // ��ȣ�ۿ� �켱������ ��ȯ
    void Interact();        // ��ȣ�ۿ� ������ ����

}
