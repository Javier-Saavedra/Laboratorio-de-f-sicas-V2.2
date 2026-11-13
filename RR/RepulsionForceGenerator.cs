using UnityEngine;

/// <summary>
/// Repulsión continua desde este objeto hacia todos los Rigidbodies cercanos.
/// F = strength / dist² * dirección_saliente
/// </summary>
public class RepulsionForceGenerator : MonoBehaviour
{
    [Header("Parámetros")]
    [Min(0f)] public float strength = 30f;
    [Min(0f)] public float radius = 6f;
    [Min(0.01f)] public float minDistance = 0.5f;

    private void FixedUpdate()
    {
        Rigidbody[] bodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in bodies)
        {
            if (rb.isKinematic) continue;
            if (rb.gameObject == gameObject) continue;

            Vector3 toBody = rb.position - transform.position;
            float dist = Mathf.Max(toBody.magnitude, minDistance);
            if (radius > 0f && dist > radius) continue;

            float magnitude = strength / (dist * dist);
            rb.AddForce(toBody.normalized * magnitude, ForceMode.Force);
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
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
