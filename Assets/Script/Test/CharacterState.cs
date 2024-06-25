using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public enum StateType
    {
        Idle,Attack,Max
    }

public class CharacterState : MonoBehaviour
{
    public StateType type;

    private void Start()
    {
        type = StateType.Idle;
    }

    public void SetIdle() {  type = StateType.Idle; }
    public void SetAttack() { type = StateType.Attack; }

    public StateType GetType() { return type; }
}
