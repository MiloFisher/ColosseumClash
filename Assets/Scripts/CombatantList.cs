using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatantList
{
    public int TOTAL_COMBATANTS = 4;

    public List<Combatant> Combatants()
    {
        List<Combatant> combatants = new List<Combatant>();
        for (int i = 0; i < TOTAL_COMBATANTS; i++)
            combatants.Add(Resources.Load<Combatant>("Combatants/" + i));
        return combatants;
    }
}
