using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialDatabase", menuName = "Juego/TutorialDatabase")]
public class TutorialDatabase : ScriptableObject
{
    public List<TutorialItem> tutoriales;
}

[System.Serializable]
public class TutorialItem
{
    public string titulo;
    
    [TextArea(3, 10)]
    public string descripcion;

    public bool desbloqueadoDesdeInicio = true;

    public string id;
}