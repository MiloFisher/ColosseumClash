using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Champion", menuName = "ScriptableObjects/Champion", order = 1)]
public class Champion : ScriptableObject
{
    public new string name;
    public Sprite image;
    public int health;
    public string attack;
    public int defense;
    public string effect;
    public int stars;
}
