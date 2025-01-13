using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseAttack : MonoBehaviour
{
    public string attackName;// name of attack
    public string attackDescription;
    public float attackDamage;// How much damanage a hero is dealing
    public float attackManaCost;// If a hero is casting a spell, calculate how much mana to spend
    public bool magic = false;
}
