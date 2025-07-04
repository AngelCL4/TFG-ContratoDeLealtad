using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;
using System.Linq;

public class ActionMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private Button[] botones;
    [SerializeField] private MovementRangeVisualizer visualizadorMovimiento;
    [SerializeField] private ObjectPanelUI objetoPanelUI;
    [SerializeField] private IntercambioPanelUI intercambioPanelUI;
    [SerializeField] private ConversationManager conversationManager;
    [SerializeField] public ObjectiveMenu objectiveMenu;
    private Button[] todosLosBotones;
    public bool MenuActivo => gameObject.activeSelf;
    private int indexActual = 0;
    private Color colorSeleccionado = Color.white;
    private Color colorNormal = new Color(0.7f, 0.7f, 0.7f, 1f);

    private enum Modo { Menu, IntercambioSeleccion, IntercambioPanel, CuracionSeleccion, CuracionPanel, AtaqueSeleccion, AtaquePanel }
    private Modo modoActual = Modo.Menu;
    private List<UnitLoader> aliadosAdyacentes;
    [SerializeField] private CurarPanelUI curarPanelUI;
    private List<UnitLoader> aliadosHeridos;
    private int indiceSeleccionado = 0;
    public bool fightConversationFinished = true;

    private Vector3 posicionAnterior;
    private PointerController pointer;
    private UnitLoader unidad;
    [SerializeField] private AtacarPanelUI atacarPanelUI;
    private List<UnitLoader> enemigosEnRango;
    private UnitLoader enemigoSeleccionado;
    private int experienciaPorCuracion = 15;
    public static ActionMenu Instance { get; private set; }

    private void Awake()
    {
        todosLosBotones = botones;
        Instance = this;
    }

    public void AbrirMenu(UnitLoader unidadActual, Vector3 posAnterior, PointerController controller)
    {
        unidad = unidadActual;
        posicionAnterior = posAnterior;
        pointer = controller;
        pointer.enabled = false;
        pointer.HabilitarInfoUnidad(true);
        gameObject.SetActive(true);
        menuUI.SetActive(true);
        modoActual = Modo.Menu;
        indexActual = 0;
        ActualizarSeleccionVisual();
        ActualizarOpciones();
    }

    public void CerrarMenu()
    {
        if (pointer != null)
            pointer.enabled = true;
            pointer.HabilitarInfoUnidad(true);
        gameObject.SetActive(false);
        pointer.MoverA(unidad.transform.position);
        pointer.unidadSeleccionada = null;
        visualizadorMovimiento.LimpiarCasillasAtaque();
    }

    private void Update()
    {
        if (!TurnManager.Instance.conversationFinished || !fightConversationFinished) return;
        if (modoActual == Modo.IntercambioSeleccion)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                // Cancelar intercambio
                modoActual = Modo.Menu;
                pointer.enabled = true;
                pointer.HabilitarInfoUnidad(true);
                AbrirMenu(unidad, posicionAnterior, pointer);
            }

            pointer.HandleMovement();

            if (Input.GetKeyDown(KeyCode.A))
            {
                Vector3Int posPuntero = FindObjectOfType<Tilemap>().WorldToCell(pointer.transform.position);

                // Evitar seleccionar la misma unidad
                Vector3Int posUnidad = FindObjectOfType<Tilemap>().WorldToCell(unidad.transform.position);
                if (posPuntero == posUnidad)
                {
                    AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                    return;
                }

                // Verificar si hay un aliado adyacente en esa posición
                foreach (var aliado in aliadosAdyacentes)
                {
                    Vector3Int posAliado = FindObjectOfType<Tilemap>().WorldToCell(aliado.transform.position);
                    if (posAliado == posPuntero)
                    {
                        indiceSeleccionado = aliadosAdyacentes.IndexOf(aliado);
                        AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                        MostrarPanelIntercambio();
                        return;
                    }
                }
                
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                Debug.Log("Esa unidad no es un aliado adyacente válido.");
            }

            return;
        }

        if (modoActual == Modo.IntercambioPanel)
        {
            return;
        }

        if (modoActual == Modo.CuracionSeleccion)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                modoActual = Modo.Menu;
                pointer.enabled = true;
                pointer.HabilitarInfoUnidad(true);
                curarPanelUI.Ocultar();
                AbrirMenu(unidad, posicionAnterior, pointer);
                return;
            }

            pointer.HandleMovement();

            Vector3Int posPuntero = FindObjectOfType<Tilemap>().WorldToCell(pointer.transform.position);
            foreach (var herido in aliadosHeridos)
            {
                Vector3Int posAliado = FindObjectOfType<Tilemap>().WorldToCell(herido.transform.position);
                if (posAliado == posPuntero)
                {
                    MostrarPanelCurar(herido);
                }
                else curarPanelUI.Ocultar();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                // Solo se puede curar a los aliados heridos adyacentes
                foreach (var herido in aliadosHeridos)
                {
                    Vector3Int posAliado = FindObjectOfType<Tilemap>().WorldToCell(herido.transform.position);
                    if (posAliado == posPuntero)
                    {
                        AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.curar);
                        RealizarCuracion(herido);
                        return;
                    }
                }
            }

            return;
        }

        if (modoActual == Modo.AtaqueSeleccion)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                unidad.barraVida.LimpiarDanioProyectado();
                foreach (var enemigo in enemigosEnRango)
                {
                    enemigo.barraVida.LimpiarDanioProyectado();
                }
                modoActual = Modo.Menu;
                pointer.enabled = true;
                pointer.HabilitarInfoUnidad(true);
                atacarPanelUI.Ocultar();
                AbrirMenu(unidad, posicionAnterior, pointer);
                return;
            }

            pointer.HandleMovement();

            Vector3Int posPuntero = FindObjectOfType<Tilemap>().WorldToCell(pointer.transform.position);

            foreach (var enemigo in enemigosEnRango)
            {
                enemigo.barraVida.LimpiarDanioProyectado();
            }
            unidad.barraVida.LimpiarDanioProyectado();

            foreach (var enemigo in enemigosEnRango)
            {
                Vector3Int posEnemigo = FindObjectOfType<Tilemap>().WorldToCell(enemigo.transform.position);
                if (posPuntero == posEnemigo)
                {
                    MostrarPanelAtaque(enemigo);
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                // Solo enemigos válidos en rango
                foreach (var enemigo in enemigosEnRango)
                {
                    Vector3Int posEnemigo = FindObjectOfType<Tilemap>().WorldToCell(enemigo.transform.position);
                    Debug.Log(posPuntero);
                    Debug.Log(posEnemigo);
                    if (posPuntero == posEnemigo)
                    {
                        enemigoSeleccionado = enemigo;
                        modoActual = Modo.AtaquePanel;
                        AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                        EjecutarCombate(unidad, enemigoSeleccionado);
                    }
                }
            }

            return;
        }

        if (modoActual == Modo.Menu)
        {
            // Navegación entre botones con las flechas
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                indexActual = (indexActual - 1 + botones.Length) % botones.Length;
                ActualizarSeleccionVisual();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                indexActual = (indexActual + 1) % botones.Length;
                ActualizarSeleccionVisual();
            }

            // Seleccionar opción actual con A
            if (Input.GetKeyDown(KeyCode.A))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                EjecutarOpcion(indexActual);
            }

            // Cancelar acción con S
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                unidad.transform.position = posicionAnterior;
                pointer.CancelarAccion();
                CerrarMenu();
            }
        }
    }

    public void EjecutarCombate(UnitLoader unidad, UnitLoader enemigoSeleccionado)
    {
        StartCoroutine(AtacarYPostProceso(unidad, enemigoSeleccionado));
    }

    private IEnumerator AtacarYPostProceso(UnitLoader unidad, UnitLoader enemigoSeleccionado)
    {
        // Se llama a la corutina de combate
        yield return StartCoroutine(RealizarCombate(unidad, enemigoSeleccionado));

        // Marcar unidad como usada, cerrar todos los menús y actualizar rangos de enemigos si se estaban mostrando
        if (unidad != null)
        {
            unidad.MarcarComoUsada();
            TurnManager.Instance.StartCoroutine(TurnManager.Instance.NotificarUnidadTerminada(unidad));
            if (pointer.mostrarRangosEnemigos)
            {
                pointer.visualizadorMovimiento.LimpiarRangosDeEnemigos();
                pointer.MostrarRangosAtaqueEnemigos();
            }
            pointer.ActualizarRangoEnemigo();
            pointer.unidadSeleccionada = null;
            pointer.MoverA(unidad.transform.position);
        }
        pointer.enabled = true;
        atacarPanelUI.Ocultar();
        CerrarMenu();
    }

    // Función de combate entre dos unidades, atacante (unidad del jugador) y atacado/defensor (unidad del enemigo)
    private IEnumerator RealizarCombate(UnitLoader atacante, UnitLoader atacado)
    {
        atacante.barraVida.LimpiarDanioProyectado();
        atacado.barraVida.LimpiarDanioProyectado();
        // Si el jefe es atacado, se cambia la música y se comprueba si hay conversación de combate
        if (atacado.datos.estado == "Jefe")
        {
            if (GameManager.Instance.bossMusicSounding == false)
            {
                var chapterData = GameManager.Instance.chapterDataJuego.chapters.FirstOrDefault(c => c.chapterName == ChapterManager.Instance.currentChapter);
                if (!string.IsNullOrEmpty(chapterData.bossMusic))
                {
                    MusicManager.Instance.PlayMusic(chapterData.bossMusic);
                    GameManager.Instance.bossMusicSounding = true;
                }
            }
            fightConversationFinished = false;
            string chapterNumber = ChapterManager.Instance.currentChapter;
            ConversationManager.Instance.StartFightConversation(atacante.datos.nombre, chapterNumber, () => fightConversationFinished = true);
            yield return new WaitUntil(() => fightConversationFinished);
        }

        int distancia = CalcularDistancia(atacante.transform.position, atacado.transform.position);
        bool enemigoPuedeContraatacar = distancia >= atacado.datos.clase.rangoAtaqueMinimo && distancia <= atacado.datos.clase.rangoAtaqueMaximo;

        // Datos del atacante
        int poderAtacante = atacante.datos.poder;
        string tipoDanoAtacante = atacante.datos.clase.tipoDano;
        int defensaObjetivo = tipoDanoAtacante == "Fisico" ? atacado.datos.defensa : atacado.datos.resistencia;
        int dañoAtacante = Mathf.Max(0, poderAtacante - defensaObjetivo);

        bool esCriticoAtacante = Random.Range(1, 100) <= Mathf.Max(0, atacante.datos.habilidad - atacado.datos.suerte);
        bool esCriticoAtacanteDoble = Random.Range(1, 100) <= Mathf.Max(0, atacante.datos.habilidad - atacado.datos.suerte);
        int velocidadAtacante = atacante.datos.velocidad;

        // Datos del defensor
        int poderDefensor = atacado.datos.poder;
        string tipoDanoDefensor = atacado.datos.clase.tipoDano;
        int defensaDelAtacante = tipoDanoDefensor == "Fisico" ? atacante.datos.defensa : atacante.datos.resistencia;
        int dañoDefensor = Mathf.Max(0, poderDefensor - defensaDelAtacante);

        bool esCriticoDefensor = Random.Range(0, 100) < Mathf.Max(0, atacado.datos.habilidad - atacante.datos.suerte);
        bool esCriticoDefensorDoble = Random.Range(0, 100) < Mathf.Max(0, atacado.datos.habilidad - atacante.datos.suerte);
        int velocidadDefensor = atacado.datos.velocidad;

        bool dobleAtacante = velocidadAtacante > velocidadDefensor;
        bool dobleDefensor = velocidadDefensor > velocidadAtacante;

        // Combate
        Debug.Log("Inicio de combate");

        // Atacante ataca
        int dañoTotalAtacante = esCriticoAtacante ? dañoAtacante * 2 : dañoAtacante;
        atacado.datos.PV -= dañoTotalAtacante;
        if (esCriticoAtacante){
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpeCrítico);
        }
        else AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpe);
        Debug.Log($"{atacante.datos.nombre} ataca a {atacado.datos.nombre} por {dañoTotalAtacante} de daño" + (esCriticoAtacante ? " (CRÍTICO!)" : ""));
        atacado.barraVida.ActualizarPV();
        // Esperar 1 segundo
        yield return new WaitForSeconds(1f);

        // Verificar si el enemigo sobrevivió para contraatacar
        if (atacado.datos.PV > 0 && enemigoPuedeContraatacar)
        {
            int dañoTotalDefensor = esCriticoDefensor ? dañoDefensor * 2 : dañoDefensor;
            atacante.datos.PV -= dañoTotalDefensor;
            if (esCriticoDefensor){
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpeCrítico);
            }
            else AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpe);
            Debug.Log($"{atacado.datos.nombre} contraataca a {atacante.datos.nombre} por {dañoTotalDefensor} de daño" + (esCriticoDefensor ? " (CRÍTICO!)" : ""));
            atacante.barraVida.ActualizarPV();
            // Esperar 1 segundo
            yield return new WaitForSeconds(1f);
        }

        // Doble ataque del atacante
        if (dobleAtacante && atacado.datos.PV > 0 && atacante.datos.PV > 0)
        {
            int dañoTotalAtacanteDoble = esCriticoAtacanteDoble ? dañoAtacante * 2 : dañoAtacante;
            atacado.datos.PV -= dañoTotalAtacanteDoble;
            if (esCriticoAtacanteDoble){
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpeCrítico);
            }
            else AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpe);
            Debug.Log($"{atacante.datos.nombre} realiza un segundo ataque por {dañoTotalAtacanteDoble} de daño" + (esCriticoDefensor ? " (CRÍTICO!)" : ""));
            atacado.barraVida.ActualizarPV();
            // Esperar 1 segundo
            yield return new WaitForSeconds(1f);
        }

        // Doble ataque del defensor
        if (dobleDefensor && atacante.datos.PV > 0 && atacado.datos.PV > 0 && enemigoPuedeContraatacar)
        {
            int dañoTotalDefensorDoble = esCriticoDefensorDoble ? dañoDefensor * 2 : dañoDefensor;
            atacante.datos.PV -= dañoTotalDefensorDoble;
            if (esCriticoDefensorDoble){
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpeCrítico);
            }
            else AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.golpe);
            Debug.Log($"{atacado.datos.nombre} realiza un segundo contraataque por {dañoTotalDefensorDoble} de daño" + (esCriticoDefensor ? " (CRÍTICO!)" : ""));
            atacante.barraVida.ActualizarPV();
            // Esperar 1 segundo
            yield return new WaitForSeconds(1f);
        }

        // Se limita la vida para no ir por debajo de cero
        atacante.datos.PV = Mathf.Max(0, atacante.datos.PV);
        atacado.datos.PV = Mathf.Max(0, atacado.datos.PV);

        // Si muere el atacado (unidad enemiga), se comprueba el objetivo del nivel y si se ha cumplido
        if (atacado.datos.PV == 0)
        {
            if (atacado.datos == pointer.enemigoSeleccionado){
                visualizadorMovimiento.LimpiarRangoDeEnemigo(pointer.enemigoSeleccionado);
            }
            yield return StartCoroutine(EsperarRetiradaYFinalizar(atacado.datos.nombre));
            TurnManager.Instance.unidadesEnemigas.Remove(atacado);
            Destroy(atacado.gameObject);
            if (objectiveMenu.data.victoryCondition == "Comandante")
            {
                yield return null;
                ComprobarJefe();
            }
            if (objectiveMenu.data.victoryCondition == "Derrotar")
            {
                yield return null;
                ComprobarVictoria();
            }
        }

        // Si muere el atacante (unidad aliada), se limpian sus bonos de estadísticas y se guarda su estado
        if (atacante.datos.PV == 0)
        {
            yield return StartCoroutine(EsperarRetiradaYFinalizar(atacante.datos.nombre));
            atacante.LimpiarBonosTerreno();
            atacante.LimpiarPotenciadores();
            atacante.LimpiarArtefactosPasivos();
            atacante.LimpiarEfectosEntrantes();
            atacante.datos.PV = atacante.datos.MaxPV;
            TurnManager.Instance.unidadesAGuardar.Add(atacante.datos);
            TurnManager.Instance.unidadesJugador.Remove(atacante);
            Destroy(atacante.gameObject);
            TurnManager.Instance.ComprobarDerrota();  
            yield break;
        }

        if (atacante != null)
        {
            // Solo ejecutar si el atacante sigue vivo, por lo que gana experiencia
            int leveldiff = atacado.datos.nivel - atacante.datos.nivel;
            if (leveldiff < 0) leveldiff = 0;
            int experienciaGanada = (atacado.datos.PV == 0) ? 30 + (5 * leveldiff) : 15 + (5 * leveldiff);
            atacante.GanarExp(atacante.datos, experienciaGanada);

            foreach (var aliada in atacante.UnidadesAliadasAdyacentes())
            {
                SupportManager.Instance.AñadirAfecto(atacante.datos.nombre, aliada.datos.nombre, 2);
            }
        }

        yield return null;
    }

    // Corrutina cuando se derrota a una unidad aliada
    public IEnumerator EsperarRetiradaYFinalizar(string nombre)
    {
        bool finalizado = false;
        conversationManager.StartRetreatQuote(nombre, () => finalizado = true);

        // Se espera a que finalice el diálogo de retirada
        yield return new WaitUntil(() => finalizado);

        var chapterData = GameManager.Instance.chapterDataJuego.chapters.FirstOrDefault(c => c.chapterName == ChapterManager.Instance.currentChapter);

        // Se restaura la música que estuviera sonando
        if (GameManager.Instance.bossMusicSounding == false)
        {
            if (!string.IsNullOrEmpty(chapterData.battleMusic))
            {
                MusicManager.Instance.PlayMusic(chapterData.battleMusic);
            }
        }
        else 
        {
            if (!string.IsNullOrEmpty(chapterData.bossMusic))
            {
                MusicManager.Instance.PlayMusic(chapterData.bossMusic);
            }
        }
    }

    private void MoverPuntero(Vector3Int direccion)
    {
        pointer.MovePointer(direccion);
    }

    private void ActualizarSeleccionVisual()
    {
        for (int i = 0; i < botones.Length; i++)
        {
            TextMeshProUGUI texto = botones[i].GetComponentInChildren<TextMeshProUGUI>();
            texto.color = (i == indexActual) ? colorSeleccionado : colorNormal;
        }
    }

    private void EjecutarOpcion(int indice)
    {
        string nombre = botones[indice].name;

        switch (nombre)
        {
            case "Atacar":
                OnAtacar();
                break;
            case "Curar":
                OnCurar();
                break;
            case "Objeto":
                OnObjeto();
                break;
            case "Intercambiar":
                OnIntercambiar();
                break;
            case "Esperar":
                StartCoroutine(OnEsperar());
                break;
            default:
                break;
        }
    }

    public void OnAtacar()
    {
        Debug.Log("Acción: Atacar");
        enemigosEnRango = unidad.EnemigosEnRango();

        if (enemigosEnRango.Count == 0)
        {
            Debug.Log("No hay enemigos en rango.");
            return;
        }

        modoActual = Modo.AtaqueSeleccion;
        pointer.enabled = true;
        pointer.HabilitarInfoUnidad(false);
        menuUI.SetActive(false);
    }

    int CalcularDistancia(Vector3 pos1, Vector3 pos2)
    {
        Vector3Int cell1 = FindObjectOfType<Tilemap>().WorldToCell(pos1);
        Vector3Int cell2 = FindObjectOfType<Tilemap>().WorldToCell(pos2);
        return Mathf.Abs(cell1.x - cell2.x) + Mathf.Abs(cell1.y - cell2.y);
    }

    private void MostrarPanelAtaque(UnitLoader enemigo)
    {
        int distancia = CalcularDistancia(unidad.transform.position, enemigo.transform.position);
        bool enemigoPuedeContraatacar = distancia >= enemigo.datos.clase.rangoAtaqueMinimo && distancia <= enemigo.datos.clase.rangoAtaqueMaximo;

        // Atacante
        int poderAtacante = unidad.datos.poder;
        string tipoDanoAtacante = unidad.datos.clase.tipoDano;
        int defensaObjetivo = tipoDanoAtacante == "Fisico" ? enemigo.datos.defensa : enemigo.datos.resistencia;
        int dañoAtacante = Mathf.Max(0, poderAtacante - defensaObjetivo);

        int criticoAtacante = Mathf.Max(0, unidad.datos.habilidad - enemigo.datos.suerte);
        int velocidadAtacante = unidad.datos.velocidad;

        // Defensor
        int poderDefensor = enemigo.datos.poder;
        string tipoDanoDefensor = enemigo.datos.clase.tipoDano;
        int defensaDelAtacante = tipoDanoDefensor == "Fisico" ? unidad.datos.defensa : unidad.datos.resistencia;
        int dañoDefensor = Mathf.Max(0, poderDefensor - defensaDelAtacante);

        int criticoDefensor = Mathf.Max(0, enemigo.datos.habilidad - unidad.datos.suerte);
        int velocidadDefensor = enemigo.datos.velocidad;

        bool dobleAtacante = velocidadAtacante > velocidadDefensor;
        bool dobleDefensor = velocidadDefensor > velocidadAtacante;

        atacarPanelUI.Mostrar(
            unidad.datos.nombre, unidad.datos.PV, dañoAtacante, criticoAtacante, dobleAtacante,
            enemigo.datos.nombre, enemigo.datos.PV, dañoDefensor, criticoDefensor, dobleDefensor, enemigoPuedeContraatacar
        );

        int dañoTotalAlEnemigo = dobleAtacante ? dañoAtacante * 2 : dañoAtacante;
        enemigo.barraVida.MostrarDanioProyectado(dañoTotalAlEnemigo);

        if (enemigoPuedeContraatacar)
        {
            int dañoTotalAlJugador = dobleDefensor ? dañoDefensor * 2 : dañoDefensor;
            unidad.barraVida.MostrarDanioProyectado(dañoTotalAlJugador);
        }
    }

    public void OnCurar()
    {
        Debug.Log("Acción: Curar");
        modoActual = Modo.CuracionSeleccion;

        aliadosHeridos = unidad.UnidadesAliadasAdyacentesHeridas();
        if (aliadosHeridos.Count == 0)
        {
            Debug.Log("No hay aliados heridos adyacentes.");
            return;
        }

        indexActual = 0;
        pointer.enabled = true;
        pointer.HabilitarInfoUnidad(false);
        menuUI.SetActive(false);
    }

    private void MostrarPanelCurar(UnitLoader aliadoHerido)
    {
        curarPanelUI.Mostrar(aliadoHerido);
    }

    private void RealizarCuracion(UnitLoader objetivo)
    {
        int poder = unidad.datos.poder;
        int nuevaVida = Mathf.Min(objetivo.datos.PV + poder, objetivo.datos.MaxPV);
        int cantidadCurada = nuevaVida - objetivo.datos.PV;

        objetivo.datos.PV = nuevaVida;

        Debug.Log($"Se curo a {objetivo.datos.nombre} por {cantidadCurada} puntos de vida.");
        objetivo.barraVida.ActualizarPV();
        // Ganancia de experiencia y afecto
        unidad.GanarExp(unidad.datos, experienciaPorCuracion);
        SupportManager.Instance.AñadirAfecto(unidad.datos.nombre, objetivo.datos.nombre, 2);
        // Funciones de final de turno de la unidad
        unidad.MarcarComoUsada();
        TurnManager.Instance.StartCoroutine(TurnManager.Instance.NotificarUnidadTerminada(unidad));
        if (pointer.mostrarRangosEnemigos)
        {
            pointer.visualizadorMovimiento.LimpiarRangosDeEnemigos();
            pointer.MostrarRangosAtaqueEnemigos();
        }
        pointer.ActualizarRangoEnemigo();
        pointer.MoverA(unidad.transform.position);
        pointer.unidadSeleccionada = null;
        pointer.enabled = true;
        curarPanelUI.Ocultar();
        CerrarMenu();
    }

    public void OnObjeto()
    {
        Debug.Log("Acción: Objeto");

        Objeto objeto = unidad.datos.objeto;
        bool objetoFueUsado = false;

        objetoPanelUI.Mostrar(unidad, objeto, () =>
        {
            if (!objetoFueUsado)
            {
                gameObject.SetActive(true); // Solo se reabre el panel si no se uso el objeto
            }
        },
        (objetoUsado) =>
        {
            // Al usar el objeto
            Debug.Log($"Usando objeto: {objetoUsado.nombre}");
            // Funciones de final de turno de la unidad
            unidad.MarcarComoUsada();
            TurnManager.Instance.StartCoroutine(TurnManager.Instance.NotificarUnidadTerminada(unidad));
            if (pointer.mostrarRangosEnemigos)
            {
                pointer.visualizadorMovimiento.LimpiarRangosDeEnemigos();
                pointer.MostrarRangosAtaqueEnemigos();
            }
            pointer.ActualizarRangoEnemigo();
            pointer.unidadSeleccionada = null;

            objetoFueUsado = true;
            CerrarMenu();
        });

        gameObject.SetActive(false);
    }

    public void OnIntercambiar()
    {
        Debug.Log("Acción: Intercambiar");
        modoActual = Modo.IntercambioSeleccion;
        // Si la unidad tiene un objeto, cuenta cualquier aliado adyacente, si no, cuentan solo los aliados adyacentes con objeto
        if (unidad.TieneObjetos()){
            aliadosAdyacentes = unidad.UnidadesAliadasAdyacentes();
        }
        else {
            aliadosAdyacentes = unidad.UnidadesAliadasAdyacentesConObjeto();
        }
        if (aliadosAdyacentes.Count == 0)
        {
            Debug.Log("No hay aliados para intercambiar.");
            return;
        }
        indexActual = 0;
        pointer.enabled = true;
        pointer.HabilitarInfoUnidad(false);
        menuUI.SetActive(false);
    }

    private void MostrarPanelIntercambio()
    {
        modoActual = Modo.IntercambioPanel;
        pointer.enabled = false;

        Objeto obj1 = unidad.datos.objeto;
        Objeto obj2 = aliadosAdyacentes[indiceSeleccionado].datos.objeto;

        intercambioPanelUI.Mostrar(obj1, obj2, 
            () => // Cancelar intercambio
            {
                modoActual = Modo.IntercambioSeleccion;
            },
            () => // Confirmar intercambio
            {
                unidad.datos.objeto = obj2;
                aliadosAdyacentes[indiceSeleccionado].datos.objeto = obj1;
                // Funciones de final de turno de la unidad
                unidad.MarcarComoUsada();
                TurnManager.Instance.StartCoroutine(TurnManager.Instance.NotificarUnidadTerminada(unidad));
                if (pointer.mostrarRangosEnemigos)
                {
                    pointer.visualizadorMovimiento.LimpiarRangosDeEnemigos();
                    pointer.MostrarRangosAtaqueEnemigos();
                }
                pointer.ActualizarRangoEnemigo();
                pointer.MoverA(unidad.transform.position);
                pointer.unidadSeleccionada = null;
                pointer.enabled = true;
                intercambioPanelUI.Cerrar();
                CerrarMenu();
            });
    }

    public IEnumerator OnEsperar()
    {
        Debug.Log("Acción: Esperar / Confirmar movimiento");
        // Se comprueba la visita en la casilla
        yield return StartCoroutine(ComprobarVisita());
        // Funciones de final de turno de la unidad
        unidad.MarcarComoUsada();
        TurnManager.Instance.StartCoroutine(TurnManager.Instance.NotificarUnidadTerminada(unidad));
        if (pointer.mostrarRangosEnemigos)
        {
            pointer.visualizadorMovimiento.LimpiarRangosDeEnemigos();
            pointer.MostrarRangosAtaqueEnemigos();
        }
        pointer.ActualizarRangoEnemigo();
        pointer.unidadSeleccionada = null;
        CerrarMenu();
        // Si el objetivo es escapar y está en la casilla de escape, la unidad limpia sus bonos de estadísticas, se guarda su estado y desaparece del mapa
        if(objectiveMenu.data.victoryCondition == "Escapar")
        {
            if (unidad.transform.position.x == objectiveMenu.data.x && unidad.transform.position.y == objectiveMenu.data.y)
            {
                unidad.LimpiarBonosTerreno();
                unidad.LimpiarPotenciadores();
                unidad.LimpiarArtefactosPasivos();
                unidad.LimpiarEfectosEntrantes();
                unidad.datos.PV = unidad.datos.MaxPV;
                TurnManager.Instance.unidadesAGuardar.Add(unidad.datos);
                TurnManager.Instance.unidadesJugador.Remove(unidad);
                ComprobarEscape();
                Destroy(unidad.gameObject);              
            }
        }
    }

    public IEnumerator ComprobarVisita()
    {
        Debug.Log(ConversationManager.Instance == null);
        if (ConversationManager.Instance == null) yield break;

        float x = unidad.transform.position.x; 
        float y = unidad.transform.position.y;

        VisitData visita = ConversationManager.Instance.BuscarVisitaEnCoordenada(x, y);

        if (visita != null && !visita.visited)
        {
            Debug.Log($"Visitando edificio en ({x},{y})");

            conversationManager.VisitConversationActive = true;

            // Iniciar la conversación asociada
            conversationManager.StartConversation(visita.conversation, () =>
            {
                // Otorgar recompensa
                DarRecompensa(visita.reward);

                // Marcar como visitada para evitar repetición
                visita.visited = true;
                conversationManager.VisitConversationActive = false;
            });

            yield return new WaitUntil(() => !conversationManager.VisitConversationActive);
        }
    }

    private void DarRecompensa(Objeto recompensa)
    {
        if (string.IsNullOrEmpty(recompensa.nombre)) return;

        // Si la recompensa es un contrato, se suma en la variable global
        if (recompensa.nombre == "Contrato")
        {
            GameManager.Instance.chapterDataJuego.contratos++;
        }
        // Si no, la recompensa (objeto) se equipa en la unidad si no tiene objeto, o se almacena si lo tiene
        else
        {
            if (unidad.TieneObjetos())
            {
                AlmacenObjetos.Instance.AñadirObjeto(recompensa);
            }
            else 
            {
                unidad.datos.objeto = recompensa;
                unidad.datos.objeto.icono = Resources.Load<Sprite>(recompensa.spritePath);
            }
        }
    }

    private void ActualizarOpciones()
    {
        // Si la unidad no tiene enemigos en rango, no se muestra la opción de atacar
        bool puedeAtacar = unidad.EnemigosEnRango().Count > 0;
        // Si la unidad no tiene objeto equipado, no se muestra la opción de objeto
        bool tieneObjeto = unidad.TieneObjetos();
        // Si la unidad no tiene unidades aliadas adyacentes con objeto, o no tiene objeto ni unidades aliadas adyacentes, no se muestra la opción de intercambiar
        bool puedeIntercambiar = unidad.UnidadesAliadasAdyacentesConObjeto().Count > 0;
        bool puedeIntercambiarObjeto = unidad.TieneObjetos() && unidad.UnidadesAliadasAdyacentes().Count > 0;
        // Si la unidad no tiene unidades aliadas adyacentes heridas y no es de la clase indicada, no se muestra la opción de curar
        bool puedeCurar = unidad.UnidadesAliadasAdyacentesHeridas().Count > 0;

        foreach (var boton in todosLosBotones)
        {
            switch (boton.name)
            {
                case "Atacar":
                    boton.gameObject.SetActive(puedeAtacar);
                    break;
                case "Curar":
                    boton.gameObject.SetActive(puedeCurar);
                    break;
                case "Objeto":
                    boton.gameObject.SetActive(tieneObjeto);
                    break;
                case "Intercambiar":
                    bool mostrarBoton = puedeIntercambiar || puedeIntercambiarObjeto;
                    boton.gameObject.SetActive(mostrarBoton);
                    break;
                case "Esperar":
                    boton.gameObject.SetActive(true);
                    break;
            }
        }
        
        var botonesVisibles = new List<Button>();
        foreach (var boton in todosLosBotones)
        {
            if (boton.gameObject.activeSelf)
                botonesVisibles.Add(boton);
        }
        botones = botonesVisibles.ToArray();

        indexActual = 0;
        ActualizarSeleccionVisual();
    }

    // Objetivo 'Derrotar'
    public void ComprobarVictoria()
    {

        // Buscar todas las unidades en la escena
        UnitLoader[] unidades = FindObjectsOfType<UnitLoader>();

        // Comprobar si queda algún enemigo o jefe
        bool quedanEnemigos = unidades.Any(unit => !unit.esAliado);
        Debug.Log(quedanEnemigos);
        if (!quedanEnemigos)
        {
            ChapterManager.Instance.chapterCompleted = true;
        }
    }

    // Ojbetivo 'Comandante'
    public void ComprobarJefe()
    {
        // Buscar todas las unidades en la escena
        UnitLoader[] unidades = FindObjectsOfType<UnitLoader>();

        // Comprobar si queda algún "Jefe"
        bool quedaJefe = unidades.Any(unit => unit.datos.estado == "Jefe");

        if (!quedaJefe)
        {
            // Si no queda ningún "Jefe", completar el capítulo
            ChapterManager.Instance.chapterCompleted = true;
        }
    }

    // Objetivo 'Escapar'
    private void ComprobarEscape()
    {
        bool quedanAliados = TurnManager.Instance.unidadesJugador.Count != 0;
        Debug.Log("Quedan aliados: " + quedanAliados);
        if (!quedanAliados)
        {
            ChapterManager.Instance.chapterCompleted = true;
        }
    }
}
