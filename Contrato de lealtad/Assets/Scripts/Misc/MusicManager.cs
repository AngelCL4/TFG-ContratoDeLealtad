using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource currentSource;
    private AudioSource nextSource;

    public float fadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Dos AudioSource para transiciones
        currentSource = gameObject.AddComponent<AudioSource>();
        nextSource = gameObject.AddComponent<AudioSource>();

        currentSource.loop = true;
        nextSource.loop = true;

        currentSource.playOnAwake = false;
        nextSource.playOnAwake = false;

        // Aplicación del volumen, por defecto 0.5f
        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
        float volumenMusica2 = PlayerPrefs.GetFloat("VolumenMusica2", 0.5f);

        currentSource.volume = volumenMusica;
        nextSource.volume = volumenMusica2;
    }

    public void ActualizarVolumenMusica(float volumen1, float volumen2)
    {
        currentSource.volume = volumen1;
        nextSource.volume = volumen2;
    }

    public void PlayMusic(string musicName)
    {
        if (string.IsNullOrEmpty(musicName)) return;

        AudioClip clip = Resources.Load<AudioClip>($"Sounds/Music/{musicName}");
        if (clip == null)
        {
            Debug.LogWarning($"No se encontró la pista de música: {musicName}");
            return;
        }

        if (currentSource.clip == clip) //La misma canción ya se está reproduciendo
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TransitionToNewMusic(clip));
    }

    private IEnumerator TransitionToNewMusic(AudioClip newClip)
    {
        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        float timer = 0f;

        float currentTarget = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
        float nextTarget = PlayerPrefs.GetFloat("VolumenMusica2", 0.5f);

        float initialCurrentVolume = currentSource.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            currentSource.volume = Mathf.Lerp(initialCurrentVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, nextTarget, t);

            yield return null;
        }

        currentSource.Stop();

        // Intercambio de AudioSources
        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        // Asegurar los volúmenes finales
        currentSource.volume = currentTarget;
        nextSource.volume = 0f;
    }
}
