using UnityEngine;

/// <summary>
/// Viento direccional con turbulencia.
/// Se agrega directamente al objeto que quieres afectar.
/// F = F_base + F_turbulencia(t)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WindForceGenerator : MonoBehaviour
{
    [Header("Fuerza base")]
    public Vector3 windDirection = Vector3.right;
    [Min(0f)] public float strength = 10f;

    [Header("Turbulencia")]
    public bool enableTurbulence = true;
    [Min(0f)] public float turbulenceStrength = 2f;
    [Min(0.01f)] public float turbulenceFrequency = 0.8f;

    private Rigidbody rb;
    private const float OFFSET_X = 0f;
    private const float OFFSET_Y = 31.41f;
    private const float OFFSET_Z = 72.83f;

    private void Awake() => rb = GetComponent<Rigidbody>();

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;

        Vector3 baseForce = windDirection.normalized * strength;

        Vector3 turbForce = Vector3.zero;
        if (enableTurbulence)
        {
            float t = Time.time * turbulenceFrequency;
            float tx = (Mathf.PerlinNoise(t + OFFSET_X, 0f) * 2f - 1f) * turbulenceStrength;
            float ty = (Mathf.PerlinNoise(t + OFFSET_Y, 0f) * 2f - 1f) * turbulenceStrength;
            float tz = (Mathf.PerlinNoise(t + OFFSET_Z, 0f) * 2f - 1f) * turbulenceStrength;
            turbForce = new Vector3(tx, ty, tz);
        }

        rb.AddForce(baseForce + turbForce, ForceMode.Force);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.9f);
        Vector3 end = transform.position + windDirection.normalized * strength * 0.3f;
        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawSphere(end, 0.08f);
    }
}
