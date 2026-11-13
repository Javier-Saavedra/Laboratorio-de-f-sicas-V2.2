using UnityEngine;

/// <summary>
/// Vórtice/Tornado: hace orbitar todos los Rigidbodies cercanos alrededor de este objeto.
/// F = tangencial + succión + elevación
/// </summary>
public class VortexForceGenerator : MonoBehaviour
{
    [Header("Parámetros")]
    public float tangentialStrength = 15f;
    [Min(0f)] public float suctionStrength = 5f;
    public float liftStrength = 3f;
    [Min(0.1f)] public float radius = 7f;
    [Min(0.05f)] public float minDistance = 0.2f;
    public Vector3 axis = Vector3.up;

    private void FixedUpdate()
    {
        Vector3 normalizedAxis = axis.normalized;
        Rigidbody[] bodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in bodies)
        {
            if (rb.isKinematic) continue;
            if (rb.gameObject == gameObject) continue;

            Vector3 toBody = rb.position - transform.position;
            Vector3 axisComponent = Vector3.Dot(toBody, normalizedAxis) * normalizedAxis;
            Vector3 radial = toBody - axisComponent;
            float dist = Mathf.Max(radial.magnitude, minDistance);
            if (dist > radius) continue;

            Vector3 tangent = Vector3.Cross(normalizedAxis, radial).normalized;
            Vector3 fTangential = tangent * tangentialStrength / dist;
            Vector3 fSuction = -radial.normalized * suctionStrength / dist;
            Vector3 fLift = normalizedAxis * liftStrength;

            rb.AddForce(fTangential + fSuction + fLift, ForceMode.Force);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + axis.normalized * 4f);
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.1f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
