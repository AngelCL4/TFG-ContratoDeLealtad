using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class SupportData
{
    public string personajeA;
    public string personajeB;
    public int nivelDesbloqueado;
    public List<int> rangosVistos = new();
}

[System.Serializable]
public class DatosApoyo
{
    public List<SupportData> conversaciones = new List<SupportData>();
}

[System.Serializable]
public class AfectoData
{
    public string personajeA;
    public string personajeB;
    public int puntos;
}

[System.Serializable]
public class ApoyoPendiente
{
    public string personajeA;
    public string personajeB;
    public int puntosRequeridos;
}

[System.Serializable]
public class DesvioPendiente
{
    public string personaje;
    public int rango;
}

public class SupportManager : MonoBehaviour
{
    //Diccionario con desvíos que se desbloquean según cada personaje
    private Dictionary<string, Dictionary<int, string>> desviosPorPersonajeYRango = new Dictionary<string, Dictionary<int, string>>
    {
        { "Lisella", new Dictionary<int, string>
            {
                { 1, "ChapterLisella1" },
                { 2, "ChapterLisella2" }
            }
        },
        { "Aki", new Dictionary<int, string>
            {
                { 1, "ChapterAki1" },
                { 2, "ChapterAki2" }
            }
        },
        { "Alena", new Dictionary<int, string>
            {
                { 1, "ChapterAlena1" },
                { 2, "ChapterAlena2" }
            }
        },
        { "Avarel", new Dictionary<int, string>
            {
                { 1, "ChapterAvarel1" },
                { 2, "ChapterAvarel2" }
            }
        },
        { "Aviria", new Dictionary<int, string>
            {
                { 1, "ChapterAviria1" },
                { 2, "ChapterAviria2" }
            }
        },
        { "Celsia", new Dictionary<int, string>
            {
                { 1, "ChapterCelsia1" },
                { 2, "ChapterCelsia2" }
            }
        },
        { "Eldael", new Dictionary<int, string>
            {
                { 1, "ChapterEldael1" },
                { 2, "ChapterEldael2" }
            }
        },
        { "Elmer", new Dictionary<int, string>
            {
                { 1, "ChapterElmer1" },
                { 2, "ChapterElmer2" }
            }
        },
        { "Fesha", new Dictionary<int, string>
            {
                { 1, "ChapterFesha1" },
                { 2, "ChapterFesha2" }
            }
        },
        { "Handox", new Dictionary<int, string>
            {
                { 1, "ChapterHandox1" },
                { 2, "ChapterHandox2" }
            }
        },
        { "Helxes", new Dictionary<int, string>
            {
                { 1, "ChapterHelxes1" },
                { 2, "ChapterHelxes2" }
            }
        },
        { "Hyugan", new Dictionary<int, string>
            {
                { 1, "ChapterHyugan1" },
                { 2, "ChapterHyugan2" }
            }
        },
        { "Ipris", new Dictionary<int, string>
            {
                { 1, "ChapterIpris1" },
                { 2, "ChapterIpris2" }
            }
        },
        { "Irina", new Dictionary<int, string>
            {
                { 1, "ChapterIrina1" },
                { 2, "ChapterIrina2" }
            }
        },
        { "Jadyr", new Dictionary<int, string>
            {
                { 1, "ChapterJadyr1" },
                { 2, "ChapterJadyr2" }
            }
        },
        { "Kami", new Dictionary<int, string>
            {
                { 1, "ChapterKami1" },
                { 2, "ChapterKami2" }
            }
        },
        { "Kiara", new Dictionary<int, string>
            {
                { 1, "ChapterKiara1" },
                { 2, "ChapterKiara2" }
            }
        },
        { "Kolax", new Dictionary<int, string>
            {
                { 1, "ChapterKolax1" },
                { 2, "ChapterKolax2" }
            }
        },
        { "Letan", new Dictionary<int, string>
            {
                { 1, "ChapterLetan1" },
                { 2, "ChapterLetan2" }
            }
        },
        { "Lilen", new Dictionary<int, string>
            {
                { 1, "ChapterLilen1" },
                { 2, "ChapterLilen2" }
            }
        },
        { "Loreas", new Dictionary<int, string>
            {
                { 1, "ChapterLoreas1" },
                { 2, "ChapterLoreas2" }
            }
        },
        { "Lumina", new Dictionary<int, string>
            {
                { 1, "ChapterLumina1" },
                { 2, "ChapterLumina2" }
            }
        },
        { "Malen", new Dictionary<int, string>
            {
                { 1, "ChapterMalen1" },
                { 2, "ChapterMalen2" }
            }
        },
        { "Mirah", new Dictionary<int, string>
            {
                { 1, "ChapterMirah1" },
                { 2, "ChapterMirah2" }
            }
        },
        { "Nandris", new Dictionary<int, string>
            {
                { 1, "ChapterNandris1" },
                { 2, "ChapterNandris2" }
            }
        },
        { "Noru", new Dictionary<int, string>
            {
                { 1, "ChapterNoru1" },
                { 2, "ChapterNoru2" }
            }
        },
        { "Olyr", new Dictionary<int, string>
            {
                { 1, "ChapterOlyr1" },
                { 2, "ChapterOlyr2" }
            }
        },
        { "Onirie", new Dictionary<int, string>
            {
                { 1, "ChapterOnirie1" },
                { 2, "ChapterOnirie2" }
            }
        },
        { "Ozane", new Dictionary<int, string>
            {
                { 1, "ChapterOzane1" },
                { 2, "ChapterOzane2" }
            }
        },
        { "Phesen", new Dictionary<int, string>
            {
                { 1, "ChapterPhesen1" },
                { 2, "ChapterPhesen2" }
            }
        },
        { "Quaill", new Dictionary<int, string>
            {
                { 1, "ChapterQuaill1" },
                { 2, "ChapterQuaill2" }
            }
        },
        { "Seiha", new Dictionary<int, string>
            {
                { 1, "ChapterSeiha1" },
                { 2, "ChapterSeiha2" }
            }
        },
        { "Selden", new Dictionary<int, string>
            {
                { 1, "ChapterSelden1" },
                { 2, "ChapterSelden2" }
            }
        },
        { "Siel", new Dictionary<int, string>
            {
                { 1, "ChapterSiel1" },
                { 2, "ChapterSiel2" }
            }
        },
        { "Tilpha", new Dictionary<int, string>
            {
                { 1, "ChapterTilpha1" },
                { 2, "ChapterTilpha2" }
            }
        },
        { "Umia", new Dictionary<int, string>
            {
                { 1, "ChapterUmia1" },
                { 2, "ChapterUmia2" }
            }
        },
        { "Vila", new Dictionary<int, string>
            {
                { 1, "ChapterVila1" },
                { 2, "ChapterVila2" }
            }
        },
        { "Waishe", new Dictionary<int, string>
            {
                { 1, "ChapterWaishe1" },
                { 2, "ChapterWaishe2" }
            }
        },
        { "Zisian", new Dictionary<int, string>
            {
                { 1, "ChapterZisian1" },
                { 2, "ChapterZisian2" }
            }
        },
    };

