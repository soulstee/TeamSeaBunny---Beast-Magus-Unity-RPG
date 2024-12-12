using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : BaseAttack
{
    public Slash()
    {
        attackName = "Slash";
        attackDescription = "Perform a basic attack against an enemy. Deals standard damage.";
        attackDamage = 10f;
        attackManaCost = 0;
    }
}
