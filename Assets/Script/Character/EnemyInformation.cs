using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInformation : Character
{
    [SerializeField]
    public MonsterList monsterType;             // ���� ID



    public Action<MonsterList> OnDeath;         // ��� �� ȣ���� �̺�Ʈ


}
