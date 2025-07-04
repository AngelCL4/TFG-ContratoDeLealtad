using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public Image backgroundImage;
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    public AudioSource musicSource;

    private Queue<string> sentences;
    private System.Action onComplete;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;  

    private void Awake()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(string jsonFileName, System.Action onComplete)
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Dialogues/{jsonFileName}");
        if (jsonAsset == null)
        {
            Debug.LogError($"No se encontró el archivo JSON: {jsonFileName}");
            onComplete?.Invoke();
            return;
        }

        DialogueData dialogueData = JsonUtility.FromJson<DialogueData>(jsonAsset.text);

        // Cambio de fondo
        Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{dialogueData.background}");
        if (bgSprite != null)
        {
            backgroundImage.sprite = bgSprite;
        }

        // Preparacion de las líneas de diálogo
        this.onComplete = onComplete;
        sentences.Clear();
        foreach (string sentence in dialogueData.lines)
        {
            sentences.Enqueue(sentence);
        }
        
        // Activación de la música
        if (!string.IsNullOrEmpty(dialogueData.music))
        {
            MusicManager.Instance.PlayMusic(dialogueData.music);
        }

        dialoguePanel.SetActive(true);
        isDialogueActive = true;
        DisplayNextSentence();
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Se muestra la línea completa inmediatamente
                SkipToFullText();
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
    
        typingCoroutine = StartCoroutine(EscribirTextoProgresivo(dialogueText, sentence));
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        isDialogueActive = false;
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

    // Para la corrutina y muestra el texto completo
    private void SkipToFullText()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = fullSentenceBeingTyped;
        isTyping = false;
    }

    private string fullSentenceBeingTyped = "";

    // Corrutina para escribir texto de forma no instantánea
    private IEnumerator EscribirTextoProgresivo(TextMeshProUGUI textoUI, string textoCompleto)
    {
        isTyping = true;
        textoUI.text = "";
        fullSentenceBeingTyped = textoCompleto;

        float delay = GetDelayPorVelocidad();

        foreach (char letra in textoCompleto)
        {
            textoUI.text += letra;
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
    }

}

[System.Serializable]
public class DialogueData
{
    public string background;
    public string music;
    public List<string> lines;
}