    public static SupportManager Instance { get; private set; }
    private Dictionary<(string, string), int> afectos = new();

    private string basePath = "apoyos"; // Archivo base (nunca se sobrescribe)
    private string savePath => Path.Combine(Application.persistentDataPath, "apoyos_partida.json"); // Archivo de guardado
    private string afectoPath => Path.Combine(Application.persistentDataPath, "afectos_partida.json");
    private List<ApoyoPendiente> apoyosPendientes = new(); // Apoyos esperando desbloqueo
    private string pendientesPath => Path.Combine(Application.persistentDataPath, "apoyos_pendientes.json");
    private List<DesvioPendiente> desviosPendientes = new(); // Desvíos esperando desbloqueo
    private string desviosPendientesPath => Path.Combine(Application.persistentDataPath, "desvios_pendientes.json");
    private DatosApoyo datosApoyo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CargarDatos();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    private void CargarDatos()
    {
        //Cargar datos de apoyo si existe archivo guardado, si no del archivo base
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            datosApoyo = JsonConvert.DeserializeObject<DatosApoyo>(json);
            Debug.Log("Datos de apoyo cargados desde partida: " + datosApoyo.conversaciones.Count);
        }
        else
        {
            TextAsset baseJson = Resources.Load<TextAsset>(basePath);
            if (baseJson != null)
            {
                datosApoyo = JsonConvert.DeserializeObject<DatosApoyo>(baseJson.text);
                Debug.Log("Datos de apoyo cargados desde base: " + datosApoyo.conversaciones.Count);
            }
            else
            {
                Debug.LogWarning("No se encontró el archivo base de apoyos en Resources.");
                datosApoyo = new DatosApoyo();
            }
        }

