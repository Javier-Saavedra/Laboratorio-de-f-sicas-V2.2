using System.Linq;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Coloca los portales A y B donde apunta el mouse.
/// Tecla A (por defecto 1) -> portal A, tecla B (por defecto 2) -> portal B.
/// El portal queda pegado a la superficie, mirando hacia afuera (su eje Z es la salida).
/// </summary>
public class PortalPlacer : MonoBehaviour
{
    [Tooltip("Camara principal. Si esta vacia se usa Camera.main.")]
    public Camera cam;
    public Portal portalA;
    public Portal portalB;

    [Header("Teclas")]
    public KeyCode teclaPortalA = KeyCode.Alpha1;
    public KeyCode teclaPortalB = KeyCode.Alpha2;

    [Header("Colocacion")]
    public float distanciaMax = 100f;
    [Tooltip("Separacion respecto a la superficie para evitar z-fighting.")]
    public float separacionSuperficie = 0.08f;
    [Tooltip("Altura minima del centro del portal sobre el suelo.")]
    public float alturaMinima = 1.2f;
    [Tooltip("Distancia minima permitida entre los dos portales.")]
    public float distanciaMinimaEntrePortales = 1.5f;

    Vector3 escalaA, escalaB;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (portalA == null) portalA = FindPortal("PortalA");
        if (portalB == null) portalB = FindPortal("PortalB");

        if (portalA != null) escalaA = portalA.transform.localScale;
        if (portalB != null) escalaB = portalB.transform.localScale;

        // Se aseguran los enlaces entre ambos
        if (portalA != null && portalB != null)
        {
            portalA.destino = portalB;
            portalB.destino = portalA;
        }
    }

    static Portal FindPortal(string nombre)
    {
        var go = GameObject.Find(nombre);
        return go != null ? go.GetComponent<Portal>() : null;
    }

    void Update()
    {
        if (cam == null || portalA == null || portalB == null) return;

        if (Input.GetKeyDown(teclaPortalA)) Colocar(portalA, portalB, escalaA);
        else if (Input.GetKeyDown(teclaPortalB)) Colocar(portalB, portalA, escalaB);
    }

    void Colocar(Portal portal, Portal otro, Vector3 escalaBase)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!PrimerGolpeValido(ray, distanciaMax, out RaycastHit hit)) return;

        Vector3 normal = hit.normal;
        Vector3 posicion;
        Quaternion rotacion;

        if (Mathf.Abs(normal.y) > 0.7f)
        {
            // Suelo o techo: el portal queda de pie, mirando hacia la camara
            Vector3 haciaCamara = cam.transform.position - hit.point;
            haciaCamara.y = 0f;
            if (haciaCamara.sqrMagnitude < 0.001f) haciaCamara = -cam.transform.forward;
            rotacion = Quaternion.LookRotation(haciaCamara.normalized, Vector3.up);
            posicion = hit.point + Vector3.up * alturaMinima;
        }
        else
        {
            // Pared: el portal mira hacia afuera de la pared
            Vector3 plano = new Vector3(normal.x, 0f, normal.z).normalized;
            rotacion = Quaternion.LookRotation(plano, Vector3.up);
            posicion = hit.point + normal * separacionSuperficie;

            // No dejar el portal hundido en el suelo
            Ray abajo = new Ray(hit.point + normal * 0.2f, Vector3.down);
            if (PrimerGolpeValido(abajo, 50f, out RaycastHit suelo))
                posicion.y = Mathf.Max(posicion.y, suelo.point.y + alturaMinima);
        }

        if ((posicion - otro.transform.position).magnitude < distanciaMinimaEntrePortales) return;

        portal.transform.SetPositionAndRotation(posicion, rotacion);

        // Animacion de aparicion
        portal.transform.DOKill();
        portal.transform.localScale = escalaBase;
        portal.transform.DOScale(escalaBase, 0.3f).From(Vector3.zero).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Primer golpe sobre una superficie fija: ignora cuerpos con Rigidbody, portales y triggers.
    /// </summary>
    static bool PrimerGolpeValido(Ray ray, float distancia, out RaycastHit resultado)
    {
        var golpes = Physics.RaycastAll(ray, distancia, ~0, QueryTriggerInteraction.Ignore)
            .OrderBy(h => h.distance);

        foreach (var h in golpes)
        {
            if (h.collider.attachedRigidbody != null) continue;
            if (h.collider.GetComponentInParent<Portal>() != null) continue;
            resultado = h;
            return true;
        }

        resultado = default;
        return false;
    }
}
