using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSwing : BaseAttack
{
    public PowerSwing()
    {
        attackName = "PowerSwing";
        attackDescription = "Swing your weapon with all your might against an enemy. A powerful, yet slow attack.";
        attackDamage = 20f;
        attackManaCost = 0;
    }
}
