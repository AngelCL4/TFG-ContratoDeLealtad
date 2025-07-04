using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using GestionDeClases;

public class UnitLoader : MonoBehaviour
{
    public Unidad datos; 
    public bool esAliado;
    private int powerBonusActual = 0;
    private int skillBonusActual = 0;
    private int speedBonusActual = 0;
    private int luckBonusActual = 0;
    private int defenseBonusActual = 0;
    private int resistanceBonusActual = 0;
    private int movementBonusActual = 0;
    public bool yaActuo = false;
    public UnitUIHandler barraVida;
    public Sprite spriteClasePersonalizado;
    private Dictionary<string, (int valor, int turnos, int decremento)> buffs = new();
    private Dictionary<string, int> buffsPermanentes = new();

    [System.Serializable]
    public class EfectoArtefactoActivo {
        public UnitLoader fuente;
        public UnitLoader objetivo;
        public string stat;
        public int valor;
    }
    public List<EfectoArtefactoActivo> efectosArtefacto = new List<EfectoArtefactoActivo>();
    public List<EfectoArtefactoActivo> efectosArtefactoEntrantes = new();

    public void ConfigurarUnidad(Unidad unidad, bool esAliado)
    {
        datos = unidad;
        this.esAliado = esAliado;
        // Si la unidad es aliada su sprite de clase es el nombre de su clase más el nombre de la unidad
        if (esAliado)
        {
            string rutaSprite = $"Sprites/{unidad.clase.nombre.Replace(" ", "")}{unidad.nombre}";
            Sprite spriteClasePersonalizado = Resources.Load<Sprite>(rutaSprite);
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

            if (sr != null && spriteClasePersonalizado != null)
            {
                sr.sprite = spriteClasePersonalizado;
            }
            else
            {
                Debug.LogWarning($"No se encontró sprite para {rutaSprite}");
            }
        }
        // Si la unidad es enemiga
        else 
        {
            int objetivoNivel = GameManager.Instance.nivelMedioEjercito + 1;
            // Si es el jefe, activar el simbolo de comandante en el prefab
            if (unidad.estado == "Jefe")
            {
                Transform simbolo = transform.Find("SimboloComandante");
                if (simbolo != null)
                {
                    simbolo.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("No se encontró 'SimboloComandante'");
                }
                // El jefe está dos niveles por encima de la media del ejército
                objetivoNivel += 1;
            }
            // Subir de nivel al resto de enemigos
            if (unidad.nivel < objetivoNivel)
            {
                while (unidad.nivel < objetivoNivel)
                {
                    SubirNivel(unidad);
                }
                // Si aumentan la vida, asegurarse de que se mantenga al máximo
                unidad.PV = unidad.MaxPV;
            }
            // Los enemigos usan la variable sprite para su sprite de clase
            string rutaSprite = $"Sprites/{unidad.sprite}";
            Sprite claseSprite = Resources.Load<Sprite>(rutaSprite);
            Transform spritePersonaje = transform.Find("Sprite");
            if (spritePersonaje != null)
            {
                SpriteRenderer sr = spritePersonaje.GetComponent<SpriteRenderer>();
                if (sr != null && claseSprite != null)
                {
                    sr.sprite = claseSprite;
                }
            }
            else
            {
                Debug.LogWarning($"Sprite no encontrado: {unidad.sprite}");
            }
        }

        // Configurar la barra de vida con los datos de la unidad
        barraVida = GetComponentInChildren<UnitUIHandler>();
        if (barraVida != null)
        {
            barraVida.Configurar(datos);
        }

        // Aplicar bonos de clase, solo se aplican una vez por bonos = true, y estos bonos nunca se borran
        if (!datos.bonos && unidad.clase.nombre != null)
        {
            datos.MaxPV     += unidad.clase.bonusPV;
            datos.PV = datos.MaxPV;
            datos.poder     += unidad.clase.bonusPoder;
            datos.habilidad += unidad.clase.bonusHabilidad;
            datos.velocidad += unidad.clase.bonusVelocidad;
            datos.suerte    += unidad.clase.bonusSuerte;
            datos.defensa   += unidad.clase.bonusDefensa;
            datos.resistencia += unidad.clase.bonusResistencia;
            datos.movimiento += unidad.clase.bonusMovimiento;

            datos.bonos = true;
        }

        // Icono del objeto si la unidad tiene uno equipado
        if (!string.IsNullOrEmpty(unidad.objeto?.spritePath))
        {
            unidad.objeto.icono = Resources.Load<Sprite>(unidad.objeto.spritePath);
        }
    }

