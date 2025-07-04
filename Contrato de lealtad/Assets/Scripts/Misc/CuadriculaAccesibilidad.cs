using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CuadriculaAccesibilidad : MonoBehaviour
{
    [SerializeField] private Tilemap gridTilemap;
    [SerializeField] private Tilemap mapaPrincipalTilemap;
    [SerializeField] private TileBase bordeTile;

    public float opacidadGuardada;

    private void Start()
    {
        float opacidadGuardada = PlayerPrefs.GetFloat("OpacidadCuadricula", 0f);
        GenerarCuadricula();
        ActualizarOpacidad(opacidadGuardada);
    }

    //Genera un tilemap encima del tilemap del nivel con las mismas dimensiones con la tile bordeTile
    public void GenerarCuadricula()
    {
        if (gridTilemap == null || mapaPrincipalTilemap == null || bordeTile == null)
        {
            return;
        }

        gridTilemap.ClearAllTiles();

        BoundsInt bounds = mapaPrincipalTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int originalPos = new Vector3Int(x, y, 0);

                if (mapaPrincipalTilemap.HasTile(originalPos))
                {
                    gridTilemap.SetTile(originalPos, bordeTile);
                }
            }
        }
    }

    //Cambio de la opacidad según las preferencias del jugador
    public void ActualizarOpacidad(float opacidad)
    {
        PlayerPrefs.SetFloat("OpacidadCuadricula", opacidad);

        Color nuevoColor = new Color(1f, 1f, 1f, opacidad);

        foreach (var pos in gridTilemap.cellBounds.allPositionsWithin)
        {
            if (gridTilemap.HasTile(pos))
            {
                gridTilemap.SetColor(pos, nuevoColor);
            }
        }
    }
}
