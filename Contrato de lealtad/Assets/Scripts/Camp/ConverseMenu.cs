using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ConverseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject personajeListContainer;
    [SerializeField] private GameObject personajePrefab;
    [SerializeField] private Transform supportListContainer;
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private GameObject supportPrefab;
    [SerializeField] private ConversationManager conversationManager;

    private List<Unidad> personajesReclutados = new List<Unidad>();
    private int currentSelectionIndex = 0;
    private List<GameObject> botonesInstanciados = new();
    private enum MenuState { Initialization, Conversation }
    private MenuState currentState = MenuState.Initialization;
    private int selectedSupportIndex = 0;
    private int selectedRankIndex = 0;
    private List<Transform> supportItems = new List<Transform>();
    private int visibleStartIndex = 0;
    private const int visibleCount = 10;
    private int personajeVisibleStartIndex = 0;
    private const int personajeVisibleCount = 10;

    public void OnEnable()
    {
        // Obtener personajes reclutados desde GameManager
        personajesReclutados = SupportManager.Instance.GetPersonajesReclutados(GameManager.Instance.datosJuego.unidades.ToList());

        MostrarListaPersonajes();
        UpdateSelectionVisual();
    }

    void Update()
    {
        switch (currentState)
        {
            case MenuState.Initialization:
                HandleInitializationInput();
                break;

            case MenuState.Conversation:
                HandleConversationInput();
                break;
        }
    }

    //En estado de inicialización, solo se muestran los personajes reclutados en una lista de 10 en 10
    private void HandleInitializationInput()
    {
        if (botonesInstanciados.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            currentSelectionIndex++;

            if (currentSelectionIndex >= personajeVisibleCount)
            {
                if (personajeVisibleStartIndex + personajeVisibleCount < personajesReclutados.Count)
                {
                    personajeVisibleStartIndex += personajeVisibleCount;
                    currentSelectionIndex = 0;
                    MostrarListaPersonajes();
                }
                else
                {
                    currentSelectionIndex = personajeVisibleCount - 1;
                }
            }

            UpdateSelectionVisual();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            currentSelectionIndex--;

            if (currentSelectionIndex < 0)
            {
                if (personajeVisibleStartIndex > 0)
                {
                    personajeVisibleStartIndex = Mathf.Max(0, personajeVisibleStartIndex - personajeVisibleCount);
                    currentSelectionIndex = personajeVisibleCount - 1;
                    MostrarListaPersonajes();
                }
                else
                {
                    currentSelectionIndex = 0;
                }
            }

            UpdateSelectionVisual();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
            SeleccionarPersonaje(personajeVisibleStartIndex + currentSelectionIndex);
            currentState = MenuState.Conversation;
            detailPanel.SetActive(true);
            selectedSupportIndex = 0;
            selectedRankIndex = 0;
            UpdateConversationSelectionVisual();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
            GameObject campMenu = GameObject.Find("CampManager");
            if (campMenu != null)
            {
                campMenu.GetComponent<CampManager>().isMenuActive = true;
            }

            gameObject.SetActive(false);
        }
    }

    //En estado de conversación, se muestran los apoyos del personaje seleccionado de 10 en 10 personajes
    private void HandleConversationInput()
    {
        if (supportItems.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
            TryStartSupportConversation();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            selectedSupportIndex++;
            if (selectedSupportIndex >= Mathf.Min(visibleCount, supportItems.Count))
            {
                if (visibleStartIndex + visibleCount < SupportManager.Instance.GetApoyos(personajesReclutados[currentSelectionIndex].nombre).Count)
                {
                    visibleStartIndex += visibleCount;
                    MostrarApoyos(personajesReclutados[currentSelectionIndex].nombre);
                }
                else
                {
                    selectedSupportIndex = Mathf.Min(visibleCount - 1, supportItems.Count - 1);
                }
            }
            UpdateConversationSelectionVisual();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            selectedSupportIndex--;
            if (selectedSupportIndex < 0)
            {
                if (visibleStartIndex > 0)
                {
                    visibleStartIndex = Mathf.Max(0, visibleStartIndex - visibleCount);
                    MostrarApoyos(personajesReclutados[currentSelectionIndex].nombre);
                    selectedSupportIndex = visibleCount - 1;
                }
                else
                {
                    selectedSupportIndex = 0;
                }
            }
            UpdateConversationSelectionVisual();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            selectedRankIndex = Mathf.Max(0, selectedRankIndex - 1);
            UpdateConversationSelectionVisual();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            selectedRankIndex = Mathf.Min(2, selectedRankIndex + 1);
            UpdateConversationSelectionVisual();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
            currentState = MenuState.Initialization;
            CerrarSupportMenu();
            detailPanel.SetActive(false);
            MostrarListaPersonajes();
            UpdateSelectionVisual();
        }
    }

    //Comprobaciones antes de iniciar una conversacion de apoyo
    private void TryStartSupportConversation()
    {
        string personaje1 = personajesReclutados[currentSelectionIndex].nombre;
        string personaje2 = supportItems[selectedSupportIndex].Find("CharacterName").GetComponent<TextMeshProUGUI>().text;
        int nivel = SupportManager.Instance.GetNivelApoyo(personaje1, personaje2);
        string primero = string.Compare(personaje1, personaje2) <= 0 ? personaje1 : personaje2;
        string segundo = string.Compare(personaje1, personaje2) <= 0 ? personaje2 : personaje1;
        int rango = selectedRankIndex + 1;
        
        if (rango <= nivel)
        {
            // Verificamos si ya fue vista
            bool yaVista = SupportManager.Instance.ConversacionVista(primero, segundo, rango);

            if (yaVista)
            {
                Debug.Log("Esta conversación ya se ha visto.");
                return; // No hacemos nada si ya fue vista
            }

            // Si no fue vista, iniciamos conversación
            string convoID = $"{primero}{segundo}Rango{rango}";
            StartCoroutine(PlaySupportConversation(convoID));
        }
        else
        {
            //La conversación no está desbloqueada todavía
            Debug.Log("Rango no desbloqueado todavía.");
        }
    }

    private IEnumerator PlaySupportConversation(string convoID)
    {
        bool conversationFinished = false;
        conversationManager.StartConversation(convoID, () => conversationFinished = true);

        yield return new WaitUntil(() => conversationFinished);

        SupportManager.Instance.MarcarConversacionVista(
            personajesReclutados[currentSelectionIndex].nombre,
            supportItems[selectedSupportIndex].Find("CharacterName").GetComponent<TextMeshProUGUI>().text,
            selectedRankIndex + 1
        );
        if(!string.IsNullOrEmpty(GameManager.Instance.musicaCampamento))
        {
            MusicManager.Instance.PlayMusic(GameManager.Instance.musicaCampamento);
        }
        Debug.Log($"Conversación {convoID} finalizada.");
    }

    private void CerrarSupportMenu()
    {
        foreach (Transform child in supportListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < botonesInstanciados.Count; i++)
        {
            var img = botonesInstanciados[i].GetComponent<Image>();
            img.color = (i == currentSelectionIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
    }

    private void UpdateConversationSelectionVisual()
    {
        for (int i = 0; i < supportItems.Count; i++)
        {
            var item = supportItems[i];
            var bg = item.GetComponent<Image>();
            bg.color = (i == selectedSupportIndex) ? Color.white : new Color(0.5f, 0.5f, 0.5f);

            for (int r = 1; r <= 3; r++)
            {
                var rango = item.Find($"Rango{r}")?.GetComponent<Image>();
                if (rango != null)
                {
                    int nivel = SupportManager.Instance.GetNivelApoyo(
                        personajesReclutados[currentSelectionIndex].nombre,
                        item.Find("CharacterName").GetComponent<TextMeshProUGUI>().text
                    );

                    if (i == selectedSupportIndex && r - 1 == selectedRankIndex)
                    {
                        rango.color = Color.white;
                    }
                    else if (nivel >= r)
                    {
                        bool yaVista = SupportManager.Instance.ConversacionVista(
                            personajesReclutados[currentSelectionIndex].nombre,
                            item.Find("CharacterName").GetComponent<TextMeshProUGUI>().text,
                            r
                        );

                        if (yaVista)
                            rango.color = Color.yellow;
                        else
                            rango.color = new Color(0.7f, 0.7f, 0.7f); // Desbloqueado pero no visto
                    }
                    else
                    {
                        rango.color = new Color(0.2f, 0.2f, 0.2f); // Bloqueado
                    }
                }
            }
        }
    }

    private void MostrarListaPersonajes()
    {
        foreach (Transform child in personajeListContainer.transform)
            Destroy(child.gameObject);

        botonesInstanciados.Clear();

        int total = personajesReclutados.Count;
        int end = Mathf.Min(personajeVisibleStartIndex + personajeVisibleCount, total);

        for (int i = personajeVisibleStartIndex; i < end; i++)
        {
            Unidad unidad = personajesReclutados[i];

            GameObject button = Instantiate(personajePrefab, personajeListContainer.transform);
            botonesInstanciados.Add(button);

            Image portraitImage = button.GetComponent<Image>();
            Sprite portrait = Resources.Load<Sprite>($"Portraits/{unidad.nombre}Neutral");

            if (portrait != null)
                portraitImage.sprite = portrait;
            else
                Debug.LogWarning($"No se encontró retrato para {unidad.nombre}Neutral");

            TextMeshProUGUI nameText = button.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = unidad.nombre;
        }

        currentSelectionIndex = Mathf.Clamp(currentSelectionIndex, 0, botonesInstanciados.Count - 1);
        UpdateSelectionVisual();
    }

    private void SeleccionarPersonaje(int index)
    {
        currentSelectionIndex = index;
        selectedSupportIndex = 0;
        selectedRankIndex = 0;
        visibleStartIndex = 0;
        string nombre = personajesReclutados[index].nombre;
        MostrarApoyos(nombre);
    }

    private void MostrarApoyos(string personaje)
    {
        foreach (Transform child in supportListContainer)
            Destroy(child.gameObject);

        var apoyos = SupportManager.Instance.GetApoyos(personaje);
        int totalApoyos = apoyos.Count;
        int end = Mathf.Min(visibleStartIndex + visibleCount, totalApoyos);
        Debug.Log($"Apoyos para {personaje}: {apoyos.Count}");
        supportItems.Clear();
        for (int i = visibleStartIndex; i < end; i++)
        {
            string apoyo = apoyos[i];
            var obj = Instantiate(supportPrefab, supportListContainer);
            supportItems.Add(obj.transform);

            obj.transform.Find("CharacterName").GetComponent<TextMeshProUGUI>().text = apoyo;

            var retrato = Resources.Load<Sprite>($"Portraits/{apoyo}Neutral");
            var portraitImage = obj.GetComponent<Image>();
            if (portraitImage != null && retrato != null)
                portraitImage.sprite = retrato;

            int nivel = SupportManager.Instance.GetNivelApoyo(personaje, apoyo);
            for (int r = 1; r <= 3; r++)
            {
                var rango = obj.transform.Find($"Rango{r}")?.GetComponent<Image>();
                if (rango != null)
                {
                    if (r <= nivel)
                        rango.color = new Color(0.7f, 0.7f, 0.7f); // Desbloqueado
                    else
                        rango.color = new Color(0.2f, 0.2f, 0.2f); // Bloqueado
                }
            }
        }
        selectedSupportIndex = 0;
        selectedRankIndex = 0;
        UpdateConversationSelectionVisual();
    }
}