    public void ActualizarBonosPorTerreno(Tilemap tilemap, TerrainLibrary terrainLibrary)
    {
        // Revertir los bonos anteriores
        datos.poder -= powerBonusActual;
        datos.habilidad -= skillBonusActual;
        datos.velocidad -= speedBonusActual;
        datos.suerte -= luckBonusActual;
        datos.defensa -= defenseBonusActual;
        datos.resistencia -= resistanceBonusActual;
        datos.movimiento -= movementBonusActual;

        // Reiniciar los valores acumulados
        powerBonusActual = skillBonusActual = speedBonusActual = luckBonusActual =
        defenseBonusActual = resistanceBonusActual = movementBonusActual = 0;

        // Aplicar nuevos bonos
        Vector3Int cell = tilemap.WorldToCell(transform.position);
        TileBase tile = tilemap.GetTile(cell);
        TerrainType terreno = terrainLibrary.GetTerrainByTile(tile);

        if (terreno != null)
        {
            powerBonusActual = terreno.powerBonus;
            skillBonusActual = terreno.skillBonus;
            speedBonusActual = terreno.speedBonus;
            luckBonusActual = terreno.luckBonus;
            defenseBonusActual = terreno.defenseBonus;
            resistanceBonusActual = terreno.resistanceBonus;
            movementBonusActual = terreno.movementBonus;

            datos.poder += powerBonusActual;
            datos.habilidad += skillBonusActual;
            datos.velocidad += speedBonusActual;
            datos.suerte += luckBonusActual;
            datos.defensa += defenseBonusActual;
            datos.resistencia += resistanceBonusActual;
            datos.movimiento += movementBonusActual;
        }
    }

    public List<UnitLoader> EnemigosEnRango()
    {
        List<UnitLoader> enemigos = new List<UnitLoader>();
        Tilemap tilemap = FindObjectOfType<Tilemap>();

        foreach (var u in FindObjectsOfType<UnitLoader>())
        {
            if (!u.esAliado)
            {
                // Obtener las posiciones en el mundo de ambas unidades
                Vector3 posActual = transform.position;
                Vector3 posEnemigo = u.transform.position;

                // Convertir las posiciones del mundo a celdas en el Tilemap
                Vector3Int posActualCelda = tilemap.WorldToCell(posActual);
                Vector3Int posEnemigoCelda = tilemap.WorldToCell(posEnemigo);

                posActualCelda.x += (posActual.x - posActualCelda.x) > 0.5f ? 1 : 0;
                posActualCelda.y += (posActual.y - posActualCelda.y) > 0.5f ? 1 : 0;

                posEnemigoCelda.x += (posEnemigo.x - posEnemigoCelda.x) > 0.5f ? 1 : 0;
                posEnemigoCelda.y += (posEnemigo.y - posEnemigoCelda.y) > 0.5f ? 1 : 0;

                int diffX = Mathf.Abs(posActualCelda.x - posEnemigoCelda.x);
                int diffY = Mathf.Abs(posActualCelda.y - posEnemigoCelda.y);

                bool enRango = false;
                
                // Si el rango de ataque es de 1 a 2
                if (datos.clase.rangoAtaqueMaximo == 2 && datos.clase.rangoAtaqueMinimo == 1)
                {
                    // Aceptar las casillas adyacentes y las casillas de rango 2
                    if ((diffX == 1 && diffY == 0) || (diffY == 1 && diffX == 0) ||
                        (diffX == 2 && diffY == 0) || (diffY == 2 && diffX == 0) || (diffX == 1 && diffY == 1))
                    {
                        enRango = true;
                    }
                }

                // Si el rango de ataque es 1
                else if (datos.clase.rangoAtaqueMaximo == 1)
                {
                    // Aceptar solo las casillas adyacentes
                    if ((diffX == 1 && diffY == 0) || (diffY == 1 && diffX == 0))
                    {
                        enRango = true;
                    }
                }
                // Si el rango de ataque es 2
                else if (datos.clase.rangoAtaqueMaximo == 2)
                {
                    // Aceptar las casillas en diagonal o a distancia 2 
                    if ((diffX == 2 && diffY == 0) || (diffY == 2 && diffX == 0) || (diffX == 1 && diffY == 1))
                    {
                        enRango = true;
                    }
                }

                // Si está en rango, lo añadimos a la lista
                if (enRango)
                {
                    enemigos.Add(u);
                }
            }
        }

        return enemigos;
    }

