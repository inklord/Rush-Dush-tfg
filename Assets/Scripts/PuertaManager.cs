using System;
using UnityEngine;

public class PuertaManager : MonoBehaviour
{
    public static Puerta puertaRotaActual = null;

    public static void RegistrarPuertaRota(Puerta puerta)
    {
        puertaRotaActual = puerta;
    }

    public static Puerta ObtenerPuertaRota()
    {
        return puertaRotaActual;
    }

    internal static void NotificarPuertaRota(Puerta puerta)
    {
        throw new NotImplementedException();
    }
}
