using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[System.Serializable]
public class ChapterStatus
{
    public string chapterName;
    public bool completed;
    public string background;
    public string daytime;
    public string battleMusic;
    public string bossMusic;
    public string campMusic;
    public List<string> unitpromoted;
}

[System.Serializable]
public class ChapterData
{
    public int contratos;
    public int entrenar;
    public List<ChapterStatus> chapters = new();
}

[System.Serializable]
public class UnlockedChapter
{
    public string chapterName;
    public string chapterTitle;
    public string description;
    public bool estaDesbloqueado;
}

[System.Serializable]
public class UnlockedChaptersData
{
    public List<UnlockedChapter> chapters = new();
}

[System.Serializable]
public class RefuerzoEnemigo
{
    public Unidad unidad;
    public int turno;
    public float x;
    public float y;
}

[System.Serializable]
public class RefuerzoWrapper
{
    public List<RefuerzoEnemigo> refuerzos;
}

public class ChapterManager : MonoBehaviour
{

    [SerializeField] private MapLoader mapLoader;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ConversationManager conversationManager;
    [SerializeField] private GameObject pointer;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UnitSpawner unitSpawner;
    [SerializeField] private CuadriculaAccesibilidad cuadricula;
    [SerializeField] private TimeTilemap time;
    [SerializeField] private ContractMenu contractMenu;
    public string currentChapter;
    public string lastMainChapter;
    public static ChapterManager Instance;
    public bool chapterCompleted;
    bool contratoCerrado = false;
    public List<RefuerzoEnemigo> refuerzos = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentChapter = GameManager.Instance.currentChapter;
        chapterCompleted = false;

        if (pointer != null) pointer.SetActive(false);
        if (cameraController != null) cameraController.enabled = false;

