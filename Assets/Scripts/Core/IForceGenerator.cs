/// <summary>
/// Interfaz que deben implementar todos los generadores de fuerza.
/// Registrarse en ParticleWorld.Register() y desregistrarse en Unregister().
/// </summary>
public interface IForceGenerator
{
    void ApplyForces(float dt);
}
 