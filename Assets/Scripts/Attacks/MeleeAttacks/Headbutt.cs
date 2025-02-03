using System.Collections;
using UnityEngine;

public class Headbutt : BaseAttack
{
    public Headbutt()
    {
        attackName = "Headbutt";
        attackDescription = "Attack your enemies with your horn directly at them. Deals minimal damage.";
        attackDamage = 10f;
        attackManaCost = 0;
    }
}
