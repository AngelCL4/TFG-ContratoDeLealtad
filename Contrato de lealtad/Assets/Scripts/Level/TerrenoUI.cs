using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public class TerrenoUI : MonoBehaviour
{
    public Tilemap tilemap;
    public TerrainLibrary terrainLibrary;

    public Image imagenCasilla;
    public TextMeshProUGUI podText;
    public TextMeshProUGUI habText;
    public TextMeshProUGUI velText;
    public TextMeshProUGUI sueText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI resText;
    public TextMeshProUGUI movText;

    public void ActualizarInfoTerreno(Vector3Int cellPos)
    {
        TileBase tile = tilemap.GetTile(cellPos);

        if (tile == null)
        {
            imagenCasilla.sprite = null;
            podText.text = habText.text = velText.text = sueText.text = defText.text = resText.text = movText.text = "-";
            return;
        }

        //Obtener el tipo de terreno para mostrar las estadísticas a las que afecta
        TerrainType terreno = terrainLibrary.GetTerrainByTile(tile);
        if (terreno != null)
        {
            imagenCasilla.sprite = terrainLibrary.GetTileForTerrain(terreno).sprite;
            podText.text = terreno.powerBonus.ToString();
            habText.text = terreno.skillBonus.ToString();
            velText.text = terreno.speedBonus.ToString();
            sueText.text = terreno.luckBonus.ToString();
            defText.text = terreno.defenseBonus.ToString();
            resText.text = terreno.resistanceBonus.ToString();
            movText.text = terreno.movementBonus.ToString();
        }
        else
        {
            imagenCasilla.sprite = null;
            podText.text = habText.text = velText.text = sueText.text = defText.text = resText.text = movText.text = "-";
        }
    }
}
