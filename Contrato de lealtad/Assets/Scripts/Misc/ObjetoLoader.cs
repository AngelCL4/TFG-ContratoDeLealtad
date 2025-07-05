using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoLoader : MonoBehaviour
{
    public static List<Objeto> objetosDisponibles;
    public DatosObjetos datos;

    // Cargado de todos los objetos en el archivo json
    void Awake()
    {
        TextAsset jsonData = Resources.Load<TextAsset>("objetos");
        if (jsonData != null)
        {
            datos = JsonUtility.FromJson<DatosObjetos>(jsonData.text);
            objetosDisponibles = new List<Objeto>(datos.objetos);
            Debug.Log($"Objetos cargados: {objetosDisponibles.Count}");
        }

        // Cargado de sprites automático
        foreach (var obj in objetosDisponibles)
        {
            obj.icono = Resources.Load<Sprite>(obj.spritePath);
        }
    }
}