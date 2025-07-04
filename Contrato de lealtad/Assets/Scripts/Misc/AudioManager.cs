using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource efectosSource;

    public AudioClip movimientoMenu;
    public AudioClip confirmar;
    public AudioClip cancelar;
    public AudioClip seleccionNivel;
    public AudioClip golpe;
    public AudioClip golpeCrítico;
    public AudioClip subirNivel;
    public AudioClip curar;

    public static AudioManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Dos volúmenes de música necesarios para el efecto de Fade In al cambiar de una canción a otra
        // Volumen a 0.5f por defecto
        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 0.5f);
        float volumenMusica2 = PlayerPrefs.GetFloat("VolumenMusica2", 0.5f);
        float volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 0.5f);

        efectosSource.volume = volumenEfectos;
    }

    public void CambiarVolumenMusica(float valor)
    {
        PlayerPrefs.SetFloat("VolumenMusica", valor);
        PlayerPrefs.SetFloat("VolumenMusica2", valor);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ActualizarVolumenMusica(valor, valor);
        }
    }

    public void CambiarVolumenEfectos(float valor)
    {
        efectosSource.volume = valor;
        PlayerPrefs.SetFloat("VolumenEfectos", valor);
    }

    public void ReproducirEfecto(AudioClip clip)
    {
        efectosSource.PlayOneShot(clip, efectosSource.volume);
    }
}
