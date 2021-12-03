using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionList
{
    public int TOTAL_CHAMPIONS = 9;

    public List<Champion> Champions()
    {
        List<Champion> champions = new List<Champion>();
        for(int i = 0; i < TOTAL_CHAMPIONS; i++ )
            champions.Add(Resources.Load<Champion>("Champions/" + i));
        return champions;
    }
}
