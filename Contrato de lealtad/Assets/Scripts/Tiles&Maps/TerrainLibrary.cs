using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TerrainLibrary", menuName = "Tiles/TerrainLibrary")]
public class TerrainLibrary : ScriptableObject
{
    [System.Serializable]
    public class TerrainEntry
    {
        public TerrainType terrainType;
        public TerrainTile terrainTile;
    }

    public List<TerrainEntry> terrains;

    private Dictionary<string, TerrainEntry> terrainLookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        terrainLookup = new Dictionary<string, TerrainEntry>();
        foreach (var entry in terrains)
        {
            if (entry.terrainType != null && !string.IsNullOrEmpty(entry.terrainType.terrainName))
            {
                terrainLookup[entry.terrainType.terrainName] = entry;
            }
        }
    }

    //Obtener tipo de terreno por nombre
    public TerrainType GetTerrainByName(string terrainName)
    {
        if (terrainLookup == null) BuildLookup();
        if (terrainLookup.TryGetValue(terrainName, out var entry))
        {
            return entry.terrainType;
        }
        return null;
    }

    //Obtener tile de terreno por tipo
    public TerrainTile GetTileForTerrain(TerrainType terrainType)
    {
        if (terrainType == null) return null;
        if (terrainLookup == null) BuildLookup();
        if (terrainLookup.TryGetValue(terrainType.terrainName, out var entry))
        {
            return entry.terrainTile;
        }
        return null;
    }

    //Obtener tipo de terreno por tile
    public TerrainType GetTerrainByTile(TileBase tile)
    {
        if (tile == null) return null;
        if (terrains == null) return null;

        foreach (var entry in terrains)
        {
            if (entry.terrainTile == tile)
                return entry.terrainType;
        }
        return null;
    }
    }
