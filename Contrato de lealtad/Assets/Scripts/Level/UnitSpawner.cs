using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitSpawner : MonoBehaviour
{
    public GameObject aliadoPrefab;
    public GameObject enemigoPrefab;
    public MapLoader mapLoader;
    public static UnitSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Generación de unidades controlables por el jugador en las casillas indicadas por playerSpawnPositions
    public void SpawnAliados()
    {
        var aliados = SupportManager.Instance.GetPersonajesReclutados(GameManager.Instance.datosJuego.unidades.ToList());
        if (aliados == null || aliados.Count == 0)
        {
            Debug.LogWarning("No hay unidades reclutadas para spawnear.");
        }
        else
        {
            Debug.Log($"Se van a spawnear {aliados.Count} unidades.");
        }

        var spawnPoints = mapLoader.currentMapData.playerSpawnPositions;

        for (int i = 0; i < aliados.Count && i < spawnPoints.Length; i++)
        {
            Vector2 pos = spawnPoints[i];
            Vector3 worldPos = new Vector3(pos.x +0.5f, mapLoader.currentMapData.mapHeight - pos.y - 1 +0.5f, 0);

            GameObject go = Instantiate(aliadoPrefab, worldPos, Quaternion.identity);
            var unidad = go.GetComponent<UnitLoader>();
            unidad.ConfigurarUnidad(aliados[i], true);

            TurnManager.Instance.RegistrarUnidadAliada(unidad);

            Vector3Int celda = Vector3Int.FloorToInt(go.transform.position);
            Unidad u = unidad.datos;
            GameManager.Instance.mapaUnidades[celda] = u;
        }

        // Aplicar efectos de los artefactos al spawnear si tienen
        foreach (var unidad in FindObjectsOfType<UnitLoader>())
        {
            if (unidad.esAliado && unidad.TieneObjetos() && unidad.datos.objeto.tipo == "Artefacto")
            {
                unidad.AplicarEfectoArtefacto(unidad.datos.objeto);
            }
        }
    }

    // Generación de unidades enemigas en las casillas indicadas por enemySpawnPositions
    public void SpawnEnemigos()
    {
        var spawnPoints = mapLoader.currentMapData.enemySpawnPositions;


        TextAsset json = Resources.Load<TextAsset>($"Data/{GameManager.Instance.currentChapter}Enemies");
        if (json == null)
        {
            Debug.LogError("No se encontró el archivo Enemies.json del capítulo");
            return;
        }

        DatosEnemigos datos = JsonUtility.FromJson<DatosEnemigos>(json.text);
        var enemigos = datos.enemigos;

        for (int i = 0; i < enemigos.Count && i < spawnPoints.Length; i++)
        {
            Vector2 pos = spawnPoints[i];
            Vector3 worldPos = new Vector3(pos.x +0.5f, mapLoader.currentMapData.mapHeight - pos.y - 1 +0.5f, 0);

            GameObject go = Instantiate(enemigoPrefab, worldPos, Quaternion.identity);
            var unidad = go.GetComponent<UnitLoader>();
            unidad.ConfigurarUnidad(enemigos[i], false);

            TurnManager.Instance.RegistrarUnidadEnemiga(unidad);

            Vector3Int celda = Vector3Int.FloorToInt(go.transform.position);
            Unidad u = unidad.datos;
            GameManager.Instance.mapaUnidades[celda] = u;
        }
    }

    // Generación de refuerzos enemigos en las casillas indicadas en el archivo Reinforcements del capítulo
    public void SpawnearRefuerzos(int turno)
    {
        Debug.Log($"Comprobando refuerzos para el turno {turno}.");
        var refuerzosTurno = ChapterManager.Instance.refuerzos.Where(r => r.turno == turno).ToList();

        if (refuerzosTurno.Count == 0)
        {
            Debug.Log("No hay refuerzos programados para este turno.");
            return;
        }

        foreach (var refuerzo in refuerzosTurno)
        {
            float worldX = refuerzo.x + 0.5f;
            float worldY = mapLoader.currentMapData.mapHeight - refuerzo.y - 1 + 0.5f;
            Vector3 posicion = new Vector3(worldX, worldY, 0);

            Debug.Log($"Intentando spawnear refuerzo {refuerzo.unidad.nombre} en ({refuerzo.x}, {refuerzo.y})");

            // Si la casilla ya está ocupada por otra unidad, los refuerzos no aparecen
            Collider2D colision = Physics2D.OverlapPoint(posicion);
            if (colision != null)
            {
                Debug.LogWarning($"Casilla ocupada en {posicion}. Refuerzo no creado.");
                continue;
            }

            GameObject enemigoGO = Instantiate(enemigoPrefab, posicion, Quaternion.identity);
            var loader = enemigoGO.GetComponent<UnitLoader>();

            if (loader != null)
            {
                loader.ConfigurarUnidad(refuerzo.unidad, false);
                TurnManager.Instance.RegistrarUnidadEnemiga(loader);

                Vector3Int celda = Vector3Int.FloorToInt(enemigoGO.transform.position);
                GameManager.Instance.mapaUnidades[celda] = loader.datos;

                Debug.Log($"Refuerzo {refuerzo.unidad.nombre} creado");
            }
        }
    }
}