using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private SettingsMenu settingsPanel;

    private Button[] menuButtons;
    private TextMeshProUGUI[] buttonTexts;
    private int currentIndex = 0;
    private Color defaultColor = new Color(0.7568f, 0.302f, 0.0f, 1f);
    private Color selectedColor = Color.white;

    private void Start()
    {
        menuButtons = new Button[] { newGameButton, continueButton, settingsButton, quitButton };

        buttonTexts = new TextMeshProUGUI[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
        {
            buttonTexts[i] = menuButtons[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        // Selección inicial
        SelectButton(currentIndex);

        //Asignación de listeners
        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void Update()
    {
        // Moverse hacia arriba
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex = (currentIndex - 1 + menuButtons.Length) % menuButtons.Length;
            SelectButton(currentIndex);
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
        }

        // Moverse hacia abajo
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex = (currentIndex + 1) % menuButtons.Length;
            SelectButton(currentIndex);
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
        }

        // Seleccionar opción con A
        if (Input.GetKeyDown(KeyCode.A))
        {
            menuButtons[currentIndex].onClick.Invoke();
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
        }
    }

    private void SelectButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(menuButtons[index].gameObject);
        menuButtons[index].Select();

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            buttonTexts[i].color = (i == index) ? selectedColor : defaultColor;
        }
    }

    //Empezar nueva partida
    private void OnNewGameClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance es null");
            return;
        }

        Debug.Log("Nueva Partida Iniciada");
        SupportManager.Instance.BorrarDatosGuardados();
        GameManager.Instance.StartNewGame();
    }

    //Cargar partida empezada
    private void OnContinueClicked()
    {
        GameManager.Instance.CargarDatosPartida();
        SceneLoader.Instance.LoadScene("CampScene");
    }

    //Abrir menú de ajustes
    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.Abrir();
            this.enabled = false;
        }
        else
        {
            Debug.LogWarning("settingsPanel es null");
        }
    }

    //Salir del juego
    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
