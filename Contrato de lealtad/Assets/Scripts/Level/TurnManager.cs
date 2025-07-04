using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum Fase
{
    Jugador,
    Enemigo
}

public class TurnManager : MonoBehaviour
{
    [SerializeField] private CombatMenu combatMenu;
    [SerializeField] private ObjectiveMenu objectiveMenu;
    [SerializeField] private ConversationManager conversationManager;
    public static TurnManager Instance { get; private set; }
    private bool cambiandoFase = false;

    public int TurnoActual { get; private set; } = 0;
    public Fase FaseActual { get; private set; } = Fase.Jugador;

    public static event Action<Fase> OnFaseCambiada;
    public static event Action<int> OnTurnoCambiado;

    public List<UnitLoader> unidadesJugador = new List<UnitLoader>();
    public List<UnitLoader> unidadesEnemigas = new List<UnitLoader>();
    public List<UnitLoader> unidadesAEliminar = new List<UnitLoader>();
    public List<Unidad> unidadesAGuardar = new List<Unidad>();
    public GameObject panelDerrota;

    private bool gameOverActivado = false;

    private bool finTurnoAutomatico;
    public bool conversationFinished = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        unidadesJugador.AddRange(FindObjectsOfType<UnitLoader>());
        unidadesEnemigas.AddRange(FindObjectsOfType<UnitLoader>());

