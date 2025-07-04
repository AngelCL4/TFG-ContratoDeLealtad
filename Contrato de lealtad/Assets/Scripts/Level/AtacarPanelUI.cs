using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AtacarPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoNombreAtacante;
    [SerializeField] private TextMeshProUGUI textoNombreDefensor;
    [SerializeField] private TextMeshProUGUI textoPVAtacante;
    [SerializeField] private TextMeshProUGUI textoPVDefensor;
    [SerializeField] private TextMeshProUGUI textoDanioAtacante;
    [SerializeField] private TextMeshProUGUI textoCriticoAtacante;
    [SerializeField] private TextMeshProUGUI textoDanioDefensor;
    [SerializeField] private TextMeshProUGUI textoCriticoDefensor;
    [SerializeField] private TextMeshProUGUI textoDobleAtacante;
    [SerializeField] private TextMeshProUGUI textoDobleDefensor;

    // Mostrar nombres de los involucrados en el combate, su vida, su daño y porcentaje de crítico
    public void Mostrar(string nombreAtacante, int pvAtacante, int dañoAtacante, int criticoAtacante, bool dobleAtacante,
    string nombreDefensor, int pvDefensor, int dañoDefensor, int criticoDefensor, bool dobleDefensor, bool puedeContraatacar)
    {
        if (puedeContraatacar)
        {
            textoDanioDefensor.text = dañoDefensor.ToString();
            textoCriticoDefensor.text = criticoDefensor + "%";
            textoDobleDefensor.gameObject.SetActive(dobleDefensor);
        }
        else
        {
            textoDanioDefensor.text = "--";
            textoCriticoDefensor.text = "--";
        }

        textoNombreAtacante.text = nombreAtacante;
        textoNombreDefensor.text = nombreDefensor;

        textoPVAtacante.text = pvAtacante.ToString();
        textoPVDefensor.text = pvDefensor.ToString();

        textoDanioAtacante.text = dañoAtacante.ToString();
        textoCriticoAtacante.text = criticoAtacante + "%";

        textoDobleAtacante.gameObject.SetActive(dobleAtacante);

        gameObject.SetActive(true);
    }

    public void Ocultar()
    {
        gameObject.SetActive(false);
    }
}
