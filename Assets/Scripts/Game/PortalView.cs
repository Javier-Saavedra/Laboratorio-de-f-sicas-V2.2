using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Hace que la cara de este portal muestre lo que se veria al otro lado (el portal destino).
/// Crea una camara oculta que renderiza a una textura, y una superficie (quad) frente al portal que la muestra.
/// </summary>
[RequireComponent(typeof(Portal))]
public class PortalView : MonoBehaviour
{
    [Tooltip("Camara del jugador. Si esta vacia se usa Camera.main.")]
    public Camera camaraJugador;
    [Tooltip("Shader Portal/PortalView.")]
    public Shader shaderVista;
    [Tooltip("Escala de la resolucion de la textura (1 = pantalla completa).")]
    [Range(0.25f, 1f)] public float resolucion = 1f;
    [Tooltip("Separacion de la superficie respecto a la cara del portal (metros).")]
    public float separacionSuperficie = 0.01f;

    Portal portal;
    Camera camPortal;
    RenderTexture rt;
    Renderer superficie;
    Material matVista;
    bool superficieOculta;

    static readonly int IdMainTex = Shader.PropertyToID("_MainTex");

    void Awake()
    {
        portal = GetComponent<Portal>();
        if (camaraJugador == null) camaraJugador = Camera.main;

        CrearSuperficie();
        CrearCamara();
    }

    void CrearSuperficie()
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Vista";

        var col = quad.GetComponent<Collider>();
        if (col != null) Destroy(col);

        quad.transform.SetParent(transform, false);
        // El Quad de Unity se ve por -Z, se gira para que mire hacia la salida del portal (+Z)
        quad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        float escalaZ = Mathf.Max(0.0001f, transform.localScale.z);
        quad.transform.localPosition = new Vector3(0f, 0f, 0.5f + separacionSuperficie / escalaZ);
        quad.transform.localScale = Vector3.one;

        Shader sh = shaderVista != null ? shaderVista : Shader.Find("Portal/PortalView");
        matVista = new Material(sh);
        matVista.name = "PortalView_" + name;

        superficie = quad.GetComponent<MeshRenderer>();
        superficie.sharedMaterial = matVista;
        superficie.shadowCastingMode = ShadowCastingMode.Off;
        superficie.receiveShadows = false;
    }

    void CrearCamara()
    {
        var go = new GameObject("PortalCam_" + name);
        camPortal = go.AddComponent<Camera>();
        camPortal.enabled = false;

        var datos = camPortal.GetUniversalAdditionalCameraData();
        datos.renderPostProcessing = false;
    }

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += AntesDeRenderizar;
        RenderPipelineManager.endCameraRendering += DespuesDeRenderizar;
        CinemachineCore.CameraUpdatedEvent.AddListener(AlActualizarCinemachine);
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= AntesDeRenderizar;
        RenderPipelineManager.endCameraRendering -= DespuesDeRenderizar;
        CinemachineCore.CameraUpdatedEvent.RemoveListener(AlActualizarCinemachine);

        if (superficie != null) superficie.enabled = true;
        if (camPortal != null) camPortal.enabled = false;
    }

    void OnDestroy()
    {
        if (camPortal != null) Destroy(camPortal.gameObject);
        if (rt != null) { rt.Release(); Destroy(rt); }
        if (matVista != null) Destroy(matVista);
    }

    void AlActualizarCinemachine(CinemachineBrain brain)
    {
        ActualizarCamara();
    }

    void LateUpdate()
    {
        ActualizarCamara();
    }

    // La superficie propia se oculta mientras su camara renderiza, para no leer y escribir la misma textura
    void AntesDeRenderizar(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam == camPortal && superficie != null)
        {
            superficie.enabled = false;
            superficieOculta = true;
        }
    }

    void DespuesDeRenderizar(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam == camPortal && superficieOculta)
        {
            superficie.enabled = true;
            superficieOculta = false;
        }
    }

    void ActualizarCamara()
    {
        if (camPortal == null) return;
        if (camaraJugador == null) camaraJugador = Camera.main;

        if (camaraJugador == null || portal.destino == null)
        {
            camPortal.enabled = false;
            return;
        }

        Transform origen = transform;
        Transform destino = portal.destino.transform;

        // Solo se renderiza si el jugador mira la cara frontal del portal y esta en pantalla
        Vector3 haciaJugador = camaraJugador.transform.position - origen.position;
        bool porDelante = Vector3.Dot(origen.forward, haciaJugador) > 0f;
        Plane[] planos = GeometryUtility.CalculateFrustumPlanes(camaraJugador);
        bool enPantalla = GeometryUtility.TestPlanesAABB(planos, superficie.bounds);

        if (!porDelante || !enPantalla)
        {
            camPortal.enabled = false;
            return;
        }

        AsegurarTextura();

        // Posicion del jugador relativa a este portal, llevada al destino y girada 180 grados
        Matrix4x4 mOrigen = Matrix4x4.TRS(origen.position, origen.rotation, Vector3.one);
        Matrix4x4 mDestino = Matrix4x4.TRS(destino.position, destino.rotation, Vector3.one);
        Matrix4x4 m = mDestino * Matrix4x4.Rotate(Quaternion.Euler(0f, 180f, 0f)) * mOrigen.inverse
                      * camaraJugador.transform.localToWorldMatrix;
        camPortal.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        // Mismos ajustes que la camara del jugador
        camPortal.ResetProjectionMatrix();
        camPortal.fieldOfView = camaraJugador.fieldOfView;
        camPortal.nearClipPlane = camaraJugador.nearClipPlane;
        camPortal.farClipPlane = camaraJugador.farClipPlane;
        camPortal.aspect = camaraJugador.aspect;
        camPortal.clearFlags = camaraJugador.clearFlags;
        camPortal.backgroundColor = camaraJugador.backgroundColor;
        camPortal.cullingMask = camaraJugador.cullingMask;
        camPortal.allowHDR = camaraJugador.allowHDR;
        camPortal.depth = camaraJugador.depth - 5f; // se renderiza antes que la camara del jugador

        // Plano de corte oblicuo: descarta todo lo que queda detras del portal destino (paredes, etc.)
        Vector3 normal = destino.forward;
        Matrix4x4 vista = camPortal.worldToCameraMatrix;
        Vector3 posCam = vista.MultiplyPoint(destino.position);
        Vector3 normalCam = vista.MultiplyVector(normal).normalized;
        float dist = -Vector3.Dot(posCam, normalCam);
        if (Mathf.Abs(dist) > 0.2f)
        {
            camPortal.projectionMatrix = camPortal.CalculateObliqueMatrix(
                new Vector4(normalCam.x, normalCam.y, normalCam.z, dist));
        }

        camPortal.enabled = true;
    }

    void AsegurarTextura()
    {
        int w = Mathf.Max(16, Mathf.RoundToInt(camaraJugador.pixelWidth * resolucion));
        int h = Mathf.Max(16, Mathf.RoundToInt(camaraJugador.pixelHeight * resolucion));

        if (rt != null && rt.width == w && rt.height == h) return;

        if (rt != null)
        {
            camPortal.targetTexture = null;
            rt.Release();
            Destroy(rt);
        }

        rt = new RenderTexture(w, h, 24, RenderTextureFormat.DefaultHDR);
        rt.name = "PortalRT_" + name;
        rt.Create();

        camPortal.targetTexture = rt;
        matVista.SetTexture(IdMainTex, rt);
    }
}
