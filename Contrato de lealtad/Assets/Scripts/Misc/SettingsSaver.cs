using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsSaver : MonoBehaviour
{
    // Guardado de la opacidad de la cuadrícula al cambiar el valor del slider
    public void GuardarOpacidadCuadricula(float valor)
    {
        PlayerPrefs.SetFloat("OpacidadCuadricula", valor);
    }
}