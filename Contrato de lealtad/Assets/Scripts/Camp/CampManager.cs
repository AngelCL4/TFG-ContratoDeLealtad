using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;

public class CampManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject[] menuOptions; // Opciones del menú
    [SerializeField] private GameObject[] activeImages; 
    [SerializeField] private GameObject[] inactiveImages; 
    [SerializeField] private Image background;

    private int currentSelectionIndex = 0;
    public bool isMenuActive = true;
    public int entrenamientos;
    private GameManager gameManager;

    // Rutas de guardado de archivos de unidades, capítulos y desbloqueos
    private string unidadSavePath;
    private string chapterSavePath;
    private string unlockedSavePath;

    public static CampManager Instance { get; private set; }

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

        unidadSavePath = Path.Combine(Application.persistentDataPath, "unitsData_partida.json");
        chapterSavePath = Path.Combine(Application.persistentDataPath, "chapterData_partida.json");
        unlockedSavePath = Path.Combine(Application.persistentDataPath, "unlockedChapters_partida.json");
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        Sprite fondo = Resources.Load<Sprite>($"Backgrounds/{gameManager.fondoCampamento}");
        if (fondo != null)
        {
            background.sprite = fondo;
        }
        else
        {
            Debug.LogWarning($"No se encontró el fondo: {gameManager.fondoCampamento}");
        }
        if(!string.IsNullOrEmpty(gameManager.musicaCampamento))
        {
            MusicManager.Instance.PlayMusic(gameManager.musicaCampamento);
        }
        UpdateMenu();
        entrenamientos = GameManager.Instance.chapterDataJuego.entrenar;
    }

    private void Update()
    {
        if (!isMenuActive) return;

        // Navegación con las flechas
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelectionIndex--;
            if (currentSelectionIndex < 0) currentSelectionIndex = menuOptions.Length - 1;
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            UpdateMenu();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelectionIndex++;
            if (currentSelectionIndex >= menuOptions.Length) currentSelectionIndex = 0;
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            UpdateMenu();
        }

        // Confirmar selección con A
        if (Input.GetKeyDown(KeyCode.A))
        {
            SelectOption();
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
        }
    }

    private void UpdateMenu()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
            activeImages[i].SetActive(false);
            inactiveImages[i].SetActive(true);
        }

        activeImages[currentSelectionIndex].SetActive(true);
        inactiveImages[currentSelectionIndex].SetActive(false);
    }

    // Cuando el jugador selecciona una opción
    public void SelectOption()
    {
        isMenuActive = false;
        switch (currentSelectionIndex)
        {
            case 0: // Contratar
                ContractMenu contractMenu = FindObjectOfType<ContractMenu>(true);
                contractMenu.gameObject.SetActive(true);
                break;
            case 1: // Conversar
                ConverseMenu converseMenu = FindObjectOfType<ConverseMenu>(true);
                converseMenu.gameObject.SetActive(true);
                break;
            case 2: // Inventario
                InventoryMenu inventoryMenu = FindObjectOfType<InventoryMenu>(true);
                inventoryMenu.gameObject.SetActive(true);
                break;
            case 3: // Entrenar
                TrainMenu trainMenu = FindObjectOfType<TrainMenu>(true);
                trainMenu.gameObject.SetActive(true);
                break;
            case 4: // Ajustes
                SettingsMenu settingsMenu = FindObjectOfType<SettingsMenu>(true);
                settingsMenu.Abrir();
                break;
            case 5: // Tutorial
                TutorialMenu tutorialMenu = FindObjectOfType<TutorialMenu>(true);
                tutorialMenu.Abrir();
                break;
            case 6: // Guardar
                SaveMenu saveMenu = FindObjectOfType<SaveMenu>(true);
                saveMenu.gameObject.SetActive(true);
                break;
            case 7: // Continuar
                ContinueMenu continueMenu = FindObjectOfType<ContinueMenu>(true);
                continueMenu.OpenContinueMenu();
                break;
        }
    }

    public void GuardarPartida()
    {
        // Guardar unidades
        string unidadesJson = JsonConvert.SerializeObject(GameManager.Instance.datosJuego, Formatting.Indented);
        File.WriteAllText(unidadSavePath, unidadesJson);
        Debug.Log("Unidades guardadas.");

        // Guardar progreso de capítulos
        string chapterJson = JsonConvert.SerializeObject(GameManager.Instance.chapterDataJuego, Formatting.Indented);
        File.WriteAllText(chapterSavePath, chapterJson);
        Debug.Log("Progreso de capítulos guardado.");

        // Guardar capítulos desbloqueados
        string unlockedJson = JsonConvert.SerializeObject(GameManager.Instance.unlockedChapterDataJuego, Formatting.Indented);
        File.WriteAllText(unlockedSavePath, unlockedJson);
        Debug.Log("Capítulos desbloqueados guardados.");

        // Guardar objetos almacenados, apoyos pendientes y desvíos pendientes
        AlmacenObjetos.Instance.GuardarObjetos();
        SupportManager.Instance.GuardarApoyosPendientes();
        SupportManager.Instance.GuardarDesviosPendientes();
    }
}
