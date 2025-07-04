using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class ObjectiveMenu : MonoBehaviour
{
    public Tilemap escapeTilemap;
    public Tile yellowTile;
    public GameObject PanelLimiteTurnos;
    public TextMeshProUGUI LimiteText;
    public TextMeshProUGUI victoryDetailsText;
    public TextMeshProUGUI defeatDetailsText;
    public ObjectiveData data;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => !string.IsNullOrEmpty(ChapterManager.Instance.currentChapter));

        string chapter = ChapterManager.Instance.currentChapter;
        TextAsset json = Resources.Load<TextAsset>($"Data/{chapter}Objective");

        if (json == null)
        {
            Debug.LogError($"No se encontró el archivo de objetivos para el capítulo");
            yield break;
        }

        data = JsonUtility.FromJson<ObjectiveData>(json.text);

        // Si el objetivo es 'Escapar'
        if (data.victoryCondition == "Escapar")
        {
            // Crear la casilla amarilla en el tilemap en las coordenadas x e y dadas
            Vector3 escapePosition = new Vector3(data.x, data.y, 0);
            Vector3Int gridPosition = escapeTilemap.WorldToCell(escapePosition);

            escapeTilemap.SetTile(gridPosition, yellowTile);
        }

        // Si el objetivo es 'Sobrevivir'
        if (data.victoryCondition == "Sobrevivir" && !ChapterManager.Instance.chapterCompleted)
        {
            PanelLimiteTurnos.SetActive(true);  // Activar el panel de la cantidad de turnos que el jugador tiene que sobrevivir

            // Actualizar el texto del panel con el turno inicial
            if (TurnManager.Instance != null)
            {
                LimiteText.text = $"{TurnManager.Instance.TurnoActual} / {data.turnos}";
            }
        }
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Si se pulsa S cuando el menú está activado, se cierra
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
            gameObject.SetActive(false);
        }

        if (ChapterManager.Instance.chapterCompleted)
        {
            PanelLimiteTurnos.SetActive(false);
        }
    }

    // Actualización del texto de límite de turnos, que se usa al principio de cada turno
    public void ActualizarTextoLimiteTurnos()
    {
        if (TurnManager.Instance.TurnoActual <= data.turnos)
        {
            LimiteText.text = $"{TurnManager.Instance.TurnoActual} / {data.turnos}";
        }
        else 
        {
            PanelLimiteTurnos.SetActive(false);
        }
    }

    public void Abrir()
    {
        gameObject.SetActive(true);
        
        victoryDetailsText.text = data.victoryDetails;
        defeatDetailsText.text = data.defeatDetails;        
    }
}

[System.Serializable]
public class ObjectiveData
{
    public string victoryCondition;
    public string victoryDetails;
    public string defeatDetails;

    public int turnos;
    public float x;
    public float y;
}
