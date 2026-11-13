using UnityEngine;

/// <summary>
/// Explosión radial desde el objeto donde está puesto.
/// Afecta a todos los Rigidbodies cercanos dentro del radio.
/// Presiona la tecla asignada para detonar.
/// F = strength * (1 - dist/radius) * dirección_saliente
/// </summary>
public class ExplosionForceGenerator : MonoBehaviour
{
    [Header("Parámetros")]
    [Min(0f)] public float strength = 800f;
    [Min(0.1f)] public float radius = 8f;
    public KeyCode triggerKey = KeyCode.Alpha1;

    [Header("Efecto Visual")]
    public GameObject explosionEffect;

    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
            Detonate();
    }

    public void Detonate()
    {
        Rigidbody[] bodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
        foreach (Rigidbody rb in bodies)
        {
            if (rb.isKinematic) continue;
            Vector3 toBody = rb.position - transform.position;
            float dist = toBody.magnitude;
            if (dist <= Mathf.Epsilon || dist > radius) continue;

            float falloff = 1f - (dist / radius);
            rb.AddForce(toBody.normalized * strength * falloff, ForceMode.Impulse);
        }

        if (explosionEffect != null)
        {
            var fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.12f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
