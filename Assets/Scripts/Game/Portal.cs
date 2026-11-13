using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Portal estilo "Portal". Se agrega a un objeto con un Collider en modo Trigger.
/// Toda Particle que toque la abertura del portal sale por el portal destino conservando su velocidad,
/// rotada según la orientación de ambos portales.
/// El eje Z azul (forward) de cada portal es la dirección por la que salen los objetos.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    [Tooltip("Portal por el que saldrán los objetos.")]
    public Portal destino;
    [Tooltip("Distancia a la que aparece el objeto frente al portal destino.")]
    public float salidaOffset = 1.2f;
    [Tooltip("Segundos antes de que un objeto pueda volver a teletransportarse.")]
    public float cooldown = 0.5f;

    // Evita bucles infinitos entre los dos portales
    static readonly Dictionary<Particle, float> ultimoViaje = new Dictionary<Particle, float>();
    // Velocidad de cada Particle en los dos ultimos pasos (la actual puede ser la ya rebotada contra una pared)
    static readonly Dictionary<Particle, Vector3> velActual = new Dictionary<Particle, Vector3>();
    static readonly Dictionary<Particle, Vector3> velAnterior = new Dictionary<Particle, Vector3>();
    static int pasoInstantanea = -1;

    [Tooltip("Margen extra (m) al detectar que una esfera toca la abertura del portal.")]
    public float margenContacto = 0.05f;

    SimulationManager simSuscrita;
    bool iniciado;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetearEstatico()
    {
        LimpiarEstado();
    }

    static void LimpiarEstado()
    {
        ultimoViaje.Clear();
        velActual.Clear();
        velAnterior.Clear();
        pasoInstantanea = -1;
    }

    // Las Particle no usan Rigidbody ni triggers: el portal las revisa en cada paso de simulacion
    void Start()
    {
        iniciado = true;
        Suscribir();
    }

    void OnEnable()
    {
        if (iniciado) Suscribir();
    }

    void OnDisable()
    {
        Desuscribir();
    }

    void Suscribir()
    {
        var sim = SimulationManager.Instance;
        if (sim == null || simSuscrita == sim) return;
        simSuscrita = sim;
        sim.OnSimulationStep += Paso;
        sim.OnSimulationReset += LimpiarEstado;
    }

    void Desuscribir()
    {
        if (simSuscrita == null) return;
        simSuscrita.OnSimulationStep -= Paso;
        simSuscrita.OnSimulationReset -= LimpiarEstado;
        simSuscrita = null;
    }

    // Una sola vez por paso (lo hace el primer portal que corre): guarda la velocidad de cada Particle
    static void TomarInstantanea(int paso)
    {
        if (paso == pasoInstantanea) return;
        pasoInstantanea = paso;

        if (paso % 200 == 0) Purgar();

        foreach (Particle p in ParticleWorld.All)
        {
            Vector3 v = p.Velocity;
            velAnterior[p] = velActual.TryGetValue(p, out Vector3 previa) ? previa : v;
            velActual[p] = v;
        }
    }

    static void Purgar()
    {
        var viejas = new List<Particle>();
        foreach (var k in velActual.Keys) if (k == null) viejas.Add(k);
        foreach (var k in ultimoViaje.Keys) if (k == null) viejas.Add(k);
        foreach (var k in viejas)
        {
            velActual.Remove(k);
            velAnterior.Remove(k);
            ultimoViaje.Remove(k);
        }
    }

    void Paso(float dt)
    {
        if (destino == null) return;

        var sim = SimulationManager.Instance;
        TomarInstantanea(sim.StepCount);
        float tiempo = sim.SimulationTime;

        Transform t = transform;
        Quaternion inversa = Quaternion.Inverse(t.rotation);
        Vector3 fwd = t.forward;
        Vector3 escala = t.lossyScale;
        float mitadAncho = Mathf.Abs(escala.x) * 0.5f;
        float mitadAlto = Mathf.Abs(escala.y) * 0.5f;

        // Giro que lleva "entrar por este portal" a "salir por el destino" (mirando al lado contrario)
        Quaternion giro = destino.transform.rotation * Quaternion.Euler(0f, 180f, 0f) * inversa;

        foreach (Particle p in ParticleWorld.All)
        {
            if (ultimoViaje.TryGetValue(p, out float ultimo) && tiempo - ultimo < cooldown) continue;

            // Posicion en el sistema local del portal (sin escala): x,y dentro de la abertura, z = distancia al plano
            Vector3 local = inversa * (p.Position - t.position);
            if (Mathf.Abs(local.x) > mitadAncho || Mathf.Abs(local.y) > mitadAlto) continue;

            // Solo entra lo que se dirige hacia el portal. Si ya rebotó contra la pared de atras se usa la velocidad del paso anterior
            Vector3 vEntrada;
            if (Vector3.Dot(p.Velocity, fwd) < -0.01f) vEntrada = p.Velocity;
            else if (velAnterior.TryGetValue(p, out Vector3 previa) && Vector3.Dot(previa, fwd) < -0.01f) vEntrada = previa;
            else continue;

            // La esfera toca el plano del portal (o lo cruzó en este paso)
            float r = p.Radius;
            if (local.z > r + margenContacto || local.z < -vEntrada.magnitude * dt) continue;

            // Sale frente al destino, con el desplazamiento lateral reflejado, y conserva la rapidez
            float offset = Mathf.Max(salidaOffset, r + 0.1f);
            Vector3 localSalida = new Vector3(-local.x, local.y, offset);
            p.Position = destino.transform.position + destino.transform.rotation * localSalida;
            p.Velocity = giro * vEntrada;

            ultimoViaje[p] = tiempo;
            velActual[p] = p.Velocity;
            velAnterior[p] = p.Velocity;
        }
    }




    void OnDrawGizmos()
    {
        Gizmos.color = destino != null ? Color.cyan : Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 1.5f);
    }
}
