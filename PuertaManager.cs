using UnityEngine;

public class PuertaManager : MonoBehaviour
{
    public static Puerta puertaRotaActual;

    public static void NotificarPuertaRota(Puerta puerta)
    {
        if (puerta != null)
        {
            puertaRotaActual = puerta;
            Debug.Log($"Puerta rota notificada: {puerta.name}");
        }
    }

    public static Puerta ObtenerPuertaRota()
    {
        return puertaRotaActual;
    }
}
