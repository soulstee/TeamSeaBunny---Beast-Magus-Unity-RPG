using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire1Spell : BaseAttack
{
    public Fire1Spell()
    {
        attackName = "Fire";
        attackDescription = "A basic fire spell that can burn enemies. Easy to cast and deals standard damage.";
        attackDamage = 10f;
        attackManaCost = 10f;
    }
}
