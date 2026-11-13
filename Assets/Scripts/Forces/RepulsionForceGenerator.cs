using UnityEngine;

/// <summary>
/// CASO: Campo de repulsión energética
/// Repele continuamente todas las partículas dentro del radio,
/// con fuerza inversamente proporcional al cuadrado de la distancia.
///
/// FÓRMULA: F = (strength / dist²) * dirección_saliente
/// </summary>
public class RepulsionForceGenerator : MonoBehaviour, IForceGenerator
{
    [Header("Parámetros")]
    [Tooltip("Intensidad de la repulsión.")]
    [Min(0f)] public float strength = 30f;

    [Tooltip("Radio máximo de efecto. 0 = afecta a todo el mundo.")]
    [Min(0f)] public float radius = 6f;

    [Tooltip("Distancia mínima para evitar fuerzas infinitas.")]
    [Min(0.01f)] public float minDistance = 0.5f;

    private void OnEnable()  { ParticleWorld.Register((IForceGenerator)this); }
    private void OnDisable() { ParticleWorld.Unregister((IForceGenerator)this); }

    public void ApplyForces(float dt)
    {
        Vector3 center = transform.position;

        foreach (Particle p in ParticleWorld.All)
        {
            Vector3 toParticle = p.Position - center;
            float dist = Mathf.Max(toParticle.magnitude, minDistance);

            if (radius > 0f && dist > radius) continue;

            // Ley de cuadrado inverso: más fuerza cerca, cae rápido con distancia
            float magnitude = strength / (dist * dist);
            Vector3 force = toParticle.normalized * magnitude;

            p.AddForce(force);
        }
    }

    private void OnDrawGizmos()
    {
        if (radius > 0f)
        {
            Gizmos.color = new Color(0.4f, 0f, 1f, 0.1f);
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.color = new Color(0.4f, 0f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        Gizmos.color = new Color(0.8f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.DrawSphere(transform.position, 0.15f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (radius > 0f ? radius + 0.3f : 1f),
            $"⚡ Repulsion  str={strength:F0}  r={radius:F1}"
        );
#endif
    }
}
