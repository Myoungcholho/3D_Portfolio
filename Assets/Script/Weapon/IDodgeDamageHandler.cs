using UnityEngine;

public interface IDodgeDamageHandler
{
    // 공격자 , 무기 타입, 맞은 위치, 처리할 데이터
    void OnDodgeDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data);

}