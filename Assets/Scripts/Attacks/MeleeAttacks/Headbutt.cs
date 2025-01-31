using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Headbutt : BaseAttack
{
    public Headbutt()
    {
        attackName = "Headbutt";
        attackDescription = "Attack your enemeies with your horn directly at them. Deals minimal damage.";
        attackDamage = 10f;
        attackManaCost = 0;
    }
}