using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InventoryMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject personajeListContainer;
    [SerializeField] private GameObject personajePrefab;
    [SerializeField] private GameObject almacenMenu;
    [SerializeField] private GameObject almacenItemPrefab;
    [SerializeField] private GameObject almacenContainer;

    private List<Unidad> personajesReclutados;
    private List<GameObject> botonesPersonajes = new();
    private Unidad unidadSeleccionada;
    private int currentSelectionIndex = 0;
    private const int visibleCount = 10;
    private int visibleStartIndex = 0;
    private List<GameObject> botonesAlmacen = new();
    private int almacenSelectionIndex = 0;
    private const int almacenVisibleCount = 10;
    private int almacenVisibleStartIndex = 0;

    private void OnEnable()
    {
        personajesReclutados = SupportManager.Instance.GetPersonajesReclutados(GameManager.Instance.datosJuego.unidades.ToList());
        MostrarListaPersonajes();
        UpdateSelectionVisual();
    }

    private void Update()
    {
        if (!almacenMenu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                currentSelectionIndex++;
                if (currentSelectionIndex >= visibleCount)
                {
                    visibleStartIndex += visibleCount;
                    currentSelectionIndex = 0;
                    MostrarListaPersonajes();
                }
                UpdateSelectionVisual();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                currentSelectionIndex--;
                if (currentSelectionIndex < 0 && visibleStartIndex > 0)
                {
                    visibleStartIndex -= visibleCount;
                    currentSelectionIndex = visibleCount - 1;
                    MostrarListaPersonajes();
                }
                UpdateSelectionVisual();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                unidadSeleccionada = personajesReclutados[visibleStartIndex + currentSelectionIndex];
                MostrarAlmacen();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                gameObject.SetActive(false);
                GameObject.Find("CampManager").GetComponent<CampManager>().isMenuActive = true;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                almacenSelectionIndex--;
                if (almacenSelectionIndex < 0)
                {
                    if (almacenVisibleStartIndex > 0)
                    {
                        almacenVisibleStartIndex -= almacenVisibleCount;
                        almacenSelectionIndex = almacenVisibleCount - 1;
                        MostrarAlmacen();
                    }
                    else
                    {
                        almacenSelectionIndex = 0;
                    }
                }
                UpdateAlmacenVisual();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
                almacenSelectionIndex++;
                if (almacenSelectionIndex >= botonesAlmacen.Count)
                {
                    almacenVisibleStartIndex += almacenVisibleCount;
                    almacenSelectionIndex = 0;
                    MostrarAlmacen();
                }
                UpdateAlmacenVisual();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
                botonesAlmacen[almacenSelectionIndex].GetComponent<Button>().onClick.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
                almacenMenu.SetActive(false);
            }
            return;
        }
    }

    private void MostrarListaPersonajes()
    {
        foreach (Transform child in personajeListContainer.transform)
            Destroy(child.gameObject);

        botonesPersonajes.Clear();
        int end = Mathf.Min(visibleStartIndex + visibleCount, personajesReclutados.Count);

        for (int i = visibleStartIndex; i < end; i++)
        {
            Unidad unidad = personajesReclutados[i];
            GameObject item = Instantiate(personajePrefab, personajeListContainer.transform);
            botonesPersonajes.Add(item);

            item.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Portraits/{unidad.nombre}Neutral");
            var objeto = unidad.objeto;
            var iconImage = item.transform.Find("ItemIcon").GetComponent<Image>();
            var nameText = item.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();

            if (unidad.objeto != null && !string.IsNullOrEmpty(unidad.objeto.nombre))
            {
                iconImage.sprite = Resources.Load<Sprite>(objeto.spritePath);
                iconImage.enabled = true;
                nameText.text = objeto.nombre;
            }
            else //Si la unidad no tiene objeto
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
                nameText.text = "-- Nada --";
            }
        }
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < botonesPersonajes.Count; i++)
        {
            var img = botonesPersonajes[i].GetComponent<Image>();
            img.color = (i == currentSelectionIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
    }

    private void MostrarAlmacen()
    {
        almacenMenu.SetActive(true);
        almacenSelectionIndex = 0;
        var objetos = AlmacenObjetos.Instance.ObtenerObjetos();

        foreach (Transform child in almacenContainer.transform)
            Destroy(child.gameObject);

        botonesAlmacen.Clear();

        int end = Mathf.Min(almacenVisibleStartIndex + almacenVisibleCount, objetos.Count);

        // Si la unidad tiene un objeto, le aparece la opción de desequiparse ese objeto
        if (unidadSeleccionada.objeto != null && !string.IsNullOrEmpty(unidadSeleccionada.objeto.nombre))
        {
            GameObject desequiparItem = Instantiate(almacenItemPrefab, almacenContainer.transform);
            desequiparItem.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = "-Desequipar objeto-";
            desequiparItem.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/QuitarObjeto");
            desequiparItem.GetComponent<Button>().onClick.AddListener(() => DesequiparObjeto(unidadSeleccionada));
            botonesAlmacen.Add(desequiparItem);
        }

        for (int i = almacenVisibleStartIndex; i < end; i++)
        {
            var objeto = objetos[i];
            GameObject item = Instantiate(almacenItemPrefab, almacenContainer.transform);
            if (objeto.icono != null)
            {
                item.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(objeto.spritePath);
            }
            else
            {
                Debug.Log("No se ha encontrado el icono");
                //Se deja como imagen en blanco
                item.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(objeto.spritePath);
            }
            item.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = objeto.nombre;
            item.GetComponent<Button>().onClick.AddListener(() => IntercambiarObjeto(unidadSeleccionada, objeto));
            botonesAlmacen.Add(item);
        }
        UpdateAlmacenVisual();
    }

    private void UpdateAlmacenVisual()
    {
        for (int i = 0; i < botonesAlmacen.Count; i++)
        {
            var fondo = botonesAlmacen[i].transform.Find("Icon")?.GetComponent<Image>();
            if (fondo != null)
                fondo.color = (i == almacenSelectionIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
    }

    // Quitar objeto de la unidad y cambiarlo por uno del almacén
    private void IntercambiarObjeto(Unidad unidad, Objeto nuevoObjeto)
    {
        var antiguo = unidad.objeto;
        unidad.objeto = nuevoObjeto;

        AlmacenObjetos.Instance.EliminarObjeto(nuevoObjeto);
        if (antiguo != null && !string.IsNullOrEmpty(antiguo.nombre))
            AlmacenObjetos.Instance.AñadirObjeto(antiguo);

        almacenMenu.SetActive(false);
        MostrarListaPersonajes();
        UpdateSelectionVisual();
    }

    // Quitar objeto de la unidad y añadirlo al almacen
    private void DesequiparObjeto(Unidad unidad)
    {
        if (unidad.objeto != null)
        {
            AlmacenObjetos.Instance.AñadirObjeto(unidad.objeto);
            unidad.objeto = null;
            Debug.Log($"{unidad.nombre} ha sido desequipado.");
        }

        almacenMenu.SetActive(false);
        MostrarListaPersonajes();
        UpdateSelectionVisual();
    }
}