using UnityEngine;

/// <summary>
/// CASO: Generador de vórtice / tornado
/// Aplica dos fuerzas combinadas:
///   1. Fuerza tangencial: hace orbitar las partículas alrededor del eje vertical.
///   2. Fuerza de succión: las jala hacia el centro del vórtice.
///
/// FÓRMULA:
///   F_tangencial = Cross(axis, toParticle).normalized * tangentialStrength / dist
///   F_succión    = -toParticle.normalized * suctionStrength / dist
/// </summary>
public class VortexForceGenerator : MonoBehaviour, IForceGenerator
{
    [Header("Parámetros")]
    [Tooltip("Fuerza de rotación (tangencial). Negativo = gira al revés.")]
    public float tangentialStrength = 15f;

    [Tooltip("Fuerza de succión hacia el eje central.")]
    [Min(0f)] public float suctionStrength = 5f;

    [Tooltip("Fuerza de elevación vertical (imita el arrastre hacia arriba de un tornado).")]
    public float liftStrength = 3f;

    [Tooltip("Radio máximo de efecto.")]
    [Min(0.1f)] public float radius = 7f;

    [Tooltip("Distancia mínima al eje para evitar singularidad.")]
    [Min(0.05f)] public float minDistance = 0.2f;

    [Tooltip("Eje de rotación del vórtice (por defecto vertical).")]
    public Vector3 axis = Vector3.up;

    private void OnEnable()  { ParticleWorld.Register((IForceGenerator)this); }
    private void OnDisable() { ParticleWorld.Unregister((IForceGenerator)this); }

    public void ApplyForces(float dt)
    {
        Vector3 center = transform.position;
        Vector3 normalizedAxis = axis.normalized;

        foreach (Particle p in ParticleWorld.All)
        {
            // Proyectar la posición de la partícula sobre el plano perpendicular al eje
            Vector3 toParticle = p.Position - center;
            Vector3 axisComponent = Vector3.Dot(toParticle, normalizedAxis) * normalizedAxis;
            Vector3 radialComponent = toParticle - axisComponent; // Vector en el plano del vórtice

            float dist = Mathf.Max(radialComponent.magnitude, minDistance);
            if (dist > radius) continue;

            // 1. Fuerza tangencial: perpendicular al radio → genera rotación
            Vector3 tangent = Vector3.Cross(normalizedAxis, radialComponent).normalized;
            Vector3 fTangential = tangent * tangentialStrength / dist;

            // 2. Fuerza de succión: jala hacia el eje central
            Vector3 fSuction = -radialComponent.normalized * suctionStrength / dist;

            // 3. Elevación vertical
            Vector3 fLift = normalizedAxis * liftStrength;

            p.AddForce(fTangential + fSuction + fLift);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        Vector3 normalizedAxis = axis.normalized;

        // Dibujar espiral del vórtice
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.8f);
        int rings = 6;
        for (int i = 0; i < rings; i++)
        {
            float t = (float)i / rings;
            float r = radius * (1f - t * 0.7f);
            float h = t * 3f;
            Gizmos.DrawWireSphere(center + normalizedAxis * h, r * 0.15f);
        }

        // Eje del vórtice
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.5f);
        Gizmos.DrawLine(center - normalizedAxis * 0.5f, center + normalizedAxis * 4f);

        // Radio de efecto
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.1f);
        Gizmos.DrawSphere(center, radius);
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.5f);
        Gizmos.DrawWireSphere(center, radius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            center + Vector3.up * (radius + 0.3f),
            $"🌀 Vórtice  tan={tangentialStrength:F1}  suc={suctionStrength:F1}  r={radius:F1}"
        );
#endif
    }
}
