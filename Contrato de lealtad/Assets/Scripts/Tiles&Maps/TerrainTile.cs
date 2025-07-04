using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTerrainTile", menuName = "Map/Terrain Tile")]
public class TerrainTile : Tile
{
    public TerrainType terrainType;
}
