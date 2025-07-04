using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using System;

public class ContractMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI contractCountText;
    [SerializeField] private GameObject[] mercenarySlots;
    [SerializeField] private Image[] mercenaryPortraits;
    [SerializeField] private Image[] mercenaryClassImages;
    [SerializeField] private TextMeshProUGUI[] mercenaryData;
    [SerializeField] private TextMeshProUGUI[] mercenaryDescriptions;
    [SerializeField] private TextMeshProUGUI confirmationMenu;
    [SerializeField] private GameObject startButton;

    private List<Unidad> availableUnits;
    private int currentSelectionIndex = 0;
    private int contractCount;
    private Unidad selectedUnit;
    private List<Unidad> seleccionados = new List<Unidad>();
    private enum MenuState
    {
        Initialization,
        Selection,
        Confirmation
    }

    private MenuState currentState;

    private string unitsDataPath = "Assets/Resources/unitsData.json";

    public void Start()
    {
        currentState = MenuState.Initialization;
        currentSelectionIndex = 0;
        UpdateContractCount();
        startButton.SetActive(true);
        contractCountText.gameObject.SetActive(true);
    }

    // Para que funcione correctamente al tener más de 1 contrato
    private void OnEnable()
    {
        currentState = MenuState.Initialization;
        currentSelectionIndex = 0;
        UpdateContractCount();
        startButton.SetActive(true);
        contractCountText.gameObject.SetActive(true);
    }

    private void LoadUnitsData()
    {
        string json = File.ReadAllText(unitsDataPath);
        DatosJuego unitData = JsonConvert.DeserializeObject<DatosJuego>(json);
        availableUnits = new List<Unidad>();

        // Solo unidades con estado 'Libre'
        foreach (var unit in unitData.unidades)
        {
            if (unit.estado == "Libre")
            {
                availableUnits.Add(unit);
            }
        }

        if (contractCount > 0 && availableUnits.Count > 0)
        {
            DisplayMercenaries();
        }
    }

    private void DisplayMercenaries()
    {
        for (int i = 0; i < mercenarySlots.Length; i++)
        {
            mercenaryData[i].text = "";
            mercenaryDescriptions[i].text = "";
            mercenaryPortraits[i].sprite = null;
            mercenaryClassImages[i].sprite = null;
            mercenarySlots[i].SetActive(false);
        }
        int count = Mathf.Min(5, availableUnits.Count); //Solo se muestran cinco si hay más
        seleccionados.Clear();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableUnits.Count);
            Unidad unit = availableUnits[randomIndex];

            while (seleccionados.Contains(unit)) // Evitar repetir unidades
            {
                randomIndex = UnityEngine.Random.Range(0, availableUnits.Count);
                unit = availableUnits[randomIndex];
            }

            seleccionados.Add(unit);

            // Asignación de información
            mercenaryData[i].text = $"{unit.nombre} - Nivel: {unit.nivel} - Clase: {unit.clase.nombre}.";
            mercenaryDescriptions[i].text = $"{unit.descripcion}";

            Sprite portrait = Resources.Load<Sprite>($"Portraits/{unit.nombre}Neutral");
            if (portrait != null)
            {
                mercenaryPortraits[i].sprite = portrait;
            }
            else
            {
                Debug.LogWarning($"No se encontró el retrato para {unit.nombre}Neutral");
            }

            Sprite classSprite = Resources.Load<Sprite>($"Sprites/{unit.clase.nombre.Replace(" ", "")}{unit.nombre}");
            if (classSprite != null)
            {
                mercenaryClassImages[i].sprite = classSprite;
            }
            else
            {
                Debug.LogWarning($"No se encontró la imagen de clase para {unit.clase.nombre}{unit.nombre}");
            }

            mercenarySlots[i].SetActive(true);
        }
        currentState = MenuState.Selection;
    }

    private void UpdateContractCount()
    {
        // Actualización del texto con la cantidad de contratos
        contractCount = GameManager.Instance.chapterDataJuego.contratos;
        contractCountText.text = $"Contratos disponibles: {contractCount}";
    }

    void Update()
    {
        if (currentState == MenuState.Initialization)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                InitializeMenu();
            }
            // Al pulsar "S" en este estado, cerrar el menú y volver al campamento
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                CloseContractMenu();
            }
        }
        else if (currentState == MenuState.Selection)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                currentSelectionIndex = (currentSelectionIndex - 1 + 5) % 5;
                UpdateMercenarySelection();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                currentSelectionIndex = (currentSelectionIndex + 1) % 5;
                UpdateMercenarySelection();
            }

            // Seleccionar un mercenario con A
            if (Input.GetKeyDown(KeyCode.A))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                SelectMercenary();
            }
            //No puedes pulsar S si ya has gastado un contrato, debes reclutar a alguien
        }
        else if (currentState == MenuState.Confirmation)
        {
            // Confirmar reclutamiento con A
            if (Input.GetKeyDown(KeyCode.A))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                ConfirmRecruited();
            }

            // Cancelar con S y volver al estado de selección
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                CancelRecruited();
            }
        }
    }

    private void InitializeMenu()
    {
        startButton.SetActive(false);
        LoadUnitsData();
    }

    private void UpdateMercenarySelection()
    {
        // Resaltar el mercenario seleccionado
        for (int i = 0; i < 5; i++)
        {
            if (i == currentSelectionIndex)
            {
                mercenarySlots[i].GetComponent<Image>().color = Color.white; // Color normal
            }
            else
            {
                mercenarySlots[i].GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f); // Color oscuro
            }
        }
    }

    private void SelectMercenary()
    {
        if (contractCount > 0)
        {
            // Confirmar el reclutamiento del mercenario
            selectedUnit = seleccionados[currentSelectionIndex];
            confirmationMenu.gameObject.SetActive(true);
            confirmationMenu.text = $"¿Reclutar a {selectedUnit.nombre}?";
            currentState = MenuState.Confirmation;
        }
    }

    public void ConfirmRecruited()
    {
        // Cambiar el estado del mercenario a "Reclutado"
        selectedUnit.estado = "Reclutado";

        // Buscar y actualizar solo la unidad reclutada en la lista de unidades disponibles
        DatosJuego unitData = JsonConvert.DeserializeObject<DatosJuego>(File.ReadAllText(unitsDataPath));
        for (int i = 0; i < unitData.unidades.Length; i++)
        {
            if (unitData.unidades[i].nombre == selectedUnit.nombre)
            {
                unitData.unidades[i].estado = "Reclutado";
                break;
            }
        }

        // Guardado de los datos actualizados en el archivo JSON
        string json = JsonConvert.SerializeObject(unitData, Formatting.Indented);
        File.WriteAllText(unitsDataPath, json);
        GameManager.Instance.datosJuego = unitData;

        // Reducir los contratos
        GameManager.Instance.chapterDataJuego.contratos--;
        UpdateContractCount();

        for (int i = 0; i < GameManager.Instance.datosJuego.unidades.Length; i++)
        {
            if (GameManager.Instance.datosJuego.unidades[i].nombre == selectedUnit.nombre)
            {
                GameManager.Instance.datosJuego.unidades[i].estado = "Reclutado";
                break;
            }
        }
        
        confirmationMenu.gameObject.SetActive(false);
        CloseContractMenu(); // Volver al menú principal
    }

    public void CancelRecruited()
    {
        confirmationMenu.gameObject.SetActive(false);
        currentState = MenuState.Selection;
    }

    public void CloseContractMenu()
    {
        // Regresar al campamento y cerrar el menú de contratos
        GameObject campMenu = GameObject.Find("CampManager");
        if (campMenu != null)
        {
            campMenu.GetComponent<CampManager>().isMenuActive = true;
        }
        ResetContractMenu();
        OnCerrarContrato?.Invoke();
        gameObject.SetActive(false);
    }

    private void ResetContractMenu()
    {
        availableUnits?.Clear();
        currentSelectionIndex = 0;
        selectedUnit = null;
        seleccionados.Clear();

        for (int i = 0; i < mercenarySlots.Length; i++)
        {
            mercenaryData[i].text = "";
            mercenaryDescriptions[i].text = "";
            mercenaryPortraits[i].sprite = null;
            mercenaryClassImages[i].sprite = null;
            mercenarySlots[i].SetActive(false);
            mercenarySlots[i].GetComponent<Image>().color = Color.white;
        }

        confirmationMenu.gameObject.SetActive(false);
        currentState = MenuState.Initialization;
    }

    public Action OnCerrarContrato;
}
