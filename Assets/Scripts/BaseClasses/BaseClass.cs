using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseClass
{
    public string characterName;

    public float baseHP;
    public float currentHP;

    public float baseMP;
    public float currentMP;

    public float baseATK;
    public float currentATK;
    public float baseDEF;
    public float currentDEF;
    public float magicDEF;

    public List<BaseAttack> attacks = new List<BaseAttack>();
}