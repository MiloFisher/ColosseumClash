using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Combatant", menuName = "ScriptableObjects/Combatant", order = 2)]
public class Combatant : ScriptableObject
{
    public string combatantName;
    public Sprite image;
    public int health;
    public string attack;
    public int defense;
    public string effect;
    public int amountInDeck;
    public int stars;
}
