using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BootManager : MonoBehaviour
{
    [SerializeField] private GameObject pressAnyKeyUI;
    [SerializeField] private GameObject mainMenuUI;

    [Header("Sequential Fade-in Settings")]
    [SerializeField] private Image[] fadeImages;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float delayBetweenFades = 0.5f;
    private bool readyForInput = false;
    private TextMeshProUGUI pressText;

    private void Start()
    {
        //Esconder HUD durante la animación
        mainMenuUI.SetActive(false);
        pressAnyKeyUI.SetActive(false);

        pressText = pressAnyKeyUI.GetComponent<TextMeshProUGUI>();
        foreach (var img in fadeImages)
        {
            var c = img.color;
            c.a = 0f;
            img.color = c;
        }
        MusicManager.Instance.PlayMusic("OverWorldOrchestra");
        //Empezar corrutina de animacion
        StartCoroutine(PlayBootSequence());
    }

    private System.Collections.IEnumerator PlayBootSequence()
    {
        yield return StartCoroutine(FadeImagesSequence());

        pressAnyKeyUI.SetActive(true);
        readyForInput = true;
        StartCoroutine(BlinkText());
    }

     private IEnumerator FadeImagesSequence()
    {
        for (int i = 0; i < fadeImages.Length; i++)
        {
            yield return StartCoroutine(FadeInImage(fadeImages[i]));
            yield return new WaitForSeconds(delayBetweenFades);
        }
    }

    private IEnumerator BlinkText()
    {
        while (readyForInput)
        {
            float timer = 0f;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / 0.5f);
                SetTextAlpha(alpha);
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            timer = 0f;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, timer / 0.5f);
                SetTextAlpha(alpha);
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SetTextAlpha(float alpha)
    {
        Color c = pressText.color;
        c.a = alpha;
        pressText.color = c;
    }

    private IEnumerator FadeInImage(Image img)
    {
        float timer = 0f;
        Color color = img.color;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / fadeDuration);
            img.color = color;
            yield return null;
        }
        color.a = 1f;
        img.color = color;
    }

    private void Update()
    {
        if (readyForInput && Input.anyKeyDown)
        {
            pressAnyKeyUI.SetActive(false);
            readyForInput = false;
            AudioManager.Instance.ReproducirEfecto(AudioManager.Instance.confirmar);
            mainMenuUI.SetActive(true);
        }
    }
}
