using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    public enum TestStateType
    {
        Idle,Attack,Max
    }

public class CharacterState : MonoBehaviour
{
    public TestStateType type;

    private void Start()
    {
        type = TestStateType.Idle;
    }

    public void SetIdle() {  type = TestStateType.Idle; }
    public void SetAttack() { type = TestStateType.Attack; }

    public TestStateType GetStateType() { return type; }
}