    //Funciones usadas en ActionMenu para mostrar botones

    public static List<UnitLoader> ObtenerEnemigosEnRangoDesde(Vector3Int origenCelda, int rangoMin, int rangoMax, bool soyAliado, Tilemap tilemap)
    {
        List<UnitLoader> enemigos = new List<UnitLoader>();

        foreach (var u in GameObject.FindObjectsOfType<UnitLoader>())
        {
            if (u.esAliado == soyAliado) continue;

            Vector3Int celdaObjetivo = tilemap.WorldToCell(u.transform.position);

            int dx = Mathf.Abs(celdaObjetivo.x - origenCelda.x);
            int dy = Mathf.Abs(celdaObjetivo.y - origenCelda.y);
            int distancia = dx + dy;

            if (distancia >= rangoMin && distancia <= rangoMax)
            {
                enemigos.Add(u);
            }
        }

        return enemigos;
    }

    public bool TieneObjetos()
    {
        return datos.objeto != null && !string.IsNullOrEmpty(datos.objeto.nombre);
    }

    public List<UnitLoader> UnidadesAliadasAdyacentes()
    {
        List<UnitLoader> aliados = new List<UnitLoader>();
        Vector3Int[] direcciones = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };
        Tilemap tilemap = FindObjectOfType<Tilemap>();

