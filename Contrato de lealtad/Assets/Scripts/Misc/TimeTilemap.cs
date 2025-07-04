using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TimeTilemap : MonoBehaviour
{
    [SerializeField] public Tilemap timeTilemap;
    [SerializeField] private Tilemap mapaPrincipalTilemap;
    [SerializeField] private TileBase eveningTile;
    [SerializeField] private TileBase nightTile;

    // Funciones similares a la de generación de cuadrículas, solo que con diferentes tiles
    public void GenerarCuadriculaTarde()
    {
        if (timeTilemap == null || mapaPrincipalTilemap == null || eveningTile == null)
        {
            return;
        }

        timeTilemap.ClearAllTiles();

        BoundsInt bounds = mapaPrincipalTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int originalPos = new Vector3Int(x, y, 0);

                if (mapaPrincipalTilemap.HasTile(originalPos))
                {
                    timeTilemap.SetTile(originalPos, eveningTile);
                }
            }
        }
    }

    public void GenerarCuadriculaNoche()
    {
        if (timeTilemap == null || mapaPrincipalTilemap == null || nightTile == null)
        {;
            return;
        }

        timeTilemap.ClearAllTiles();

        BoundsInt bounds = mapaPrincipalTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int originalPos = new Vector3Int(x, y, 0);

                if (mapaPrincipalTilemap.HasTile(originalPos))
                {
                    timeTilemap.SetTile(originalPos, nightTile);
                }
            }
        }
    }
}
