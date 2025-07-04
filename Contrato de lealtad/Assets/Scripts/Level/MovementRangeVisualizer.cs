using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementRangeVisualizer : MonoBehaviour
{
    [SerializeField] public Tilemap tilemap;
    [SerializeField] private Tilemap rangeTilemap;  // Tilemap para mostrar las casillas azules de rango
    [SerializeField] private Tile highlightTile; // Tile azul
    [SerializeField] private Tilemap attackTilemap; // Tilemap para mostrar las casillas rojas de rango de ataque
    [SerializeField] private Tile attackTile;  // Tile rojo 
    [SerializeField] private Tile enemyRangeTile; // Tile rosa
    [SerializeField] private Tilemap enemyTilemap; // Tilemap para rangos de ataque enemigos 
    [SerializeField] private Tile highlightEnemyTile;  // Tile purpura 
    [SerializeField] private Tilemap enemyHighlightTilemap; // Tilemap para rango de ataque enemigo elegido
    private List<Vector3Int> casillasAccesibles = new List<Vector3Int>();
    private Dictionary<Unidad, List<Vector3Int>> rangosDeEnemigos = new Dictionary<Unidad, List<Vector3Int>>();

    private string ObtenerEstadoUnidadEn(Vector3Int posicion)
    {
        if (GameManager.Instance.mapaUnidades.TryGetValue(posicion, out Unidad unidad))
        {
            return unidad.estado;
        }

        return null;
    }

    public void LimpiarCasillas()
    {
        foreach (var pos in casillasAccesibles)
            rangeTilemap.SetTile(pos, null);

        casillasAccesibles.Clear();
        LimpiarCasillasAtaque();
    }

    public void LimpiarCasillasAtaque()
    {
        attackTilemap.ClearAllTiles();
    }

    public List<Vector3Int> MostrarCasillasAccesibles(Vector3Int start, Unidad unidad, TerrainLibrary terrainLibrary, List<Vector3Int> ocupadas, bool mostrarVisualmente)
    {
        LimpiarCasillas();

        Queue<Nodo> frontera = new Queue<Nodo>();
        Dictionary<Vector3Int, int> costePorPos = new Dictionary<Vector3Int, int>();

        frontera.Enqueue(new Nodo(start, 0));
        costePorPos[start] = 0;

        Tilemap tilemapDestino = (unidad.estado == "Enemigo" || unidad.estado == "Jefe") ? enemyTilemap : rangeTilemap;
        Tile tileDestino = (unidad.estado == "Enemigo" || unidad.estado == "Jefe") ? enemyRangeTile : highlightTile;

        List<Vector3Int> resultado = new List<Vector3Int>();

        while (frontera.Count > 0)
        {
            Nodo actual = frontera.Dequeue();

            foreach (var dir in Direcciones())
            {
                Vector3Int vecino = actual.pos + dir;

                if (!tilemap.HasTile(vecino)) continue;

                // Si la casilla está ocupada por otra unidad y no es del mismo estado (es decir, son enemigos entre sí), cuenta como un muro
                bool estaOcupada = ocupadas.Contains(vecino);
                bool puedePasarPorAliado = false;

                if (estaOcupada)
                {
                    string estadoOcupante = ObtenerEstadoUnidadEn(vecino);
                    // Si son del mismo estado no cuenta como un muro
                    if (estadoOcupante == unidad.estado)
                    {
                        puedePasarPorAliado = true;
                    }
                }

                // Si está ocupada y no puede pasar, se ignora
                if (estaOcupada && !puedePasarPorAliado) continue;

                var terreno = terrainLibrary.GetTerrainByTile(tilemap.GetTile<TileBase>(vecino));
                if (terreno == null) continue;

                int coste = terreno.movementCost;
                
                if (unidad.clase.tipoMovimiento == "Pie")
                {
                    if (!terreno.isWalkable) continue;
                }
                else if (unidad.clase.tipoMovimiento == "Montado")
                {
                    if (!terreno.isWalkable) continue;
                    if (coste > 1) coste += 1;
                }
                else if (unidad.clase.tipoMovimiento == "Volador")
                {
                    coste = 1;
                }

                int nuevoCoste = actual.coste + coste;
                if (nuevoCoste > unidad.movimiento) continue;

                if (!costePorPos.ContainsKey(vecino) || nuevoCoste < costePorPos[vecino])
                {
                    costePorPos[vecino] = nuevoCoste;
                    frontera.Enqueue(new Nodo(vecino, nuevoCoste));
                    // Solo añadir al resultado si está libre o es la casilla inicial
                    if (!ocupadas.Contains(vecino) || vecino == start)
                    {
                        resultado.Add(vecino);
                        if (mostrarVisualmente)
                            tilemapDestino.SetTile(vecino, tileDestino);
                    }

                    if (vecino == start && mostrarVisualmente)
                        tilemapDestino.SetTile(start, tileDestino);
                }
            }
        }

        if (!resultado.Contains(start))
        {
            resultado.Add(start);
            if (mostrarVisualmente)
                tilemapDestino.SetTile(start, tileDestino);
        }

        if (unidad.estado != "Enemigo" && unidad.estado != "Jefe")
            casillasAccesibles = new List<Vector3Int>(resultado);

        return resultado;
    }

    public List<Vector3Int> MostrarCasillasAccesiblesEnemigo(Vector3Int start, Unidad unidad, TerrainLibrary terrainLibrary, List<Vector3Int> ocupadas, bool mostrarVisualmente)
    {
        LimpiarCasillas();

        Queue<Nodo> frontera = new Queue<Nodo>();
        Dictionary<Vector3Int, int> costePorPos = new Dictionary<Vector3Int, int>();

        frontera.Enqueue(new Nodo(start, 0));
        costePorPos[start] = 0;

        Tilemap tilemapDestino = enemyTilemap;
        Tile tileDestino = enemyRangeTile;

        List<Vector3Int> resultado = new List<Vector3Int>();

        while (frontera.Count > 0)
        {
            Nodo actual = frontera.Dequeue();

            foreach (var dir in Direcciones())
            {
                Vector3Int vecino = actual.pos + dir;

                if (!tilemap.HasTile(vecino)) continue;

                bool estaOcupada = ocupadas.Contains(vecino);
                bool puedePasar = true;

                if (estaOcupada)
                {
                    string estadoOcupante = ObtenerEstadoUnidadEn(vecino);

                    // Solo permitir el paso si es un aliado (enemigo o jefe)
                    if (estadoOcupante == "Enemigo" || estadoOcupante == "Jefe")
                    {
                        puedePasar = true;
                    }
                    else
                    {
                        puedePasar = false;
                    }

                    // Siempre puede estar en su propia casilla
                    if (vecino == start)
                    {
                        puedePasar = true;
                    }
                }

                if (estaOcupada && !puedePasar) continue;

                var terreno = terrainLibrary.GetTerrainByTile(tilemap.GetTile<TileBase>(vecino));
                if (terreno == null) continue;

                int coste = terreno.movementCost;

                if (unidad.clase.tipoMovimiento == "Pie")
                {
                    if (!terreno.isWalkable) continue;
                }
                else if (unidad.clase.tipoMovimiento == "Montado")
                {
                    if (!terreno.isWalkable) continue;
                    if (coste > 1) coste += 1;
                }
                else if (unidad.clase.tipoMovimiento == "Volador")
                {
                    coste = 1;
                }

                int nuevoCoste = actual.coste + coste;
                if (nuevoCoste > unidad.movimiento) continue;

                if (!costePorPos.ContainsKey(vecino) || nuevoCoste < costePorPos[vecino])
                {
                    costePorPos[vecino] = nuevoCoste;
                    frontera.Enqueue(new Nodo(vecino, nuevoCoste));

                    if (!ocupadas.Contains(vecino) || vecino == start)
                    {
                        resultado.Add(vecino);
                        if (mostrarVisualmente)
                            tilemapDestino.SetTile(vecino, tileDestino);
                    }

                    if (vecino == start && mostrarVisualmente)
                        tilemapDestino.SetTile(start, tileDestino);
                }
            }
        }

        if (!resultado.Contains(start))
        {
            resultado.Add(start);
            if (mostrarVisualmente)
                tilemapDestino.SetTile(start, tileDestino);
        }

        if (unidad.estado == "Enemigo" || unidad.estado == "Jefe")
            casillasAccesibles = new List<Vector3Int>(resultado);

        return resultado;
    }

    // Mostrar rango de ataque en base al rango mínimo y máximo de la unidad y sus posibles posiciones de movimiento
    // Si es un enemigo o jefe se muestra en el tilemap indicado, diferente a si es una unidad jugable
    public void MostrarRangoAtaque(List<Vector3Int> posiblesPosiciones, int rangoMin, int rangoMax, Unidad unidad)
    {
        Debug.Log(unidad.estado);
        HashSet<Vector3Int> casillasRangoAtaque = new HashSet<Vector3Int>();

        foreach (Vector3Int posicion in posiblesPosiciones)
        {
            for (int dx = -rangoMax; dx <= rangoMax; dx++)
            {
                for (int dy = -rangoMax; dy <= rangoMax; dy++)
                {
                    int distancia = Mathf.Abs(dx) + Mathf.Abs(dy);
                    if (distancia >= rangoMin && distancia <= rangoMax)
                    {
                        Vector3Int ataquePos = new Vector3Int(posicion.x + dx, posicion.y + dy, 0);
                        if ((unidad.estado != "Enemigo" && unidad.estado != "Jefe") && casillasAccesibles.Contains(ataquePos)) continue;
                        casillasRangoAtaque.Add(ataquePos);
                    }
                }
            }
        }

        Tilemap tilemapDestino = (unidad.estado == "Enemigo" || unidad.estado == "Jefe") ? enemyTilemap : attackTilemap;
        Tile tileDestino = (unidad.estado == "Enemigo" || unidad.estado == "Jefe") ? enemyRangeTile : attackTile;

        foreach (var pos in casillasRangoAtaque)
        {
            tilemapDestino.SetTile(pos, tileDestino);
        }

    }

    public bool EsCasillaAccesible(Vector3Int cell) => casillasAccesibles.Contains(cell);

    private struct Nodo
    {
        public Vector3Int pos;
        public int coste;
        public Nodo(Vector3Int p, int c)
        {
            pos = p;
            coste = c;
        }
    }

    private List<Vector3Int> Direcciones() => new List<Vector3Int> {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    // Mostrar rango de ataque de un enemigo específico
    public void MostrarRangoDeEnemigo(Unidad enemigo, List<Vector3Int> rangoAtaque)
    {
        foreach (var casilla in rangoAtaque)
        {
            enemyHighlightTilemap.SetTile(casilla, highlightEnemyTile);
        }
        rangosDeEnemigos[enemigo] = rangoAtaque;
    }

    // Limpiar el rango de ataque de un enemigo específico
    public void LimpiarRangoDeEnemigo(Unidad enemigo)
    {
        if (rangosDeEnemigos.ContainsKey(enemigo))
        {
            foreach (var casilla in rangosDeEnemigos[enemigo])
            {
                enemyHighlightTilemap.SetTile(casilla, null);
            }
            rangosDeEnemigos.Remove(enemigo);
        }
    }

    // Limpiar todos los rangos de ataque de todos los enemigos
    public void LimpiarRangosDeEnemigos()
    {
        enemyTilemap.ClearAllTiles();
    }
}
