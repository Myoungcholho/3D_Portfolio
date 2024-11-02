using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInformation : Character
{
    [SerializeField]
    public MonsterList monsterType;             // 몬스터 ID



    public Action<MonsterList> OnDeath;         // 사망 시 호출할 이벤트


}
