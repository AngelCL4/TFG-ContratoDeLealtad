using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurarPanelUI : MonoBehaviour
{
    [SerializeField] private Image iconoClase;
    [SerializeField] private TextMeshProUGUI nombreTexto;
    [SerializeField] private TextMeshProUGUI pvTexto;

    // Mostrar la unidad a curar, su nombre y su vida actual comparada a su vida máxima
    public void Mostrar(UnitLoader objetivo)
    {
        iconoClase.sprite = Resources.Load<Sprite>($"Portraits/{objetivo.datos.nombre}Neutral");
        nombreTexto.text = objetivo.datos.nombre;
        pvTexto.text = $"PV: {objetivo.datos.PV} / {objetivo.datos.MaxPV}";
        gameObject.SetActive(true);
    }

    public void Ocultar()
    {
        gameObject.SetActive(false);
    }
}