        StartCoroutine(LoadChapter());
    }

    private IEnumerator LoadChapter()
    {
        int chapterNumber = ExtraerNumeroDeCapitulo(currentChapter);
        // Iniciar Preludio
        bool preludioFinished = false;
        dialogueManager.StartDialogue(currentChapter + "Preludio", () => preludioFinished = true);
        yield return new WaitUntil(() => preludioFinished);
        // Cargar eventos, visitas y refuerzos
        conversationManager.LoadChapterEvents(currentChapter + "Events");
        conversationManager.chapterVisits = conversationManager.LoadChapterVisits(currentChapter + "Visits");
        LoadReinforcements(currentChapter);

        // Iniciar Conversación
        bool conversationFinished = false;
        conversationManager.StartConversation(currentChapter + "Conversation", () => conversationFinished = true);
        yield return new WaitUntil(() => conversationFinished);
        Debug.Log("Conversacion finalizada");

        // Buscar el capítulo por nombre
        var chapterData = GameManager.Instance.chapterDataJuego.chapters
            .FirstOrDefault(c => c.chapterName == currentChapter);

        // Si es el primer capítulo activar el menú de contratos
        if (currentChapter == "Chapter1")
        {
            contractMenu.gameObject.SetActive(true);
            contractMenu.OnCerrarContrato = () => contratoCerrado = true;
            yield return new WaitUntil(() => contratoCerrado);
        }

        // Cargar mapa
        mapLoader.LoadMapFromJson(currentChapter + "Map");
        
        // Generar cuadrícula y actualizar la opacidad
        cuadricula.GenerarCuadricula();
        cuadricula.ActualizarOpacidad(cuadricula.opacidadGuardada);

        // Generar la cuadrícula de tiempo del día, o limpiarla
        if (chapterData.daytime == "Night")
        {
            time.GenerarCuadriculaNoche();
        }
        else if (chapterData.daytime == "Evening")
        {
            time.GenerarCuadriculaTarde();
        }
        else
        {
            time.timeTilemap.ClearAllTiles();
        }

        if (pointer != null) pointer.SetActive(true);
        if (cameraController != null) cameraController.enabled = true;

        GameManager.Instance.ActualizarNivelMedioEjercito();
        
        // Spawneo de unidades e inicio de la jugabilidad
        unitSpawner.SpawnAliados();
        unitSpawner.SpawnEnemigos();
        StartCoroutine(TurnManager.Instance.IniciarFaseJugador());

        // Activación de la música de batalla
        if (!string.IsNullOrEmpty(chapterData.battleMusic))
        {
            MusicManager.Instance.PlayMusic(chapterData.battleMusic);
        }

        // Esperar a que el capítulo se complete
        yield return new WaitUntil(() => chapterCompleted);


        // Marcar capítulo como completo
        if (chapterData != null)
        {
            chapterData.completed = true;
            // Si hay unidades a las que promocionar, se promocionan con UnitLoader temporal
            if (chapterData.unitpromoted != null && chapterData.unitpromoted.Count > 0)
            {
                foreach (var unitName in chapterData.unitpromoted)
                {
                    Unidad promociona = GameManager.Instance.datosJuego.unidades.FirstOrDefault(u => u.nombre == unitName);
                    if (promociona != null)
                    {
                        var tempGameObject = new GameObject("TempUnitLoader");
                        var tempUnitLoader = tempGameObject.AddComponent<UnitLoader>();
                        tempUnitLoader.ConfigurarUnidad(promociona, true);
                        tempUnitLoader.Promocionar(promociona);
                        GameObject.Destroy(tempGameObject);
                    }
                    else
                    {
                        Debug.LogWarning($"Unidad no encontrada para promocionar: {unitName}");
                    }
                }
            }
            
            // Se cambian las variables globales a las indicadas en los datos del capítulo
            GameManager.Instance.fondoCampamento = chapterData.background;
            GameManager.Instance.musicaCampamento = chapterData.campMusic;
        }
        else
        {
            Debug.LogWarning($"No se encontró el capítulo en chapterDataJuego con nombre: {currentChapter}");
        }

        // Si es un capítulo de la historia principal
        if (chapterNumber != -1)
        {
            // Se desbloquea el siguiente capítulo si no es el último capítulo
            if (chapterNumber < 15)
            {
                GameManager.Instance.DesbloquearCapitulo(chapterNumber);
            }

            // Se gana un contrato si es menor del capítulo 8
            if (chapterNumber < 8)
            {
                GameManager.Instance.chapterDataJuego.contratos++;
            }

            lastMainChapter = currentChapter;
            GameManager.Instance.lastMainChapter = lastMainChapter;
        }

        GameManager.Instance.GuardarDatosPersistentes();

        List<string> nombresUnidades = TurnManager.Instance.unidadesJugador.Select(u => u.datos.nombre).ToList();
        SupportManager.Instance.AfectoAlFinalizarNivel(nombresUnidades);

        // Limpiar cambios de estadísticas y curar a todas las unidades
        foreach (var unidad in TurnManager.Instance.unidadesJugador)
        {
            unidad.datos.PV = unidad.datos.MaxPV;
            unidad.LimpiarBonosTerreno();
            unidad.LimpiarPotenciadores();
            unidad.LimpiarArtefactosPasivos();
            unidad.LimpiarEfectosEntrantes();
        }

        GameManager.Instance.bossMusicSounding = false;

        // Iniciar Postludio
        bool postludioFinished = false;
        conversationManager.StartConversation(currentChapter + "Postludio", () => postludioFinished = true);
        yield return new WaitUntil(() => postludioFinished);
        GameManager.Instance.ActualizarNivelMedioEjercito();
        GameManager.Instance.AjustarNivelUnidadesLibres();

        // Calcular en número de entrenamientos en función del último capítulo de la historia principal completado
        CalcularEntrenamientos(ExtraerNumeroDeCapitulo(GameManager.Instance.lastMainChapter));

        // Guardad estado de las unidades, tanto las derrotadas como las que sobreviven
        GuardarUnidadesCaidas();
        GuardarEstadoUnidadesAlTerminarNivel();

        // Comprobar si se pueden desbloquear los apoyoos y desvíos pendientes
        SupportManager.Instance.RevisarApoyosPendientes();
        SupportManager.Instance.RevisarDesviosPendientes();
        

        // Carga de escena de campamento
        SceneLoader.Instance.LoadScene("CampScene");

        // Desbloquear tutoriales del capítulo completado
        GestorTutoriales.Instance.DesbloquearTutorial(currentChapter);
        yield break;
    }

    public void CalcularEntrenamientos(int chapter)
    {
        if (chapter >= 8) GameManager.Instance.chapterDataJuego.entrenar = 3;
        if (chapter >= 4) GameManager.Instance.chapterDataJuego.entrenar = 2;
        if (chapter >= 1) GameManager.Instance.chapterDataJuego.entrenar = 1;
    }

    private int ExtraerNumeroDeCapitulo(string chapterName)
    {
        // Solo acepta capítulos exactamente como "Chapter1", no los opcionales como "ChapterLisella1"
        if (System.Text.RegularExpressions.Regex.IsMatch(chapterName, @"^Chapter\d+$"))
        {
            string numeroComoTexto = chapterName.Substring("Chapter".Length);
            return int.TryParse(numeroComoTexto, out int result) ? result : -1;
        }

        return -1; // No es un capítulo principal
    }

    private void LoadReinforcements(string nombreCapitulo)
    {
        TextAsset archivo = Resources.Load<TextAsset>("Data/" + nombreCapitulo + "Reinforcements");
        if (archivo != null)
        {
            refuerzos = JsonUtility.FromJson<RefuerzoWrapper>(archivo.text).refuerzos;
        }
        else
        {
            Debug.LogWarning("No se encontró el archivo de refuerzos para " + nombreCapitulo);
        }
    }

    public void GuardarUnidadesCaidas()
    {
        var unidades = TurnManager.Instance.unidadesAGuardar;

        foreach (Unidad unidad in unidades)
        {
            GuardarEstadoUnidad(unidad);
        }

        TurnManager.Instance.unidadesAGuardar.Clear(); // Tras guardar, se limpia la lista
    }

    public void GuardarEstadoUnidadesAlTerminarNivel()
    {
        List<Unidad> personajesReclutados = SupportManager.Instance
            .GetPersonajesReclutados(GameManager.Instance.datosJuego.unidades.ToList());

        foreach (UnitLoader loader in FindObjectsOfType<UnitLoader>())
        {
            Unidad unidadEscena = loader.datos;

            if (unidadEscena == null || string.IsNullOrEmpty(unidadEscena.nombre))
            {
                continue;
            }

            Unidad unidadJuego = personajesReclutados.FirstOrDefault(u => u != null && u.nombre == unidadEscena.nombre);

            if (unidadJuego != null)
            {
                // Sincronizar datos
                unidadJuego.nivel = unidadEscena.nivel;
                unidadJuego.experiencia = unidadEscena.experiencia;
                unidadJuego.MaxPV = unidadEscena.MaxPV;
                unidadJuego.poder = unidadEscena.poder;
                unidadJuego.habilidad = unidadEscena.habilidad;
                unidadJuego.velocidad = unidadEscena.velocidad;
                unidadJuego.suerte = unidadEscena.suerte;
                unidadJuego.defensa = unidadEscena.defensa;
                unidadJuego.resistencia = unidadEscena.resistencia;
            }
            else
            {
                Debug.LogWarning($"No se encontró unidad con nombre '{unidadEscena.nombre}' en los datos del jugador.");
            }
        }

        // Guardado de datos de unidades en el JSON
        GameManager.Instance.GuardarUnidadesEnJson();
    }

    // Función que se llama al guardar los datos de unidades caídas en combate, o retiradas en mapas de objetivo 'Escapar'
    public void GuardarEstadoUnidad(Unidad unidadEscena)
    {
        if (unidadEscena == null || string.IsNullOrEmpty(unidadEscena.nombre))
        {
            return;
        }

        List<Unidad> personajesReclutados = SupportManager.Instance
            .GetPersonajesReclutados(GameManager.Instance.datosJuego.unidades.ToList());

        Unidad unidadJuego = personajesReclutados
            .FirstOrDefault(u => u != null && u.nombre == unidadEscena.nombre);

        if (unidadJuego != null)
        {
            unidadJuego.nivel = unidadEscena.nivel;
            unidadJuego.experiencia = unidadEscena.experiencia;
            unidadJuego.MaxPV = unidadEscena.MaxPV;
            unidadJuego.poder = unidadEscena.poder;
            unidadJuego.habilidad = unidadEscena.habilidad;
            unidadJuego.velocidad = unidadEscena.velocidad;
            unidadJuego.suerte = unidadEscena.suerte;
            unidadJuego.defensa = unidadEscena.defensa;
            unidadJuego.resistencia = unidadEscena.resistencia;
            Debug.Log($"Estado de {unidadJuego.nombre} guardado correctamente.");
        }
        else
        {
            Debug.LogWarning($"No se encontró unidad '{unidadEscena.nombre}' en los datos del jugador.");
        }
    }

}
