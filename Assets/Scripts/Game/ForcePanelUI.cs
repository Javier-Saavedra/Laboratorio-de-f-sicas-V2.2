using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de sliders de las fuerzas. Cada tarjeta (Sub_*) aparece cuando su fuerza esta activa
/// y desaparece cuando se apaga. Los sliders leen y escriben los parametros de PhysicsPropertyManager.
/// La explosion es un evento puntual: su tarjeta se queda visible unos segundos tras cada detonacion
/// y mientras el puntero este encima.
/// </summary>
public class ForcePanelUI : MonoBehaviour
{
    public enum Fuerza { Viento, Explosion, Magnetismo, Vortice, Repulsion }

    public enum Parametro
    {
        VientoFuerza, VientoTurbulencia, VientoDireccion,
        ExplosionFuerza, ExplosionRadio,
        MagnetismoG, MagnetismoRadio,
        VorticeTangencial, VorticeRadio, VorticeElevacion,
        RepulsionFuerza, RepulsionRadio
    }

    [Serializable]
    public class Tarjeta
    {
        public Fuerza fuerza;
        public RectTransform raiz;
        public CanvasGroup grupo;
        public TMP_Text tecla;
        [NonSerialized] public float alfa;
    }

    [Serializable]
    public class Control
    {
        public Parametro parametro;
        public Slider slider;
        public TMP_Text valor;
        public string formato = "0.0";
        public string sufijo = "";
    }

    public PhysicsPropertyManager manager;
    public Tarjeta[] tarjetas;
    public Control[] controles;

    [Tooltip("Segundos que sigue visible la tarjeta de explosion tras detonar.")]
    public float segundosExplosion = 5f;
    [Tooltip("Velocidad de aparicion y desaparicion.")]
    public float velocidadFade = 10f;

    void Start()
    {
        if (manager == null) manager = FindFirstObjectByType<PhysicsPropertyManager>();
        if (manager == null) { enabled = false; return; }

        foreach (var c in controles)
        {
            var ctl = c;
            ctl.slider.SetValueWithoutNotify(Mathf.Clamp(Leer(ctl.parametro), ctl.slider.minValue, ctl.slider.maxValue));
            ActualizarTexto(ctl, ctl.slider.value);
            ctl.slider.onValueChanged.AddListener(v =>
            {
                Escribir(ctl.parametro, v);
                ActualizarTexto(ctl, v);
            });
        }

        foreach (var t in tarjetas)
        {
            if (t.tecla != null) t.tecla.text = NombreTecla(t.fuerza);
            t.alfa = 0f;
            t.grupo.alpha = 0f;
            t.raiz.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        ActualizarTarjetas();
        AjustarEscala();
    }

    float escalaActual = 1f;

    // Si todas las tarjetas estan abiertas y no caben en pantalla, el panel se reduce solo
    void AjustarEscala()
    {
        var rectPanel = (RectTransform)transform;
        var rectCanvas = transform.parent as RectTransform;
        float disponible = (rectCanvas != null ? rectCanvas.rect.height : 1080f) - 80f;
        float alto = rectPanel.rect.height;
        float meta = (alto > disponible && alto > 0f) ? disponible / alto : 1f;
        escalaActual = Mathf.Lerp(escalaActual, meta, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
        rectPanel.localScale = Vector3.one * escalaActual;
    }

    void ActualizarTarjetas()
    {
        if (manager == null) return;
        float dt = Time.unscaledDeltaTime;

        foreach (var t in tarjetas)
        {
            bool visible = DebeVerse(t);
            t.alfa = Mathf.MoveTowards(t.alfa, visible ? 1f : 0f, velocidadFade * dt);

            if (t.alfa > 0f && !t.raiz.gameObject.activeSelf) t.raiz.gameObject.SetActive(true);
            if (t.alfa <= 0f && !visible)
            {
                if (t.raiz.gameObject.activeSelf) t.raiz.gameObject.SetActive(false);
                continue;
            }

            float suave = t.alfa * t.alfa * (3f - 2f * t.alfa);
            t.grupo.alpha = suave;
            t.grupo.interactable = visible;
            t.grupo.blocksRaycasts = visible;
            t.raiz.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, suave);
        }
    }

    bool DebeVerse(Tarjeta t)
    {
        switch (t.fuerza)
        {
            case Fuerza.Viento: return manager.VientoActivo;
            case Fuerza.Magnetismo: return manager.MagnetismoActivo;
            case Fuerza.Vortice: return manager.VorticeActivo;
            case Fuerza.Repulsion: return manager.RepulsionActivo;
            case Fuerza.Explosion:
                if (Time.unscaledTime - manager.UltimaExplosion < segundosExplosion) return true;
                return t.alfa > 0.01f &&
                       RectTransformUtility.RectangleContainsScreenPoint(t.raiz, Input.mousePosition, null);
        }
        return false;
    }

    string NombreTecla(Fuerza f)
    {
        switch (f)
        {
            case Fuerza.Viento: return manager.vientoKey.ToString().ToUpper();
            case Fuerza.Explosion: return manager.explosionKey.ToString().ToUpper();
            case Fuerza.Magnetismo: return manager.magnetismoKey.ToString().ToUpper();
            case Fuerza.Vortice: return manager.vorticeKey.ToString().ToUpper();
            default: return manager.repulsionKey.ToString().ToUpper();
        }
    }

    static void ActualizarTexto(Control c, float v)
    {
        if (c.valor != null) c.valor.text = v.ToString(c.formato) + c.sufijo;
    }

    float Leer(Parametro p)
    {
        switch (p)
        {
            case Parametro.VientoFuerza: return manager.windStrength;
            case Parametro.VientoTurbulencia: return manager.windTurbulence;
            case Parametro.VientoDireccion:
                Vector3 d = manager.windDirection;
                float ang = Mathf.Atan2(-d.z, d.x) * Mathf.Rad2Deg;
                return (ang % 360f + 360f) % 360f;
            case Parametro.ExplosionFuerza: return manager.explosionStrength;
            case Parametro.ExplosionRadio: return manager.explosionRadius;
            case Parametro.MagnetismoG: return manager.magnetismoG;
            case Parametro.MagnetismoRadio: return manager.magnetismoRadius;
            case Parametro.VorticeTangencial: return manager.vorticeTangential;
            case Parametro.VorticeRadio: return manager.vorticeRadius;
            case Parametro.VorticeElevacion: return manager.vorticeLift;
            case Parametro.RepulsionFuerza: return manager.repulsionStrength;
            default: return manager.repulsionRadius;
        }
    }

    void Escribir(Parametro p, float v)
    {
        switch (p)
        {
            case Parametro.VientoFuerza: manager.windStrength = v; break;
            case Parametro.VientoTurbulencia: manager.windTurbulence = v; break;
            case Parametro.VientoDireccion: manager.windDirection = Quaternion.Euler(0f, v, 0f) * Vector3.right; break;
            case Parametro.ExplosionFuerza: manager.explosionStrength = v; break;
            case Parametro.ExplosionRadio: manager.explosionRadius = v; break;
            case Parametro.MagnetismoG: manager.magnetismoG = v; break;
            case Parametro.MagnetismoRadio: manager.magnetismoRadius = v; break;
            case Parametro.VorticeTangencial: manager.vorticeTangential = v; break;
            case Parametro.VorticeRadio: manager.vorticeRadius = v; break;
            case Parametro.VorticeElevacion: manager.vorticeLift = v; break;
            case Parametro.RepulsionFuerza: manager.repulsionStrength = v; break;
            case Parametro.RepulsionRadio: manager.repulsionRadius = v; break;
        }
    }
}
