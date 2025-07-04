using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapCell
{
    public int x;
    public int y;
    public string terrain;

    public float rotation;
    public bool flipX;
    public bool flipY;
}

[System.Serializable]
public class MapData
{
    public int mapWidth;
    public int mapHeight;
    public MapCell[] cells;
    public Vector2[] playerSpawnPositions;
    public Vector2[] enemySpawnPositions;
}