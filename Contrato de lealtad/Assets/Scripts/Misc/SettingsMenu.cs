using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private List<OptionMenu> opciones;
    private int indiceActual = 0;
    private bool enModoEdicion = false;
    private bool panelBandaSonoraActivo = false;
    [SerializeField] private Slider cuadriculaSlider;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderEfectos;
    [SerializeField] private GameObject mainMenuControllerObject;
    [SerializeField] private GameObject panelBandaSonora;

    public static SettingsMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(false);

        //Asignacion de valores por defecto
        float opacidad = PlayerPrefs.GetFloat("OpacidadCuadricula", 0f);
        cuadriculaSlider.value = opacidad;
        sliderMusica.value = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
        sliderMusica.value = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
        sliderEfectos.value = PlayerPrefs.GetFloat("VolumenEfectos", 0.5f);

        if (!PlayerPrefs.HasKey("VelocidadTexto"))
        {
            PlayerPrefs.SetString("VelocidadTexto", "Normal");
        }
        if (!PlayerPrefs.HasKey("FinalTurno"))
        {
            PlayerPrefs.SetString("FinalTurno", "Automatico");
        }
    }

    public void Abrir()
    {
        gameObject.SetActive(true);
        ActualizarSeleccion();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);

            // Si está activo el panel de banda sonora, se cierra
            if (panelBandaSonoraActivo)
            {
                panelBandaSonora.SetActive(false);
                panelBandaSonoraActivo = false;
                return;
            }

            // Si estás editando algún ajuste, vuelves, si no, sales del menú
            if (enModoEdicion)
            {
                enModoEdicion = false;
            }
            else
            {
                CerrarMenu();
            }
        }

        // Si no estás en modo edición, te mueves entre ajustes, si lo estás, cambias los valores
        if (!enModoEdicion)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                CambiarOpcion(-1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                CambiarOpcion(1);
            }
            if (Input.GetKeyDown(KeyCode.A)) {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                ActivarOpcion();
            }

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                opciones[indiceActual].CambiarValor(-1);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                opciones[indiceActual].CambiarValor(1);
            }
            if (Input.GetKeyDown(KeyCode.A)) {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                ConfirmarOpcion();
            }
        }
    }

    private void CambiarOpcion(int dir)
    {
        opciones[indiceActual].Seleccionar(false);
        indiceActual = (indiceActual + dir + opciones.Count) % opciones.Count;
        ActualizarSeleccion();
    }

    private void ActualizarSeleccion()
    {
        opciones[indiceActual].Seleccionar(true);
    }

    private void ActivarOpcion()
    {
        var actual = opciones[indiceActual];

        if (actual.tipo == TipoOpcion.Boton)
        {
            actual.Confirmar(); // Banda sonora
        }
        else
        {
            enModoEdicion = true;
        }
    }

    private void ConfirmarOpcion()
    {
        var actual = opciones[indiceActual];
        actual.Confirmar();
        enModoEdicion = false;
    }

    public void MostrarPanelBandaSonora()
    {
        panelBandaSonora.SetActive(true);
        panelBandaSonoraActivo = true;
    }

    // Si estás en MainMenuScene, el menú principal se activa de nuevo, si estás en CampScene, el menú del campamento se activa de nuevo, si no (en ChapterScene) el menú simplemente se cierra 
    private void CerrarMenu()
    {
        gameObject.SetActive(false);
        if (mainMenuControllerObject != null)
        {
            mainMenuControllerObject.GetComponent<MainMenuController>().enabled = true;
        }
        GameObject campManagerObj = GameObject.Find("CampManager");
        
        if (campManagerObj != null)
        {
            CampManager campManager = campManagerObj.GetComponent<CampManager>();
            if (campManager != null)
            {
                campManager.isMenuActive = true;
            }
        }
    }
}
