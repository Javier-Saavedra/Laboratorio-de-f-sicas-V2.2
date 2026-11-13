using UnityEngine;

/// <summary>
/// CASO: Núcleo magnético
/// Atrae o repele partículas según la ley de cuadrado inverso,
/// similar a la gravitación universal pero configurable en signo.
///
/// FÓRMULA: F = G * m * r̂ / |r|²
/// Positivo = atracción,  Negativo = repulsión
/// </summary>
public class MagnetismForceGenerator : MonoBehaviour, IForceGenerator
{
    [Header("Parámetros")]
    [Tooltip("Constante gravitacional/magnética. Positivo = atrae, Negativo = repele.")]
    public float G = 20f;

    [Tooltip("Radio máximo de efecto. 0 = afecta a todo el mundo.")]
    [Min(0f)] public float radius = 8f;

    [Tooltip("Distancia mínima para evitar singularidad (división por cero).")]
    [Min(0.05f)] public float minDistance = 0.3f;

    private void OnEnable()  { ParticleWorld.Register((IForceGenerator)this); }
    private void OnDisable() { ParticleWorld.Unregister((IForceGenerator)this); }

    public void ApplyForces(float dt)
    {
        Vector3 center = transform.position;

        foreach (Particle p in ParticleWorld.All)
        {
            Vector3 toCenter = center - p.Position;  // Apunta HACIA el núcleo
            float dist = Mathf.Max(toCenter.magnitude, minDistance);

            if (radius > 0f && dist > radius) continue;

            // F = G * masa * dirección / dist²
            // La masa escala la fuerza → aceleración constante independiente de la masa (como gravedad real)
            float magnitude = G * p.Mass / (dist * dist);
            Vector3 force = toCenter.normalized * magnitude;

            p.AddForce(force);
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

        // Núcleo con anillos para verse magnético
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.9f);

#if UNITY_EDITOR
        string label = attracts ? "🧲 Atracción" : "🧲 Repulsión";
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (radius > 0f ? radius + 0.3f : 1.2f),
            $"{label}  G={G:F1}  r={radius:F1}"
        );
#endif
    }
}
