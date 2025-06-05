using UnityEngine;

public class Puerta : MonoBehaviour
{
    public bool esReal = false;
    private bool yaAtravesada = false;
    public float fuerzaCaida = 1000f;
    private Rigidbody rb;
    public FilaPuertas filaPadre;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    public bool EstaRota()
    {
        return yaAtravesada || (rb != null && !rb.isKinematic);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (esReal && !yaAtravesada &&
            (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("IA")))
        {
            yaAtravesada = true;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(-transform.forward * fuerzaCaida);
            }

            if (filaPadre != null)
                filaPadre.NotificarPuertaRota(this);

            Destroy(gameObject, 0.4f);
        }
    }
}
