using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualSword : Melee
{
    protected override void Reset()
    {
        base.Reset();
        type = WeaponType.DualSword;
    }
}
