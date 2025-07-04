using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject textConfirmacion;
    [SerializeField] private GameObject textSalida;
    [SerializeField] private TextMeshProUGUI siText;
    [SerializeField] private TextMeshProUGUI noText;

    private int currentSelection = 0;
    private bool isConfirmingExit = false;
    private bool isActive = false;

    private void OnEnable()
    {
        isActive = true;
        isConfirmingExit = false;
        currentSelection = 0;
        textConfirmacion.SetActive(true);
        textSalida.SetActive(false);
        UpdateSelection();
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            currentSelection = 1 - currentSelection;
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
            if (!isConfirmingExit)
            {
                if (currentSelection == 0) // Sí, guardar con función de CampManager
                {
                    CampManager.Instance.GuardarPartida();
                    textConfirmacion.SetActive(false);
                    textSalida.SetActive(true);
                    isConfirmingExit = true;
                    currentSelection = 0;
                    UpdateSelection();
                }
                else // No, cancelar guardado
                {
                    CloseMenu();
                }
            }
            else
            {
                if (currentSelection == 0) // Sí, salir al menú principal
                {
                    CloseMenu();
                    Destroy(CampManager.Instance.gameObject); 
                    SceneManager.LoadScene("MainMenuScene");
                }
                else // No, volver al campamento
                {
                    CloseMenu();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
            CloseMenu();
        }
    }

    private void UpdateSelection()
    {
        if (currentSelection == 0)
        {
            siText.color = Color.yellow; 
            noText.color = Color.white;
        }
        else
        {
            siText.color = Color.white;
            noText.color = Color.yellow;
        }
    }

    private void CloseMenu()
    {
        isActive = false;
        this.gameObject.SetActive(false);
        CampManager.Instance.isMenuActive = true; // Reactivar el menú principal
    }
}