        string modo = PlayerPrefs.GetString("FinalTurno", "Automatico"); // Por defecto final de turno automático
        finTurnoAutomatico = modo == "Automatico";
    }

    private void Update()
    {
        if (gameOverActivado && Input.anyKeyDown)
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
            VolverAlMenuPrincipal();
        }
    }

    public void ComprobarDerrota()
    {
        if (unidadesJugador.Count == 0 && !gameOverActivado)
        {
            ActivarGameOver();
        }
    }

    // Se activa el panel de derrota, no se guardan los cambios a las unidades (como subidas de nivel) y se pausa el juego
    private void ActivarGameOver()
    {
        gameOverActivado = true;
        unidadesAGuardar.Clear();
        panelDerrota.SetActive(true);
        Time.timeScale = 0f;
    }

    private void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f; // Restaurar el tiempo antes de cambiar de escena
        SceneLoader.Instance.LoadScene("MainMenuScene");
    }

    public void ActualizarModoFinTurno()
    {
        string modo = PlayerPrefs.GetString("FinalTurno", "Automatico");
        finTurnoAutomatico = modo == "Automatico";
        Debug.Log($"Modo de finalización actualizado: {(finTurnoAutomatico ? "Automático" : "Confirmar")}");
    }

    // Registro de unidades aliadas
    public void RegistrarUnidadAliada(UnitLoader unidad)
    {
        if (!unidadesJugador.Contains(unidad))
            unidadesJugador.Add(unidad);
    }

    // Registro de unidades enemigas
    public void RegistrarUnidadEnemiga(UnitLoader unidad)
    {
        if (!unidadesEnemigas.Contains(unidad))
            unidadesEnemigas.Add(unidad);
    }

    // Si todas las unidades aliadas han actuado y el fin de turno es automático se cambia de fase
    public IEnumerator NotificarUnidadTerminada(UnitLoader unidad)
    {
        if (TodasLasUnidadesAliadasHanActuado() && finTurnoAutomatico && !cambiandoFase)
        {
            if (ConversationManager.Instance.VisitConversationActive) // Si hay una conversación de visita activa, se espera a que termine
            {
                yield return new WaitUntil(() => !ConversationManager.Instance.VisitConversationActive);
            }
            CambiarAFaseEnemiga();
        }
    }

    private bool TodasLasUnidadesAliadasHanActuado()
    {
        foreach (var unidad in unidadesJugador)
        {
            if (!unidad.yaActuo) return false;
        }
        return true;
    }

    // Se pasa la fase del jugador y todas las unidades se marcan como usadas
    public void PasarFaseJugador()
    {
        if (FaseActual == Fase.Jugador)
        {
            foreach (var unidad in unidadesJugador)
            {
                unidad.MarcarComoUsada();
            }
            CambiarAFaseEnemiga();
        }
    }

    public IEnumerator IniciarFaseJugador()
    {
        FaseActual = Fase.Jugador;
        // Se aumenta el turno actual
        TurnoActual++;
        combatMenu.ActualizarTextoTurnos();

        //Se spawnean refuerzos
        UnitSpawner.Instance.SpawnearRefuerzos(TurnoActual);

        conversationFinished = false;
        //Sucede la conversación de evento de ese turno
        conversationManager.StartEventConversation(TurnoActual, () => conversationFinished = true);
        yield return new WaitUntil(() => conversationFinished);
        foreach (var unidad in unidadesJugador)
        {
            unidad.ResetearUso(); // Se reactiva el uso de las unidades del jugador
            unidad.ActualizarBuffsTemporales();
        }

        OnTurnoCambiado?.Invoke(TurnoActual);
        OnFaseCambiada?.Invoke(FaseActual);

        Debug.Log($"Turno {TurnoActual}: Comienza la fase del jugador.");

        // Se comprueba si han pasado todos los turnos indicados en el objetivo 'Sobrevivir' si es ese el objetivo
        if(objectiveMenu.data.victoryCondition == "Sobrevivir")
        {
            ComprobarSobrevivir();
        }
        objectiveMenu.ActualizarTextoLimiteTurnos();
    }

    private void CambiarAFaseEnemiga()
    {
        if (cambiandoFase) return;

        cambiandoFase = true;

        FaseActual = Fase.Enemigo;
        OnFaseCambiada?.Invoke(FaseActual);

        Debug.Log("Comienza la fase del enemigo.");

        StartCoroutine(SimularFaseEnemiga());
    }

    private System.Collections.IEnumerator SimularFaseEnemiga()
    {
        // Bucle for para poder controlar el índice y modificar la lista durante el ciclo, por si una unidad muere durante su acción
        for (int i = 0; i < unidadesEnemigas.Count; i++)
        {
            var enemigo = unidadesEnemigas[i];
            EnemyAI ia = enemigo.GetComponent<EnemyAI>();
            
            if (ia != null)
            {
                yield return StartCoroutine(ia.TomarDecision(enemigo));
            }
            else
            {
                Debug.LogWarning($"EnemyAI no encontrado en {enemigo.name}");
            }

            // Si el enemigo muere durante la toma de decisiones, se elimina inmediatamente
            if (enemigo.datos.PV == 0)
            {
                unidadesEnemigas.RemoveAt(i);
                Destroy(enemigo.gameObject);

                yield return new WaitForEndOfFrame();

                if (ActionMenu.Instance != null)
                {   
                    // Se comprueba si el enemigo muerto es el jefe y el objetivo es 'Comandante'
                    if (ActionMenu.Instance.objectiveMenu.data.victoryCondition == "Comandante")
                        ActionMenu.Instance.ComprobarJefe();
                        
                    // Se comprueba si el enemigo muerto es el último enemigo y el objetivo es 'Derrotar'
                    if (ActionMenu.Instance.objectiveMenu.data.victoryCondition == "Derrotar")
                        ActionMenu.Instance.ComprobarVictoria();
                }

                i--; // Decremento del índice para evitar saltarse el siguiente enemigo
                continue;
            }

            if (enemigo != null)
            {
                enemigo.ResetearUso();
            }

            yield return new WaitForSeconds(1f); // Retardo de 1 segundo entre acciones de enemigos
        }

        cambiandoFase = false;
        StartCoroutine(IniciarFaseJugador());
    }

    // Si el turno actual es mayor que el objetivo, completar el capítulo
    public void ComprobarSobrevivir()
    {
        if (TurnoActual > objectiveMenu.data.turnos)
        {
            ChapterManager.Instance.chapterCompleted = true;
        }
    }
}
