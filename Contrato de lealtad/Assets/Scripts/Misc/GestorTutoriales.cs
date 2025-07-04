using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorTutoriales : MonoBehaviour
{
    public static GestorTutoriales Instance;

    private HashSet<string> tutorialesDesbloqueados = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Desbloqueo de los tutoriales disponibles desde el inicio
        foreach (var t in tutorialDatabase.tutoriales)
        {
            if (t.desbloqueadoDesdeInicio)
                tutorialesDesbloqueados.Add(t.id);
        }
    }

    [SerializeField] private TutorialDatabase tutorialDatabase;

    public bool EstaDesbloqueado(string id)
    {
        return tutorialesDesbloqueados.Contains(id);
    }

    public void DesbloquearTutorial(string id)
    {
        if (!tutorialesDesbloqueados.Contains(id))
        {
            tutorialesDesbloqueados.Add(id);
        }
    }
}
