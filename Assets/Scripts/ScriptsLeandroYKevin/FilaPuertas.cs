using UnityEngine;
using System.Collections.Generic;

public class FilaPuertas : MonoBehaviour
{
    public List<Puerta> puertas = new List<Puerta>();
    public int puertasRealesNecesarias = 1; // Cambiado a 1 para el sistema de líder
    public float espacioEntrePuertas = 2f;

    private static Dictionary<int, List<int>> configuracionesPorFila = new Dictionary<int, List<int>>();
    private static int semillaGlobal = -1;
    private static System.Random randomCompartido;
    private List<int> indicesPuertasReales = new List<int>();
    private int idFila;

    [HideInInspector] public Puerta puertaRota = null;
    public bool TienePuertaRota => puertaRota != null && puertaRota.EstaRota();

    // Evento para notificar cuando una puerta es rota
    public System.Action<Puerta> OnPuertaRota;

    void Start()
    {
        InicializarSemilla();
        ConfigurarFilaDePuertas();
        AsignarFilaPadreAPuertas();
    }

    private void InicializarSemilla()
    {
        if (semillaGlobal == -1)
        {
            semillaGlobal = PlayerPrefs.GetInt("SemillaPuertas", -1);
            if (semillaGlobal == -1)
            {
                semillaGlobal = Random.Range(int.MinValue, int.MaxValue);
                PlayerPrefs.SetInt("SemillaPuertas", semillaGlobal);
            }
            randomCompartido = new System.Random(semillaGlobal);
        }

        idFila = GetInstanceID();
    }

    private void ConfigurarFilaDePuertas()
    {
        if (!configuracionesPorFila.ContainsKey(idFila))
        {
            GenerarNuevaConfiguracion();
            configuracionesPorFila[idFila] = new List<int>(indicesPuertasReales);
        }
        else
        {
            AplicarConfiguracionExistente();
        }
    }

    private void AsignarFilaPadreAPuertas()
    {
        for (int i = 0; i < puertas.Count; i++)
        {
            if (puertas[i] != null)
            {
                puertas[i].filaPadre = this;
            }
        }
    }

    public void NotificarPuertaRota(Puerta puerta)
    {
        if (!TienePuertaRota && puerta.esReal)
        {
            puertaRota = puerta;
            OnPuertaRota?.Invoke(puerta);

            // Notificar al PuertaManager (sistema estático) si existe
            if (typeof(PuertaManager).GetMethod("NotificarPuertaRota") != null)
            {
                PuertaManager.NotificarPuertaRota(puerta);
            }
        }
    }

    private void GenerarNuevaConfiguracion()
    {
        Random.State estadoOriginal = Random.state;
        Random.InitState(System.DateTime.Now.Millisecond + idFila.GetHashCode());

        ConfigurarPuertasAleatorias();

        Random.state = estadoOriginal;

        indicesPuertasReales.Clear();
        for (int i = 0; i < puertas.Count; i++)
        {
            if (puertas[i] != null && puertas[i].esReal)
                indicesPuertasReales.Add(i);
        }
    }

    private void AplicarConfiguracionExistente()
    {
        foreach (Puerta puerta in puertas)
        {
            if (puerta != null)
                puerta.esReal = false;
        }

        foreach (int indice in configuracionesPorFila[idFila])
        {
            if (indice < puertas.Count && puertas[indice] != null)
                puertas[indice].esReal = true;
        }
    }

    private void ConfigurarPuertasAleatorias()
    {
        // Resetear todas las puertas
        foreach (Puerta puerta in puertas)
        {
            if (puerta != null)
                puerta.esReal = false;
        }

        // Asegurarse de que haya al menos una puerta real
        int puertasAsignadas = 0;
        while (puertasAsignadas < puertasRealesNecesarias)
        {
            int i = randomCompartido.Next(0, puertas.Count);
            if (puertas[i] != null && !puertas[i].esReal)
            {
                puertas[i].esReal = true;
                puertasAsignadas++;
            }
        }
    }

    public Puerta ObtenerPuertaReal()
    {
        foreach (Puerta puerta in puertas)
        {
            if (puerta != null && puerta.esReal)
                return puerta;
        }
        return null;
    }

    private void OnDisable()
    {
        if (configuracionesPorFila.ContainsKey(idFila) && !gameObject.scene.isLoaded)
        {
            configuracionesPorFila.Remove(idFila);
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("SemillaPuertas");
    }

    // Método para validar el estado de las puertas
    public void ValidarEstadoPuertas()
    {
        puertas.RemoveAll(p => p == null);
        bool tienePuertaReal = false;

        foreach (Puerta puerta in puertas)
        {
            if (puerta.esReal)
            {
                tienePuertaReal = true;
                break;
            }
        }

        if (!tienePuertaReal && puertas.Count > 0)
        {
            Debug.LogWarning("No se encontró puerta real. Generando nueva configuración.");
            ConfigurarPuertasAleatorias();
        }
    }
}
