using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContinueMenu : MonoBehaviour
{
    [Header("References UI")]
    [SerializeField] private Transform levelList;
    [SerializeField] private GameObject levelPrefab;
    [SerializeField] private TextMeshProUGUI levelDescription;

    private List<UnlockedChapter> unlockedChapters;
    private List<ChapterStatus> chapterStatus;

    private List<GameObject> instantiatedLevels = new();
    private int currentSelectionIndex = 0;
    private int currentPageStartIndex = 0;

    private const int pageSize = 10; //Número de niveles por página

    private bool isActive = false;

    public void OpenContinueMenu()
    {
        isActive = true;
        gameObject.SetActive(true);
        LoadLevels();
        UpdateLevelSelection();
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            if (currentSelectionIndex < Mathf.Min(pageSize - 1, instantiatedLevels.Count - 1))
            {
                currentSelectionIndex++;
            }
            else if (currentPageStartIndex + pageSize < instantiatedLevels.Count)
            {
                currentPageStartIndex += pageSize;
                currentSelectionIndex = 0;
                LoadLevels();
            }
            UpdateLevelSelection();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.movimientoMenu);
            if (currentSelectionIndex > 0)
            {
                currentSelectionIndex--;
            }
            else if (currentPageStartIndex > 0)
            {
                currentPageStartIndex -= pageSize;
                currentSelectionIndex = 0;
                LoadLevels();
            }
            UpdateLevelSelection();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.cancelar);
            CloseContinueMenu();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.seleccionNivel);
            ConfirmLevel();
        }
    }

    private void LoadLevels()
    {
        // Limpieza para que no se dupliquen
        foreach (var level in instantiatedLevels)
        {
            Destroy(level);
        }
        instantiatedLevels.Clear();

        var gameManager = GameManager.Instance;
        unlockedChapters = gameManager.unlockedChapterDataJuego.chapters;
        chapterStatus = gameManager.chapterDataJuego.chapters;

        int visibleLevels = 0;
        for (int i = 0; i < unlockedChapters.Count; i++)
        {
            var chapter = unlockedChapters[i];
            var status = chapterStatus.Find(c => c.chapterName == chapter.chapterName);

            if (chapter.estaDesbloqueado && (status == null || !status.completed))
            {
                if (visibleLevels >= currentPageStartIndex && visibleLevels < currentPageStartIndex + pageSize)
                {
                    GameObject newLevel = Instantiate(levelPrefab, levelList);
                    newLevel.GetComponentInChildren<TextMeshProUGUI>().text = chapter.chapterTitle;

                    var levelItem = newLevel.AddComponent<LevelItem>();
                    levelItem.chapterData = chapter;

                    instantiatedLevels.Add(newLevel);
                }
                visibleLevels++;
            }
        }
    }

    private void UpdateLevelSelection()
    {
        for (int i = 0; i < instantiatedLevels.Count; i++)
        {
            var textComponent = instantiatedLevels[i].GetComponentInChildren<TextMeshProUGUI>();
            if (i == currentSelectionIndex)
            {
                textComponent.color = Color.yellow; // Seleccionado
                UpdateDescription(i);
            }
            else
            {
                textComponent.color = Color.white; // No seleccionado
            }
        }
    }

    private void UpdateDescription(int index)
    {
        if (index >= 0 && index < instantiatedLevels.Count)
        {
            var levelItem = instantiatedLevels[index].GetComponent<LevelItem>();
            levelDescription.text = levelItem.chapterData.description;
        }
    }

    private void ConfirmLevel()
    {
        var selectedLevelItem = instantiatedLevels[currentSelectionIndex].GetComponent<LevelItem>();
        if (selectedLevelItem != null)
        {
            string chapterName = selectedLevelItem.chapterData.chapterName;
            GameManager.Instance.currentChapter = chapterName; //Se actualiza la variable global
            Debug.Log("Capítulo confirmado: " + chapterName);
            SceneLoader.Instance.LoadScene("ChapterScene");
        }
        else
        {
            Debug.LogWarning("No se encontró LevelItem en el nivel seleccionado.");
        }
    }

    private void CloseContinueMenu()
    {
        isActive = false;
        gameObject.SetActive(false);
        CampManager.Instance.isMenuActive = true;
    }
}
