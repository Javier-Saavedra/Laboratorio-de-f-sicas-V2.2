using UnityEngine;

/// <summary>
/// Restriccion de contencion: mantiene cada Particle dentro de la sala (suelo y paredes).
///
/// Por que hace falta: ParticleWorld corrige el solapamiento entre dos esferas moviendo sus posiciones
/// directamente. Si una esfera esta apoyada sobre otra que descansa en el suelo, esa correccion puede
/// empujar a la de abajo dentro del suelo, y el SphereCast de Particle no detecta un collider cuando
/// ya se esta dentro de el: la particula caeria para siempre.
///
/// Despues de cada paso de simulacion (cuando ParticleWorld ya termino) se devuelve cada particula al
/// interior de la sala y se anula la velocidad que la empujaria hacia afuera. Es una correccion de
/// posicion por restriccion, no una fuerza: no cambia la energia de las particulas que estan dentro.
/// </summary>
public class ContencionParticulas : MonoBehaviour
{
    [Header("Limites de la sala")]
    [Tooltip("Altura de la superficie del suelo.")]
    public float sueloY = 0f;
    [Tooltip("Cara interior de las paredes en X (minimo, maximo).")]
    public Vector2 limitesX = new Vector2(-10f, 10f);
    [Tooltip("Cara interior de las paredes en Z (minimo, maximo).")]
    public Vector2 limitesZ = new Vector2(-10f, 10f);

    [Tooltip("Altura maxima: evita que los cuerpos se pierdan hacia arriba (gravedad invertida, explosiones).")]
    public float techoY = 8f;

    [Header("Reposo")]
    [Tooltip("Una particula apoyada en el suelo con |velocidad vertical| menor a esto se considera en reposo.")]
    public float reposoVertical = 0.4f;

    void OnEnable()
    {
        if (SimulationManager.Instance != null)
            SimulationManager.Instance.OnSimulationStep += Aplicar;
    }

    void OnDisable()
    {
        if (SimulationManager.Instance != null)
            SimulationManager.Instance.OnSimulationStep -= Aplicar;
    }

    void Aplicar(float dt)
    {
        foreach (Particle p in ParticleWorld.All)
        {
            Vector3 pos = p.Position;
            Vector3 vel = p.Velocity;
            float r = p.Radius;
            bool cambio = false;

            // Suelo
            float minY = sueloY + r;
            if (pos.y < minY)
            {
                pos.y = minY;
                if (vel.y < 0f) vel.y = 0f;
                cambio = true;
            }
            else if (pos.y < minY + 0.002f && Mathf.Abs(vel.y) < reposoVertical && vel.y != 0f)
            {
                vel.y = 0f; // apoyada: elimina el rebote minimo que queda en reposo
                cambio = true;
            }

            // Techo
            float maxY = techoY - r;
            if (pos.y > maxY) { pos.y = maxY; if (vel.y > 0f) vel.y = 0f; cambio = true; }

            // Paredes
            float minX = limitesX.x + r, maxX = limitesX.y - r;
            if (pos.x < minX) { pos.x = minX; if (vel.x < 0f) vel.x = 0f; cambio = true; }
            else if (pos.x > maxX) { pos.x = maxX; if (vel.x > 0f) vel.x = 0f; cambio = true; }

            float minZ = limitesZ.x + r, maxZ = limitesZ.y - r;
            if (pos.z < minZ) { pos.z = minZ; if (vel.z < 0f) vel.z = 0f; cambio = true; }
            else if (pos.z > maxZ) { pos.z = maxZ; if (vel.z > 0f) vel.z = 0f; cambio = true; }

            if (cambio)
            {
                p.Position = pos;
                p.Velocity = vel;
            }
        }
    }
}
