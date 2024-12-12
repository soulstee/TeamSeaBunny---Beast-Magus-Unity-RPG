using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSpell : BaseAttack
{
    public PoisonSpell()
    {
        attackName = "Poison";
        attackDescription = "A basic poison spell that deals damage over time.";
        attackDamage = 5f;
        attackManaCost = 5f;
    }
}