        // Cargado de puntos de afecto
        afectos = new Dictionary<(string, string), int>();
        if (File.Exists(afectoPath))
        {
            string afectoJson = File.ReadAllText(afectoPath);
            var afectosList = JsonConvert.DeserializeObject<List<AfectoData>>(afectoJson);
            foreach (var a in afectosList)
            {
                var key = (a.personajeA, a.personajeB);
                afectos[key] = a.puntos;
            }
            Debug.Log("Datos de afectos cargados: " + afectos.Count);
        }
    }

    //Guardado de datos de apoyo y afecto
    public void GuardarDatosApoyo()
    {
        string json = JsonConvert.SerializeObject(datosApoyo, Formatting.Indented);
        File.WriteAllText(savePath, json);
    }

    public void GuardarDatosAfecto()
    {
        List<AfectoData> afectosList = new List<AfectoData>();
        foreach (var kvp in afectos)
        {
            afectosList.Add(new AfectoData
            {
                personajeA = kvp.Key.Item1,
                personajeB = kvp.Key.Item2,
                puntos = kvp.Value
            });
        }

        string afectoJson = JsonConvert.SerializeObject(afectosList, Formatting.Indented);
        File.WriteAllText(afectoPath, afectoJson);
    }

    public void BorrarDatosGuardados()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Datos de apoyo de partida eliminados.");
        }

        if (File.Exists(afectoPath))
        {
            File.Delete(afectoPath);
            Debug.Log("Datos de afectos eliminados.");
        }

        // Reseteo de los datos en memoria cargando desde el archivo base
        TextAsset baseJson = Resources.Load<TextAsset>(basePath);
        if (baseJson != null)
        {
            datosApoyo = JsonConvert.DeserializeObject<DatosApoyo>(baseJson.text);
            Debug.Log("Datos de apoyo restaurados desde base");
        }
        else
        {
            datosApoyo = new DatosApoyo();
            Debug.LogWarning("Archivo base de apoyos no encontrado. Datos reiniciados.");
        }

        afectos.Clear(); // Reseteo de afectos
    }

    //Obtener apoyos de un personaje
    public List<string> GetApoyos(string personaje)
    {
        List<string> apoyos = new List<string>();

        foreach (var apoyo in datosApoyo.conversaciones)
        {
            if (apoyo.personajeA == personaje)
                apoyos.Add(apoyo.personajeB);
            else if (apoyo.personajeB == personaje)
                apoyos.Add(apoyo.personajeA);
        }

        return apoyos;
    }

    //Obtener nivel de apoyo entre una pareja de personajes
    public int GetNivelApoyo(string personaje, string otro)
    {
        foreach (var apoyo in datosApoyo.conversaciones)
        {
            if ((apoyo.personajeA == personaje && apoyo.personajeB == otro) ||
                (apoyo.personajeB == personaje && apoyo.personajeA == otro))
            {
                return apoyo.nivelDesbloqueado;
            }
        }
        return 0;
    }

    //Desbloquear apoyo entre dos personajes
    public void DesbloquearApoyo(string personaje, string otro)
    {
        var apoyo = datosApoyo.conversaciones.Find(a =>
            (a.personajeA == personaje && a.personajeB == otro) ||
            (a.personajeA == otro && a.personajeB == personaje));

        if (apoyo == null)
        {
            apoyo = new SupportData
            {
                personajeA = personaje,
                personajeB = otro,
                nivelDesbloqueado = 1,
                rangosVistos = new List<int>()
            };
            datosApoyo.conversaciones.Add(apoyo);
        }
        else
        {
            if (apoyo.nivelDesbloqueado < 3)
                apoyo.nivelDesbloqueado++;
        }

        GuardarDatosApoyo();
    }

    //Añadir puntos de afecto entre una pareja de personajes
    public void AñadirAfecto(string personajeA, string personajeB, int puntos)
    {
        // Ordenar la pareja siempre igual
        var key = (personajeA.CompareTo(personajeB) < 0) ? (personajeA, personajeB) : (personajeB, personajeA);

        // Si tienen una conversación pendiente no ganan afecto
        if (TieneConversacionPendiente(personajeA, personajeB))
        {
            Debug.Log($"{key.Item1} y {key.Item2} tienen un apoyo pendiente de ver.");
            return; 

        }

        if (!afectos.ContainsKey(key))
            afectos[key] = 0;

        afectos[key] += puntos;

        Debug.Log($"{key.Item1} y {key.Item2} ganan {puntos} puntos de afecto. Total: {afectos[key]}");

        // Comprobar si toca desbloquear un apoyo
        if (afectos[key] == 20 || afectos[key] == 40 || afectos[key] == 60)
        {
            if (TieneApoyo(personajeA, personajeB))
            {
                if (PuedeDesbloquearApoyo(afectos[key]))
                {
                    Debug.Log($"Desbloqueando apoyo entre {key.Item1} y {key.Item2}");
                    DesbloquearApoyo(personajeA, personajeB);
                }
                else
                {
                    // Guardar como apoyo pendiente si no se puede desbloquear de momento
                    apoyosPendientes.Add(new ApoyoPendiente
                    {
                        personajeA = personajeA,
                        personajeB = personajeB,
                        puntosRequeridos = afectos[key]
                    });
                    Debug.Log($"Apoyo pendiente entre {key.Item1} y {key.Item2} guardado");
                }
            }
        }

        GuardarDatosAfecto();
    }

    //Comprobar si se puede desbloquear apoyo en base al capítulo completado
    private bool PuedeDesbloquearApoyo(int puntos)
    {
        if (puntos == 20) return true;
        if (puntos == 40) return /*GameManager.Instance.chapterDataJuego.chapters.Find(c => c.chapterName == "Chapter4")?.completed ==*/ true;
        if (puntos == 60) return /*GameManager.Instance.chapterDataJuego.chapters.Find(c => c.chapterName == "Chapter8")?.completed ==*/ true;
        return false;
    }

    //Otorgar afecto entre todas las unidades del ejército desplegadas al terminar el nivel
    public void AfectoAlFinalizarNivel(List<string> nombresUnidades)
    {
        for (int i = 0; i < nombresUnidades.Count; i++)
        {
            for (int j = i + 1; j < nombresUnidades.Count; j++)
            {
                AñadirAfecto(nombresUnidades[i], nombresUnidades[j], 2);
            }
        }
    }

    // Comprobar si dos unidades tiene apoyo entre ellas
    public bool TieneApoyo(string personajeA, string personajeB)
    {
        foreach (var apoyo in datosApoyo.conversaciones)
        {
            if ((apoyo.personajeA == personajeA && apoyo.personajeB == personajeB) ||
                (apoyo.personajeA == personajeB && apoyo.personajeB == personajeA))
            {
                return true;
            }
        }
        return false;
    }

    // Marcar una conversacion como vista
    public void MarcarConversacionVista(string personajeA, string personajeB, int rango)
    {
        foreach (var c in datosApoyo.conversaciones)
        {
            if ((c.personajeA == personajeA && c.personajeB == personajeB) ||
                (c.personajeA == personajeB && c.personajeB == personajeA))
            {
                // Solo añadir si no está ya marcado como visto
                if (!c.rangosVistos.Contains(rango))
                {
                    c.rangosVistos.Add(rango);
                    GuardarDatosApoyo();

                    // Intenar desbloquear desvío si uno de los personajes es Demain
                    if (personajeA == "Demain")
                    {
                        IntentarDesbloquearDesvio(personajeB, rango);
                    }
                    else if (personajeB == "Demain")
                    {
                        IntentarDesbloquearDesvio(personajeA, rango);
                    }
                }
                return;
            }
        }
    }

    public bool ConversacionVista(string personajeA, string personajeB, int rango)
    {
        foreach (var c in datosApoyo.conversaciones)
        {
            if ((c.personajeA == personajeA && c.personajeB == personajeB) ||
                (c.personajeA == personajeB && c.personajeB == personajeA))
            {
                return c.rangosVistos.Contains(rango);
            }
        }
        return false;
    }

    public List<Unidad> GetPersonajesReclutados(List<Unidad> unidades)
    {
        List<Unidad> reclutados = new List<Unidad>();
        foreach (var unidad in unidades)
        {
            if (unidad.estado == "Reclutado")
            {
                reclutados.Add(unidad);
            }
        }
        return reclutados;
    }

    private bool TieneConversacionPendiente(string personajeA, string personajeB)
    {
        foreach (var c in datosApoyo.conversaciones)
        {
            if ((c.personajeA == personajeA && c.personajeB == personajeB) ||
                (c.personajeA == personajeB && c.personajeB == personajeA))
            {
                // Si el nivel desbloqueado es mayor que el número de conversaciones vistas, hay una pendiente
                return c.nivelDesbloqueado > c.rangosVistos.Count;
            }
        }
        return false;
    }

    public void IntentarDesbloquearDesvio(string personaje, int rango)
    {
        if (desviosPorPersonajeYRango.ContainsKey(personaje) && desviosPorPersonajeYRango[personaje].ContainsKey(rango))
        {
            bool puedeDesbloquear = false;

            //Comprobar si se pueden desbloquear desvíos en base a los capítulos completados
            if (rango == 1)
                puedeDesbloquear = GameManager.Instance.chapterDataJuego.chapters.Find(c => c.chapterName == "Chapter4")?.completed == true;
            else if (rango == 2)
                puedeDesbloquear = GameManager.Instance.chapterDataJuego.chapters.Find(c => c.chapterName == "Chapter8")?.completed == true;

            if (puedeDesbloquear)
            {
                string capitulo = desviosPorPersonajeYRango[personaje][rango];
                DesbloquearDesvio(capitulo);
            }
            else
            {
                desviosPendientes.Add(new DesvioPendiente
                {
                    personaje = personaje,
                    rango = rango
                });
                Debug.Log($"Desvío de {personaje} rango {rango} pendiente hasta completar capítulo.");
            }
        }
    }

    // Modificar el archivo de GameManager para desbloquear el capítulo
    private void DesbloquearDesvio(string nombreCapitulo)
    {
        UnlockedChapter capitulo = GameManager.Instance.unlockedChapterDataJuego.chapters.Find(c => c.chapterName == nombreCapitulo);

        if (capitulo != null)
        {
            //capitulo.estaDesbloqueado = true;
            Debug.Log($"Desvío desbloqueado: {nombreCapitulo}");
        }
        else
        {
            Debug.LogWarning($"No se encontró el capítulo {nombreCapitulo} para desbloquear.");
        }
    }

    public void GuardarApoyosPendientes()
    {
        string json = JsonConvert.SerializeObject(apoyosPendientes, Formatting.Indented);
        File.WriteAllText(pendientesPath, json);
    }

    public void CargarApoyosPendientes()
    {
        if (File.Exists(pendientesPath))
        {
            string pendientesJson = File.ReadAllText(pendientesPath);
            apoyosPendientes = JsonConvert.DeserializeObject<List<ApoyoPendiente>>(pendientesJson);
            Debug.Log($"Apoyos pendientes cargados: {apoyosPendientes.Count}");
        }
    }

    // Comprobación de los apoyos pendientes para desbloquarlos si se puede
    public void RevisarApoyosPendientes()
    {
        List<ApoyoPendiente> desbloqueadosAhora = new();
        
        foreach (var pendiente in apoyosPendientes)
        {
            if (PuedeDesbloquearApoyo(pendiente.puntosRequeridos))
            {
                DesbloquearApoyo(pendiente.personajeA, pendiente.personajeB);
                desbloqueadosAhora.Add(pendiente);
            }
        }

        foreach (var p in desbloqueadosAhora)
            apoyosPendientes.Remove(p);

        if (desbloqueadosAhora.Count > 0)
            GuardarApoyosPendientes();
    }

    public void ResetearApoyosPendientes()
    {
        apoyosPendientes.Clear();
        if (File.Exists(pendientesPath))
        {
            File.Delete(pendientesPath);
            Debug.Log("Apoyos pendientes reseteados al iniciar nueva partida.");
        }
    }

    public void GuardarDesviosPendientes()
    {
        string json = JsonConvert.SerializeObject(desviosPendientes, Formatting.Indented);
        File.WriteAllText(desviosPendientesPath, json);
    }

    public void CargarDesviosPendientes()
    {
        if (File.Exists(desviosPendientesPath))
        {
            string json = File.ReadAllText(desviosPendientesPath);
            desviosPendientes = JsonConvert.DeserializeObject<List<DesvioPendiente>>(json);
            Debug.Log($"📥 Desvíos pendientes cargados: {desviosPendientes.Count}");
        }
    }

    // Comprobación de los desvíos pendientes para desbloquarlos si se puede
    public void RevisarDesviosPendientes()
    {
        List<DesvioPendiente> desbloqueados = new();

        foreach (var d in desviosPendientes)
        {
            if ((d.rango == 1 && GameManager.Instance.chapterDataJuego.chapters.Find(c => c.chapterName == "Chapter4")?.completed == true) ||
                (d.rango == 2 && GameManager.Instance.chapterDataJuego.chapters.Find(c => c.chapterName == "Chapter8")?.completed == true))
            {
                string capitulo = desviosPorPersonajeYRango[d.personaje][d.rango];
                DesbloquearDesvio(capitulo);
                desbloqueados.Add(d);
            }
        }

        foreach (var d in desbloqueados)
            desviosPendientes.Remove(d);

        if (desbloqueados.Count > 0)
            GuardarDesviosPendientes();
    }

    public void ResetearDesviosPendientes()
    {
        desviosPendientes.Clear();
        if (File.Exists(desviosPendientesPath))
        {
            File.Delete(desviosPendientesPath);
            Debug.Log("Desvíos pendientes reseteados al iniciar nueva partida.");
        }
    }
    
}