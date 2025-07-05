using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class AlmacenObjetos : MonoBehaviour
{
    public static AlmacenObjetos Instance { get; private set; }
    public List<Objeto> objetosAlmacenados = new();

    private string almacenSavePath; //Archivo json donde se guardan los objetos

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        
        almacenSavePath = Path.Combine(Application.persistentDataPath, "objetosAlmacenados.json");
    }
    
    // Incluir objeto en el almacen
    public void AñadirObjeto(Objeto obj)
    {
        if (!objetosAlmacenados.Contains(obj))
            objetosAlmacenados.Add(obj);
    }

    // Sacar objeto del almacen
    public void EliminarObjeto(Objeto obj)
    {
        if (objetosAlmacenados.Contains(obj))
            objetosAlmacenados.Remove(obj);
    }

    public List<Objeto> ObtenerObjetos()
    {
        return objetosAlmacenados;
    }
    
    // Guardado de objetos almacenados, que se usa al guardar la partida
    public void GuardarObjetos()
    {
        string objetosJson = JsonConvert.SerializeObject(objetosAlmacenados, Formatting.Indented);
        File.WriteAllText(almacenSavePath, objetosJson);
        Debug.Log("Objetos almacenados guardados.");
    }

    // Carga de objetos almacenados, usada al cargar una partida
    public void CargarObjetos()
    {
        if (File.Exists(almacenSavePath))
        {
            string objetosJson = File.ReadAllText(almacenSavePath);
            objetosAlmacenados = JsonConvert.DeserializeObject<List<Objeto>>(objetosJson);
            Debug.Log("Objetos almacenados cargados.");
        }
        else
        {
            Debug.Log("No se encontró el archivo de objetos almacenados.");
        }
    }
}
