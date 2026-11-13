using UnityEngine;
public class WindForceGenerator : MonoBehaviour, IForceGenerator
{
   
    public Vector3 windDirection = Vector3.right;
    public bool enableTurbulence = true;
    [Min(0f)] public float turbulenceStrength = 1.5f;
    [Min(0.01f)] public float turbulenceFrequency = 0.8f;
    [Min(0f)] public float strength = 5f;

    [Header("Zona de efecto")]
    [Tooltip("Radio de influencia alrededor de este GameObject. 0 = afecta a todas las partículas del mundo.")]
    [Min(0f)] public float radius = 0f;

    private const float OFFSET_X = 0f;
    private const float OFFSET_Y = 31.41f;
    private const float OFFSET_Z = 72.83f;
    private void OnEnable() { ParticleWorld.Register((IForceGenerator)this); }
    private void OnDisable() { ParticleWorld.Unregister((IForceGenerator)this); }

    public void ApplyForces(float dt)
    {
        // F_base: dirección normalizada × magnitud
        Vector3 baseForce = windDirection.normalized * strength;

        // F_turbulencia: ruido de Perlin centrado en 0 (-1 … +1) en cada eje
        Vector3 turbForce = Vector3.zero;
        if (enableTurbulence)
        {
            float t = Time.time * turbulenceFrequency;
            float tx = (Mathf.PerlinNoise(t + OFFSET_X, 0f) * 2f - 1f) * turbulenceStrength;
            float ty = (Mathf.PerlinNoise(t + OFFSET_Y, 0f) * 2f - 1f) * turbulenceStrength;
            float tz = (Mathf.PerlinNoise(t + OFFSET_Z, 0f) * 2f - 1f) * turbulenceStrength;

            turbForce = new Vector3(tx, ty, tz);
        }

        Vector3 totalForce = baseForce + turbForce;

        // Iterar todas las partículas del mundo
        foreach (Particle p in ParticleWorld.All)
        {
            // Si radius > 0, solo afectar a las que estén dentro de la zona
            if (radius > 0f)
            {
                float dist = Vector3.Distance(transform.position, p.Position);
                if (dist > radius) continue;
            }

            p.AddForce(totalForce);
        }
    }
    private void OnDrawGizmos()
    {
        // Dirección y magnitud del viento
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.9f);
        Vector3 arrowEnd = transform.position + windDirection.normalized * strength * 0.3f;
        Gizmos.DrawLine(transform.position, arrowEnd);
        Gizmos.DrawSphere(arrowEnd, 0.08f);

        // Zona de efecto (si está activa)
        if (radius > 0f)
        {
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.15f);
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        #if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (radius > 0f ? radius + 0.3f : 0.5f),
            $"Wind  str={strength:F1}  turb={turbulenceStrength:F1}"
        );
#endif
    }
}