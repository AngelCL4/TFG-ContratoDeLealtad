using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ConversationManager : MonoBehaviour
{

    public static ConversationManager Instance { get; private set; }

    public Image backgroundImage;
    public Image leftCharacterImage;
    public Image rightCharacterImage;
    public TextMeshProUGUI leftdialogueText;
    public TextMeshProUGUI rightdialogueText;
    public GameObject leftconversationPanel;
    public GameObject rightconversationPanel;
    public ChapterVisits chapterVisits;
    public bool VisitConversationActive { get; set; } = false;
    private Dictionary<string, bool> conversationSeenTracker = new Dictionary<string, bool>();
    private Objeto rewardActual;

    private Queue<DialogueLine> dialogueQueue;
    private System.Action onComplete;
    private bool isConversationActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string textoCompletoActual = "";
    private TextMeshProUGUI textoUIActual = null;

    private ChapterEvents loadedChapterEvents;
    private bool chapterEventsLoaded = false;
    private bool eventConversationFinished = false;

    private void Awake()
    {
        dialogueQueue = new Queue<DialogueLine>();
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartConversation(string jsonFileName, System.Action onComplete)
    {
        Debug.Log($"Intentando cargar archivo JSON: {jsonFileName}");
        this.onComplete = onComplete;
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Dialogues/{jsonFileName}");
        if (jsonAsset == null)
        {
            Debug.LogError($"No se encontró el archivo JSON: {jsonFileName}");
            onComplete?.Invoke();
            return;
        }


        ConversationData conversationData = JsonUtility.FromJson<ConversationData>(jsonAsset.text);

        rewardActual = conversationData.reward;

        // Preparacion del diálogo
        dialogueQueue.Clear();
        foreach (DialogueLine line in conversationData.dialogue)
        {
            dialogueQueue.Enqueue(line);
        }

        // Cargado de las imágenes de los personajes iniciales y el fondo
        bool hayIzquierda = PreloadInitialPortraits(conversationData.dialogue);

        bool hayCambioDeFondo = conversationData.dialogue.Any(line => !string.IsNullOrEmpty(line.backgroundChange));
        backgroundImage.gameObject.SetActive(hayCambioDeFondo);
        leftCharacterImage.gameObject.SetActive(hayIzquierda);
        rightCharacterImage.gameObject.SetActive(true);

        isConversationActive = true;
        DisplayNextLine();
    }

    // Cargado de conversaciones de evento del capítulo en curso
    public void LoadChapterEvents(string jsonFileName)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Dialogues/{jsonFileName}");
        if (jsonAsset == null)
        {
            Debug.LogError($"No se encontró el archivo de eventos: {jsonFileName}");
            return;
        }

        loadedChapterEvents = JsonUtility.FromJson<ChapterEvents>(jsonAsset.text);
        chapterEventsLoaded = true;
        Debug.Log($"Eventos del capítulo {jsonFileName} cargados correctamente.");
    }

    // Cargado de conversaciones de visita del capítulo en curso
    public ChapterVisits LoadChapterVisits(string chapterVisitsFileName)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Dialogues/{chapterVisitsFileName}");

        if (jsonAsset == null)
        {
            Debug.LogError($"No se encontró el archivo de visitas: {chapterVisitsFileName}");
            return null;
        }

        ChapterVisits chapterVisits = JsonUtility.FromJson<ChapterVisits>(jsonAsset.text);
        Debug.Log($"Visitas del capítulo {chapterVisitsFileName} cargados correctamente.");
        return chapterVisits;
    }

    // Cada turno, se comprueba si hay una conversacion de evento y si la hay, comienza
    public void StartEventConversation(int turno, System.Action onComplete)
    {

        if (!chapterEventsLoaded)
        {
            Debug.LogWarning("Los eventos del capítulo no han sido cargados todavía.");
            onComplete?.Invoke();
            return;
        }

        EventConversation evento = loadedChapterEvents.eventConversations.Find(e => e.turno == turno);

        if (evento == null)
        {
            Debug.Log($"No hay evento para el turno {turno}");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"Iniciando conversación de evento en turno {turno}.");

        dialogueQueue.Clear();
        foreach (DialogueLine line in evento.dialogue)
        {
            dialogueQueue.Enqueue(line);
        }

        bool hayIzquierda = PreloadInitialPortraits(evento.dialogue);

        bool hayCambioDeFondo = evento.dialogue.Any(line => !string.IsNullOrEmpty(line.backgroundChange));
        backgroundImage.gameObject.SetActive(hayCambioDeFondo);
        leftCharacterImage.gameObject.SetActive(hayIzquierda);
        rightCharacterImage.gameObject.SetActive(true);

        isConversationActive = true;
        this.onComplete = onComplete;

        DisplayNextLine();
    }

    public bool IsEventConversationFinished()
    {
        return eventConversationFinished;
    }

    private void Update()
    {
        if (isConversationActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                SkipToFullText(); // Muestra todo el texto de inmediato
            }
            else
            {
                DisplayNextLine(); // Avanza a la siguiente línea
            }
        }
    }

    // Carga los primeros retratos de una conversacion a izquierda y derecha, si los hay, para evitar el cuadro blanco
    private bool PreloadInitialPortraits(List<DialogueLine> dialogueLines)
    {
        string firstLeftPortrait = null;
        string firstRightPortrait = null;
        string firstLeftDirection = "left";
        string firstRightDirection = "left";

        foreach (DialogueLine line in dialogueLines)
        {
            if (line.position == "left" && firstLeftPortrait == null)
            {
                firstLeftPortrait = line.portrait;
                firstLeftDirection = line.facingDirection;
            }
            else if (line.position == "right" && firstRightPortrait == null)
            {
                firstRightPortrait = line.portrait;
                firstRightDirection = line.facingDirection;
            }

            if (firstLeftPortrait != null && firstRightPortrait != null)
                break;
        }

        if (!string.IsNullOrEmpty(firstLeftPortrait))
        {
            Sprite leftSprite = Resources.Load<Sprite>($"Portraits/{firstLeftPortrait}");
            if (leftSprite != null)
            {
                leftCharacterImage.sprite = leftSprite;
                SetFacingDirection(leftCharacterImage, firstLeftDirection);
            }
        }

        if (!string.IsNullOrEmpty(firstRightPortrait))
        {
            Sprite rightSprite = Resources.Load<Sprite>($"Portraits/{firstRightPortrait}");
            if (rightSprite != null)
            {
                rightCharacterImage.sprite = rightSprite;
                SetFacingDirection(rightCharacterImage, firstRightDirection);
            }
        }

        return !string.IsNullOrEmpty(firstLeftPortrait);
    }

    public void DisplayNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndConversation();
            return;
        }

        DialogueLine line = dialogueQueue.Dequeue();

        // Cambio de fondo si el JSON lo indica
        if (!string.IsNullOrEmpty(line.backgroundChange))
        {
            Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{line.backgroundChange}");
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
            }
        }

        // Cambio de retrato del personaje si el JSON lo indica
        Sprite portraitSprite = Resources.Load<Sprite>($"Portraits/{line.portrait}");

        if (portraitSprite != null)
        {
            if (line.position == "left")
            {
                leftCharacterImage.sprite = portraitSprite;
                SetFacingDirection(leftCharacterImage, line.facingDirection);
            }
            else if (line.position == "right")
            {
                rightCharacterImage.sprite = portraitSprite;
                SetFacingDirection(rightCharacterImage, line.facingDirection);
            }
        }

        // Ajuste de opacidad: el que habla se mantiene con tono normal, el otro se oscurece
        Color activeColor = new Color(1f, 1f, 1f, 1f);
        Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        // Activación de música y cambio si el JSON lo indica 
        if (!string.IsNullOrEmpty(line.music))
        {
            MusicManager.Instance.PlayMusic(line.music);
        }

        // Si el personaje que habla está a la izquierda
        if (line.position == "left")
        {
            leftCharacterImage.color = activeColor;
            rightCharacterImage.color = inactiveColor;
            leftconversationPanel.SetActive(true);
            rightconversationPanel.SetActive(false);
            leftdialogueText.gameObject.SetActive(true);
            rightdialogueText.gameObject.SetActive(false);
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            textoUIActual = leftdialogueText;
            textoCompletoActual = line.text;
            typingCoroutine = StartCoroutine(EscribirTextoProgresivo(textoUIActual, textoCompletoActual));
        }

        // Si el personaje que habla está a la derecha
        else
        {
            rightCharacterImage.color = activeColor;
            leftCharacterImage.color = inactiveColor;
            leftconversationPanel.SetActive(false);
            rightconversationPanel.SetActive(true);
            leftdialogueText.gameObject.SetActive(false);
            rightdialogueText.gameObject.SetActive(true);
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            textoUIActual = rightdialogueText;
            textoCompletoActual = line.text;
            typingCoroutine = StartCoroutine(EscribirTextoProgresivo(textoUIActual, textoCompletoActual));
        }
    }

    // Para la corrutina y muestra el texto completo
    private void SkipToFullText()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (textoUIActual != null)
            textoUIActual.text = textoCompletoActual;

        isTyping = false;
    }

    private void SetFacingDirection(Image characterImage, string direction)
    {
        if (direction == "right")
        {
            characterImage.rectTransform.localScale = new Vector3(-1, 1, 1); // Invertir horizontalmente
        }
        else
        {
            characterImage.rectTransform.localScale = new Vector3(1, 1, 1); // Mantener la orientación normal
        }
    }

    private void EndConversation()
    {
        //Ocultar todos los elementos
        leftconversationPanel.SetActive(false);
        rightconversationPanel.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
        leftdialogueText.gameObject.SetActive(false);
        rightdialogueText.gameObject.SetActive(false);
        leftCharacterImage.gameObject.SetActive(false);
        rightCharacterImage.gameObject.SetActive(false);
        isConversationActive = false;
        // Si la conversación tiene una recompensa (objeto), se añade al almacén
        if (!string.IsNullOrEmpty(rewardActual.nombre))
        {
            AlmacenObjetos.Instance.AñadirObjeto(rewardActual);
            Debug.Log($"Recompensa otorgada: {rewardActual.nombre}");
            rewardActual = null;
        }
        onComplete?.Invoke();
    }

    // Velocidad del texto según las preferencias del jugador
    private float GetDelayPorVelocidad()
    {
        string velocidad = PlayerPrefs.GetString("VelocidadTexto", "Normal");

        switch (velocidad)
        {
            case "Lenta": return 0.1f;
            case "Rapida": return 0f;
            default: return 0.05f;
        }
    }

    // Corrutina para escribir texto de forma no instantánea
    private IEnumerator EscribirTextoProgresivo(TextMeshProUGUI textoUI, string textoCompleto)
    {
        isTyping = true;
        textoUI.text = "";
        float delay = GetDelayPorVelocidad();

        foreach (char letra in textoCompleto)
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
    }

    // Comprueba si hay una visita en la casilla que se le pasa
    public VisitData BuscarVisitaEnCoordenada(float x, float y)
    {
        return chapterVisits?.visits.Find(v => v.x == x && v.y == y);
    }

    // Diálogo de retirada para personajes no genéricos
    public void StartRetreatQuote(string nombre, System.Action onComplete)
    {
        Debug.Log($"Intentando cargar diálogo de retirada para: {nombre}");
        this.onComplete = onComplete;

        TextAsset jsonAsset = Resources.Load<TextAsset>("Dialogues/RetreatQuotes");
        if (jsonAsset == null)
        {
            Debug.LogError("No se encontró el archivo JSON: RetreatQuotes");
            onComplete?.Invoke();
            return;
        }

        RetreatQuoteList quoteList = JsonUtility.FromJson<RetreatQuoteList>(jsonAsset.text);
        RetreatQuoteEntry quote = quoteList.quotes.Find(q => q.character == nombre);

        if (quote == null)
        {
            Debug.LogWarning($"No se encontró diálogo de retirada para el personaje: {nombre}");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"Diálogo de retirada encontrado para {nombre}, líneas: {quote.dialogue.Count}");

        dialogueQueue.Clear();
        foreach (DialogueLine line in quote.dialogue)
        {
            dialogueQueue.Enqueue(line);
        }

        bool hayIzquierda = PreloadInitialPortraits(quote.dialogue);
        bool hayFondo = quote.dialogue.Any(line => !string.IsNullOrEmpty(line.backgroundChange));

        backgroundImage.gameObject.SetActive(hayFondo);
        leftCharacterImage.gameObject.SetActive(hayIzquierda);
        rightCharacterImage.gameObject.SetActive(true);

        isConversationActive = true;
        DisplayNextLine();
    }

    // Conversacion de combate al enfrentarse a un jefe enemigo con la unidad adecuada
    public void StartFightConversation(string attackerName, string chapterNumber, System.Action onComplete)
    {
        string jsonFileName = $"{chapterNumber}Boss{attackerName}";
        Debug.Log($"Intentando cargar conversación de jefe: {jsonFileName}");

        this.onComplete = onComplete;

        TextAsset jsonAsset = Resources.Load<TextAsset>($"Dialogues/{jsonFileName}");

        // Si no hay conversación, se continúa normalmente
        if (jsonAsset == null)
        {
            Debug.LogWarning($"No se encontró conversación de jefe para {attackerName} atacando al jefe en capítulo {chapterNumber}.");
            onComplete?.Invoke(); 
            return;
        }

        FightDialogueData conversationData = JsonUtility.FromJson<FightDialogueData>(jsonAsset.text);

        // Se verifica si ya se ha visto la conversación, si ya se ha visto, termina sin hacer nada
        if (conversationSeenTracker.ContainsKey(jsonFileName) && conversationSeenTracker[jsonFileName])
        {
            onComplete?.Invoke();
            return;
        }

        // Se marca la conversación como vista en el diccionario para evitar repeticiones
        conversationSeenTracker[jsonFileName] = true;

        dialogueQueue.Clear();
        foreach (DialogueLine line in conversationData.dialogue)
        {
            dialogueQueue.Enqueue(line);
        }

        bool hayIzquierda = PreloadInitialPortraits(conversationData.dialogue);
        bool hayCambioDeFondo = conversationData.dialogue.Any(line => !string.IsNullOrEmpty(line.backgroundChange));

        backgroundImage.gameObject.SetActive(hayCambioDeFondo);
        leftCharacterImage.gameObject.SetActive(hayIzquierda);
        rightCharacterImage.gameObject.SetActive(true);

        isConversationActive = true;
        DisplayNextLine();
    }
}



[System.Serializable]
public class ConversationData
{
    public List<DialogueLine> dialogue;
    public Objeto reward;
}


[System.Serializable]
public class DialogueLine
{
    public string backgroundChange;
    public string character;
    public string portrait;
    public string position;
    public string facingDirection;
    public string text;
    public string music;
}

[System.Serializable]
public class FightDialogueData
{
    public bool seen;
    public List<DialogueLine> dialogue;
}


[System.Serializable]
public class EventConversation
{
    public int turno;
    public List<DialogueLine> dialogue;
}

[System.Serializable]
public class ChapterEvents
{
    public List<EventConversation> eventConversations;
}

[System.Serializable]
public class VisitData
{
    public float x;
    public float y;
    public string conversation;
    public Objeto reward;
    public bool visited;
}

[System.Serializable]
public class ChapterVisits
{
    public List<VisitData> visits;
}

[System.Serializable]
public class RetreatQuoteEntry
{
    public string character;
    public List<DialogueLine> dialogue;
}

[System.Serializable]
public class RetreatQuoteList
{
    public List<RetreatQuoteEntry> quotes;
}