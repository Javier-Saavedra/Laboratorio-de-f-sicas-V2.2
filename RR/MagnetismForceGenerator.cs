using UnityEngine;

/// <summary>
/// Magnetismo: atrae o repele todos los Rigidbodies cercanos hacia este objeto.
/// F = G * m * r̂ / |r|²
/// Positivo = atrae, Negativo = repele.
/// </summary>
public class MagnetismForceGenerator : MonoBehaviour
{
    [Header("Parámetros")]
    [Tooltip("Positivo = atrae, Negativo = repele")]
    public float G = 20f;
    [Min(0f)] public float radius = 8f;
    [Min(0.05f)] public float minDistance = 0.3f;

    private void FixedUpdate()
    {
        Rigidbody[] bodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in bodies)
        {
            if (rb.isKinematic) continue;
            // No aplicarse a sí mismo
            if (rb.gameObject == gameObject) continue;

            Vector3 toCenter = transform.position - rb.position;
            float dist = Mathf.Max(toCenter.magnitude, minDistance);
            if (radius > 0f && dist > radius) continue;

            float magnitude = G * rb.mass / (dist * dist);
            rb.AddForce(toCenter.normalized * magnitude, ForceMode.Force);
        }
    }

    private void OnDrawGizmos()
    {
        bool attracts = G >= 0f;
        Color col = attracts ? new Color(0f, 0.8f, 1f) : new Color(1f, 0.2f, 0.2f);
        if (radius > 0f)
        {
            Gizmos.color = new Color(col.r, col.g, col.b, 0.1f);
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.color = new Color(col.r, col.g, col.b, 0.7f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}
