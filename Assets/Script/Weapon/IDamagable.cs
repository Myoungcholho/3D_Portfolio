using UnityEngine;

public interface IDamagable
{
    // ������ , ���� Ÿ��, ���� ��ġ, ó���� ������
    void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data);
}