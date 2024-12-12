using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseEnemy: BaseClass
{
    public enum Type
    {
        Earth,
        Fire,
        Water,
        Wind
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Superrare
    }

    public Type EnemyType;
    public Rarity rarity;
}
