using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum TipoOpcion { Slider, Selector, Boton } //Los tres tipos de ajustes que existen

public class OptionMenu : MonoBehaviour
{
    public TipoOpcion tipo;
    public TMP_Text etiqueta;
    public Slider slider;
    public TMP_Text[] opciones;
    public int opcionActual = 0;

    public void Seleccionar(bool activo)
    {
        etiqueta.color = activo ? Color.yellow : Color.white;

        if (tipo == TipoOpcion.Selector)
        {
            for (int i = 0; i < opciones.Length; i++)
                opciones[i].color = (i == opcionActual && activo) ? Color.yellow : Color.white;
        }
    }

    public void CambiarValor(int direccion)
    {
        switch (tipo)
        {
            case TipoOpcion.Slider:
                slider.value += direccion * 0.1f;
                break;
            case TipoOpcion.Selector:
                opcionActual = Mathf.Clamp(opcionActual + direccion, 0, opciones.Length - 1);
                ActualizarSelector();
                break;
        }
    }

    public void Confirmar()
    {
        if (tipo == TipoOpcion.Boton)
        {
            SettingsMenu.Instance.MostrarPanelBandaSonora();
        }
        else if (tipo == TipoOpcion.Selector)
        {
            if (etiqueta.text == "Velocidad de texto")
            {
                string velocidad = "";

                switch (opcionActual)
                {
                    case 0: velocidad = "Lenta"; break;
                    case 1: velocidad = "Normal"; break;
                    case 2: velocidad = "Rapida"; break;
                }

                CambiarVelocidadTexto(velocidad);
            }

            if (etiqueta.text == "Final de turno")
            {
                string final = "";

                switch (opcionActual)
                {
                    case 0: final = "Automatico"; break;
                    case 1: final = "Confirmar"; break;
                }

                CambiarFinalTurno(final);
            }
        }
    }

    private void CambiarVelocidadTexto(string velocidad)
    {
        PlayerPrefs.SetString("VelocidadTexto", velocidad);
        PlayerPrefs.Save();
        Debug.Log("Velocidad de texto guardada: " + velocidad);
    }

    private void CambiarFinalTurno(string final)
    {
        PlayerPrefs.SetString("FinalTurno", final);
        PlayerPrefs.Save();
        TurnManager.Instance.ActualizarModoFinTurno();
        Debug.Log("Final de turno guardado: " + final);
    }

    private void ActualizarSelector()
    {
        for (int i = 0; i < opciones.Length; i++)
            opciones[i].color = (i == opcionActual) ? Color.yellow : Color.white;
    }
}
