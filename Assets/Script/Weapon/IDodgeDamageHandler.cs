using UnityEngine;

public interface IDodgeDamageHandler
{
    // ������ , ���� Ÿ��, ���� ��ġ, ó���� ������
    void OnDodgeDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data);

}