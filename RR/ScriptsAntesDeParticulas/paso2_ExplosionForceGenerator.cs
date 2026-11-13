using UnityEngine;
 
public class ExplosionForceGenerator : MonoBehaviour, IForceGenerator
{
    [Header("Parámetros")]
    [Min(0f)] public float strength = 200f;
    [Min(0.1f)] public float radius = 8f;
    public bool continuous = false;
 
    private bool shouldExplode = false;
 
    private void Start()
    {
        shouldExplode = false;
    }
 
    private void OnEnable()  { ParticleWorld.Register((IForceGenerator)this); }
    private void OnDisable() { ParticleWorld.Unregister((IForceGenerator)this); }
 
    public void Trigger()
    {
        shouldExplode = true;
    }
 
    public void ApplyForces(float dt)
    {
        if (!continuous && !shouldExplode) return;
        shouldExplode = false;
 
        Vector3 center = transform.position;
 
        foreach (Particle p in ParticleWorld.All)
        {
            Vector3 toParticle = p.Position - center;
            float dist = toParticle.magnitude;
 
            if (dist <= Mathf.Epsilon || dist > radius) continue;
 
            float falloff = 1f - (dist / radius);
            Vector3 force = toParticle.normalized * strength * falloff;
            p.AddForce(force);
        }
    }
 
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.12f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.15f);
 
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (radius + 0.3f),
            $"💥 Explosion  str={strength:F0}  r={radius:F1}  continuous={continuous}"
        );
#endif
    }
}