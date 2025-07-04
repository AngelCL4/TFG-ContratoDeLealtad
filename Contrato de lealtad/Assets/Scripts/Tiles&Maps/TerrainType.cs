using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainType", menuName = "Map/Terrain Type")]
public class TerrainType : ScriptableObject
{
    public string terrainName;
    public int movementCost;
    public bool isWalkable;
    public int powerBonus;
    public int skillBonus;
    public int speedBonus;
    public int luckBonus;
    public int defenseBonus;
    public int resistanceBonus;
    public int movementBonus;
}