        foreach (var dir in direcciones)
        {
            Vector3Int pos = tilemap.WorldToCell(transform.position) + dir;
            Vector3 worldPos = tilemap.GetCellCenterWorld(pos);
            Collider2D col = Physics2D.OverlapPoint(worldPos);

            if (col != null && col.gameObject != this.gameObject) // ⬅️ evita autocolisión
            {
                if (col.TryGetComponent(out UnitLoader aliado) && aliado.esAliado)
                {
                    aliados.Add(aliado);
                }
            }
        }
        return aliados;
    }

    public List<UnitLoader> UnidadesAliadasAdyacentesHeridas()
    {
        List<UnitLoader> aliadosHeridos = new List<UnitLoader>();

        // Verificación, solo cuatro clases pueden curar
        string clase = datos.clase.nombre;
        if (clase != "Curandero" && clase != "Clerigo" && clase != "Trovador" && clase != "Caballero Mago")
        {
            return aliadosHeridos; // Si no puede curar, devuelve lista vacía
        }

        Vector3Int[] direcciones = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };
        Tilemap tilemap = FindObjectOfType<Tilemap>();

        foreach (var dir in direcciones)
        {
            Vector3Int pos = tilemap.WorldToCell(transform.position) + dir;
            Vector3 worldPos = tilemap.GetCellCenterWorld(pos);
            Collider2D col = Physics2D.OverlapPoint(worldPos);

            if (col != null && col.gameObject != this.gameObject)
            {
                if (col.TryGetComponent(out UnitLoader aliado) && aliado.esAliado)
                {
                    if (aliado.datos.PV < aliado.datos.MaxPV)
                    {
                        aliadosHeridos.Add(aliado);
                    }
                }
            }
        }

        return aliadosHeridos;
    }

    public List<UnitLoader> UnidadesAliadasAdyacentesConObjeto()
    {
        List<UnitLoader> aliadosConObjeto = new List<UnitLoader>();
        Vector3Int[] direcciones = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };
        Tilemap tilemap = FindObjectOfType<Tilemap>();

        foreach (var dir in direcciones)
        {
            Vector3Int pos = tilemap.WorldToCell(transform.position) + dir;
            Vector3 worldPos = tilemap.GetCellCenterWorld(pos);
            Collider2D col = Physics2D.OverlapPoint(worldPos);

            if (col != null && col.gameObject != this.gameObject)
            {
                if (col.TryGetComponent(out UnitLoader aliado) &&
                    aliado.esAliado &&
                    aliado.datos.objeto != null &&
                    aliado.datos.objeto.icono != null)
                {
                    aliadosConObjeto.Add(aliado);
                }
            }
        }

        return aliadosConObjeto;
    }

    public void MarcarComoUsada()
    {
        yaActuo = true;

        // Actualizar la posición
        Vector3Int nuevaPos = Vector3Int.FloorToInt(transform.position);

        // Buscar la clave anterior
        var claves = GameManager.Instance.mapaUnidades
            .Where(kv => kv.Value == this.datos)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var viejaPos in claves)
        {
            GameManager.Instance.mapaUnidades.Remove(viejaPos);
        }

        // Añadir la nueva posición
        GameManager.Instance.mapaUnidades[nuevaPos] = this.datos;

        // Cambiar color de sprite para mostrar que ya actuó
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        if (!ChapterManager.Instance.chapterCompleted)
        {
            UnitLoader.RecalcularEfectosPasivos();
        }
    }

    public void ResetearUso()
    {
        yaActuo = false;

        // Restaurar color original
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }

    public void SubirNivel(Unidad unidad)
    {
        unidad.nivel++;
        unidad.MaxPV += SubidaDeEstadistica(unidad.probabilidadesCrecimiento[0]);
        unidad.poder += SubidaDeEstadistica(unidad.probabilidadesCrecimiento[1]);
        unidad.habilidad += SubidaDeEstadistica(unidad.probabilidadesCrecimiento[2]);
        unidad.velocidad += SubidaDeEstadistica(unidad.probabilidadesCrecimiento[3]);
        unidad.suerte += SubidaDeEstadistica(unidad.probabilidadesCrecimiento[4]);
        unidad.defensa += SubidaDeEstadistica(unidad.probabilidadesCrecimiento[5]);
        unidad.resistencia += SubidaDeEstadistica(unidad.probabilidadesCrecimiento[6]);
        if (barraVida != null)
        {
            barraVida.ActualizarPV();
        }
        Debug.Log($"{unidad.nombre} subió de nivel.");
    }

    public int SubidaDeEstadistica(int crecimiento)
    {
        int incremento = 0;
        // Si la probabilidad es más de 100, se otorga un punto de forma automática y se calcula el siguiente
        if (crecimiento > 100)
        {
            incremento++;
            int overflowChance = crecimiento - 100;
            if (Random.Range(1, 101) <= overflowChance)
            {
                incremento++;
            }
        }
        else
        {
            if (Random.Range(1, 101) <= crecimiento)
            {
                incremento++;
            }
        }
        return incremento;
    }

    public void GanarExp(Unidad unidad, int cantidad)
    {
        unidad.experiencia += cantidad;
        //Cuando la unidad tenga más de 100 de experiencia, esta se resta y se aumenta el nivel
        while (unidad.experiencia >= 100)
        {
            unidad.experiencia -= 100;
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.subirNivel);
            SubirNivel(unidad);
        }
    }

    public void Curar(int valor)
    {
        int nuevaVida = Mathf.Min(datos.PV + valor, datos.MaxPV); //Asegura que no supere el máximo
        datos.PV = nuevaVida;
        barraVida.ActualizarPV();
    }

    public void Potenciar(string stat, int valor, int duracion, int decremento)
    {
        if (buffs.ContainsKey(stat))
        {
            var (v, t, d) = buffs[stat];

            // No aplicar si el nuevo buff es igual o peor
            if (valor <= v && duracion <= t)
            {
                return;
            }

            // Remover el buff anterior antes de aplicar el nuevo
            RemoverBuff(stat, v);
        }

        buffs[stat] = (valor, duracion, decremento);
        AplicarBuff(stat, valor);
    }

    private void AplicarBuff(string stat, int valor)
    {
        switch (stat)
        {
            case "PV": datos.MaxPV += valor; 
                barraVida.ActualizarPV();
                break;
            case "Poder": datos.poder += valor; break;
            case "Habilidad": datos.habilidad += valor; break;
            case "Velocidad": datos.velocidad += valor; break;
            case "Suerte": datos.suerte += valor; break;
            case "Defensa": datos.defensa += valor; break;
            case "Resistencia": datos.resistencia += valor; break;
            case "Movimiento": datos.movimiento += valor; break;
        }
    }

    private void RemoverBuff(string stat, int valor)
    {
        AplicarBuff(stat, -valor); // Valor negativo
    }   

    public void ActualizarBuffsTemporales()
    {
        List<string> expirar = new();

        foreach (var stat in buffs.Keys.ToList())
        {
            var (valor, turnos, decremento) = buffs[stat];
            RemoverBuff(stat, valor); // Se quita el valor actual
            valor = Mathf.Max(0, valor - decremento);
            turnos--;
            if (turnos <= 0 || valor == 0)
            {
                expirar.Add(stat);
            }
            else
            {
                buffs[stat] = (valor, turnos, decremento);
                AplicarBuff(stat, valor); // Se aplica el nuevo valor
            }
        }
        foreach (string stat in expirar)
        {
            buffs.Remove(stat);
        }
    }

    public void PotenciarPermanentemente(string stat, int valor)
    {
        // Se aplica el aumento
        AplicarBuff(stat, valor);

        // Se acumula en el diccionario
        if (buffsPermanentes.ContainsKey(stat))
        {
            buffsPermanentes[stat] += valor;
        }
        else
        {
            buffsPermanentes[stat] = valor;
        }
    }

    public void AplicarEfectoArtefacto(Objeto artefacto)
    {
        if (artefacto.tipo == "Artefacto" && artefacto.uso == TipoUso.Pasivo)
        {
            // Obtener unidades dentro del rango actual
            List<UnitLoader> unidadesActuales = ObtenerUnidadesEnRango(artefacto.rango, artefacto.afectaAliados);

            if (!unidadesActuales.Contains(this) && artefacto.afectaAliados)
            {
                unidadesActuales.Add(this); // La unidad que lo lleva también se ve afectada
            }

            // Buscar unidades que ya tienen el efecto aplicado previamente
            List<UnitLoader> unidadesConEfecto = new List<UnitLoader>();
            
            foreach (var efecto in efectosArtefacto)
            {
                if (efecto.stat == artefacto.statAfectada && efecto.fuente == this)
                {
                    unidadesConEfecto.Add(efecto.objetivo);
                }
            }

            // Aplicar efecto a las nuevas unidades en rango que aún no lo tengan
            foreach (var unidad in unidadesActuales)
            {
                bool yaAplicado = efectosArtefacto.Any(e => e.fuente == this && e.stat == artefacto.statAfectada && e.objetivo == unidad);
                if (!yaAplicado)
                {
                    AplicarEfectoEstadistica(artefacto.statAfectada, artefacto.valor, unidad);
                    efectosArtefacto.Add(new EfectoArtefactoActivo
                    {
                        fuente = this,
                        stat = artefacto.statAfectada,
                        valor = artefacto.valor,
                        objetivo = unidad
                    });
                    unidad.efectosArtefactoEntrantes.Add(new EfectoArtefactoActivo 
                    { 
                        fuente = this,
                        stat = artefacto.statAfectada,
                        valor = artefacto.valor,
                        objetivo = unidad
                    });
                }
            }

            // Quitar efecto a las unidades que estaban en rango pero ya no lo están
            foreach (var unidad in unidadesConEfecto)
            {
                if (!unidadesActuales.Contains(unidad))
                {
                    AplicarEfectoEstadistica(artefacto.statAfectada, -artefacto.valor, unidad);
                    efectosArtefacto.RemoveAll(e => e.fuente == this && e.stat == artefacto.statAfectada && e.objetivo == unidad);
                }
            }
        }
    }

    public List<UnitLoader> ObtenerUnidadesEnRango(int rango, bool afectaAliados)
    {
        List<UnitLoader> unidadesEnRango = new List<UnitLoader>();
        Tilemap tilemap = FindObjectOfType<Tilemap>();

        for (int x = -rango; x <= rango; x++)
        {
            for (int y = -rango; y <= rango; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= rango)
                {
                    Vector3Int offset = new Vector3Int(x, y, 0);
                    Vector3Int pos = tilemap.WorldToCell(transform.position) + offset;
                    Vector3 worldPos = tilemap.GetCellCenterWorld(pos);

                    Collider2D col = Physics2D.OverlapPoint(worldPos);

                    if (col != null && col.gameObject != this.gameObject)
                    {
                        if (col.TryGetComponent(out UnitLoader unidad))
                        {
                            if (unidad.esAliado == afectaAliados)
                            {
                                unidadesEnRango.Add(unidad);
                            }
                        }
                    }
                }
            }
        }
        return unidadesEnRango;
    }

    public void AplicarEfectoEstadistica(string stat, int valor, UnitLoader unidad)
    {
        // Verificar a qué estadistica afecta y aplicar el valor
        switch (stat)
        {
            case "Poder": unidad.datos.poder += valor; break;
            case "Habilidad": unidad.datos.habilidad += valor; break;
            case "Velocidad": unidad.datos.velocidad += valor; break;
            case "Suerte": unidad.datos.suerte += valor; break;
            case "Defensa": unidad.datos.defensa += valor; break;
            case "Resistencia": unidad.datos.resistencia += valor; break;
            case "Movimiento": unidad.datos.movimiento += valor; break;
        }
    }

    public static void RecalcularEfectosPasivos()
    {
        var todasLasUnidades = FindObjectsOfType<UnitLoader>();

        // Limpiar todos los efectos actuales
        foreach (var unidad in todasLasUnidades)
        {
            unidad.LimpiarEfectosArtefacto();
        }

        foreach (var unidad in todasLasUnidades)
        {
            if (unidad.TieneObjetos() &&
                unidad.datos.objeto.tipo == "Artefacto" &&
                unidad.datos.objeto.uso == TipoUso.Pasivo)
            {
                unidad.AplicarEfectoArtefacto(unidad.datos.objeto);
            }
        }
    }

    public void LimpiarEfectosArtefacto()
    {
        var efectosAEliminar = efectosArtefacto.Where(e => e.fuente == this).ToList();

        foreach (var efecto in efectosAEliminar)
        {
            // Quitar efecto en el objetivo
            efecto.objetivo.AplicarEfectoEstadistica(efecto.stat, -efecto.valor, efecto.objetivo);

            // Limpiar de su lista de efectos entrantes
            efecto.objetivo.efectosArtefactoEntrantes.RemoveAll(e =>
                e.fuente == this && e.stat == efecto.stat && e.objetivo == efecto.objetivo);
        }

        efectosArtefacto.RemoveAll(e => e.fuente == this);
    }

    public void LimpiarEfectosEntrantes()
    {
        foreach (var efecto in efectosArtefactoEntrantes)
        {
            AplicarEfectoEstadistica(efecto.stat, -efecto.valor, this);
        }

        efectosArtefactoEntrantes.Clear();
    }

    public void LimpiarBonosTerreno()
    {
        datos.poder -= powerBonusActual;
        datos.habilidad -= skillBonusActual;
        datos.velocidad -= speedBonusActual;
        datos.suerte -= luckBonusActual;
        datos.defensa -= defenseBonusActual;
        datos.resistencia -= resistanceBonusActual;
        datos.movimiento -= movementBonusActual;
    }

    public void LimpiarPotenciadores()
    {
        foreach (var (stat, (valor, _, _)) in buffs.ToList())
        {
            RemoverBuff(stat, valor);
        }
        buffs.Clear();
    }

    public void LimpiarArtefactosPasivos()
    {
        LimpiarEfectosArtefacto();
    }

    public void Promocionar(Unidad unidad)
    {
        if (!unidad.bonosPromocion)
        {
            int indiceActual = GestorDeClases.clasesDisponibles.FindIndex(c => c.nombre == unidad.clase.nombre);

            // Se pasa a la siguiente clase de la lista, que es la versión 'avanzada' de la clase 'básica'
            if (indiceActual + 1 < GestorDeClases.clasesDisponibles.Count)
            {
                unidad.clase = GestorDeClases.clasesDisponibles[indiceActual + 1];
                unidad.MaxPV     += unidad.clase.bonusPV;
                unidad.PV = unidad.MaxPV;
                unidad.poder     += unidad.clase.bonusPoder;
                unidad.habilidad += unidad.clase.bonusHabilidad;
                unidad.velocidad += unidad.clase.bonusVelocidad;
                unidad.suerte    += unidad.clase.bonusSuerte;
                unidad.defensa   += unidad.clase.bonusDefensa;
                unidad.resistencia += unidad.clase.bonusResistencia;
                unidad.movimiento += unidad.clase.bonusMovimiento;

                unidad.bonosPromocion = true;
                Debug.Log("Clase promocionada a: " + unidad.clase.nombre);
            }
        }
    }
}
