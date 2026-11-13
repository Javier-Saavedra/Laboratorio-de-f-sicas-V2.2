using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Portal estilo "Portal". Se agrega a un objeto con un Collider en modo Trigger.
/// Todo Rigidbody que entre sale por el portal destino conservando su velocidad,
/// rotada según la orientación de ambos portales.
/// El eje Z azul (forward) de cada portal es la dirección por la que salen los objetos.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    [Tooltip("Portal por el que saldrán los objetos.")]
    public Portal destino;
    [Tooltip("Distancia a la que aparece el objeto frente al portal destino.")]
    public float salidaOffset = 1.2f;
    [Tooltip("Segundos antes de que un objeto pueda volver a teletransportarse.")]
    public float cooldown = 0.5f;

    // Evita bucles infinitos entre los dos portales
    static readonly Dictionary<Rigidbody, float> ultimoViaje = new Dictionary<Rigidbody, float>();

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (destino == null) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        if (ultimoViaje.TryGetValue(rb, out float t) && Time.time - t < cooldown) return;
        ultimoViaje[rb] = Time.time;

        // Giro que lleva "entrar por este portal" a "salir por el destino" (mirando al lado contrario)
        Quaternion giro = destino.transform.rotation * Quaternion.Euler(0f, 180f, 0f) * Quaternion.Inverse(transform.rotation);

        rb.position = destino.transform.position + destino.transform.forward * salidaOffset;
        rb.rotation = giro * rb.rotation;
        rb.linearVelocity = giro * rb.linearVelocity;
        rb.angularVelocity = giro * rb.angularVelocity;

        rb.transform.position = rb.position;
        rb.transform.rotation = rb.rotation;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = destino != null ? Color.cyan : Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 1.5f);
    }
}
