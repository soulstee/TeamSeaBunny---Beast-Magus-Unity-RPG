using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : BaseAttack
{
    public Shoot()
    {
        attackName = "Shoot";
        attackDescription = "Shoot at an enemy with your ranged weapon. Deals standard damage.";
        attackDamage = 10f;
        attackManaCost = 0;
    }
}