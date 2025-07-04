using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatMenu : MonoBehaviour
{
    [SerializeField] private TutorialMenu tutorialMenu;
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private ObjectiveMenu objectiveMenu;
    [SerializeField] private TextMeshProUGUI textoTurnos;
    [SerializeField] private TerrenoUI uiTerreno;
    public GameObject menuPanel;
    public TextMeshProUGUI[] opciones;
    private int opcionSeleccionada = 0;
    public bool menuActivo = false;
    public bool finPulsado = false;

    public bool MenuActivo => menuActivo;

    void Start()
    {
        menuPanel.SetActive(false);
        ActualizarSeleccionVisual();
    }

    void Update()
    {
        // Si hay una conversación activa o ya está el menú activo, no ocurre nada
        if (!TurnManager.Instance.conversationFinished) return;
        if (!menuActivo) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            opcionSeleccionada = (opcionSeleccionada - 1 + opciones.Length) % opciones.Length;
            ActualizarSeleccionVisual();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            opcionSeleccionada = (opcionSeleccionada + 1) % opciones.Length;
            ActualizarSeleccionVisual();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
            CerrarMenu();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
            EjecutarOpcionSeleccionada();
        }
    }

    // Actualización del texto de turnos
    public void ActualizarTextoTurnos()
    {   
        textoTurnos.text = $"Turno: {TurnManager.Instance.TurnoActual.ToString()}";
    }

    // Si el menú ya está abierto, no volver a abrirlo
    public void AbrirMenu()
    {
        if (menuActivo) return;
        menuPanel.SetActive(true);
        uiTerreno.gameObject.SetActive(true);
        menuActivo = true;
        opcionSeleccionada = 0;
        ActualizarSeleccionVisual();
    }

    public void CerrarMenu()
    {
        uiTerreno.gameObject.SetActive(false);
        menuPanel.SetActive(false);
        menuActivo = false;
    }

    void ActualizarSeleccionVisual()
    {
        for (int i = 0; i < opciones.Length; i++)
        {
            opciones[i].color = (i == opcionSeleccionada) ? Color.yellow : Color.white;
        }
    }

    void EjecutarOpcionSeleccionada()
    {
        string opcion = opciones[opcionSeleccionada].text;
        Debug.Log("Seleccionaste: " + opcion);

        // Dependiendo de la opción elegida se abre el menú correspondiente y se cierra el menú anterior, si eliges Fin se pasa a la fase del enemigo
        switch (opcion)
        {
            case "Tutoriales":
                CerrarMenu();
                tutorialMenu.Abrir();
                break;
            case "Ajustes":
                CerrarMenu();
                settingsMenu.Abrir();
                break;
            case "Objetivo":
                CerrarMenu();
                objectiveMenu.Abrir();
                break;
            case "Fin":
                finPulsado = true;
                CerrarMenu();
                TurnManager.Instance.PasarFaseJugador();
                break;
        }
    }
}
