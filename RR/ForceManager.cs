using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Maneja las 5 fuerzas con el mismo patrón que PhysicsPropertyManager.
/// Tecla = toggle fuerza, P = panel de propiedades con sliders.
/// </summary>
public class ForceManager : MonoBehaviour
{
    [Header("── VIENTO ──────────────────")]
    [Tooltip("Tecla para activar/desactivar el viento.")]
    public KeyCode vientoKey = KeyCode.H;
    public WindForceGenerator vientoGenerador;
    public TextMeshProUGUI vientoKeyText;
    public Image vientoOnImage;
    public Image vientoOffImage;

    [Header("── EXPLOSIÓN ───────────────")]
    [Tooltip("Tecla para detonar la explosión.")]
    public KeyCode explosionKey = KeyCode.J;
    public ExplosionForceGenerator explosionGenerador;
    public TextMeshProUGUI explosionKeyText;
    public Image explosionImage;

    [Header("── MAGNETISMO ──────────────")]
    [Tooltip("Tecla para activar/desactivar el magnetismo.")]
    public KeyCode magnetismoKey = KeyCode.K;
    public MagnetismForceGenerator magnetismoGenerador;
    public TextMeshProUGUI magnetismoKeyText;
    public Image magnetismoOnImage;
    public Image magnetismoOffImage;

    [Header("── VÓRTICE ─────────────────")]
    [Tooltip("Tecla para activar/desactivar el vórtice.")]
    public KeyCode vorticeKey = KeyCode.L;
    public VortexForceGenerator vorticeGenerador;
    public TextMeshProUGUI vorticeKeyText;
    public Image vorticeOnImage;
    public Image vorticeOffImage;

    [Header("── REPULSIÓN ───────────────")]
    [Tooltip("Tecla para activar/desactivar la repulsión.")]
    public KeyCode repulsionKey = KeyCode.N;
    public RepulsionForceGenerator repulsionGenerador;
    public TextMeshProUGUI repulsionKeyText;
    public Image repulsionOnImage;
    public Image repulsionOffImage;

    [Header("── PANEL PROPIEDADES ───────")]
    [Tooltip("Tecla para abrir/cerrar el panel de propiedades.")]
    public KeyCode panelKey = KeyCode.P;
    public GameObject panelPropiedades;
    public TextMeshProUGUI panelKeyText;

    [Header("Sliders — Viento")]
    public Slider sliderVientoFuerza;
    public Slider sliderVientoTurbulencia;
    public TextMeshProUGUI textoVientoFuerza;
    public TextMeshProUGUI textoVientoTurbulencia;

    [Header("Sliders — Explosión")]
    public Slider sliderExplosionFuerza;
    public Slider sliderExplosionRadio;
    public TextMeshProUGUI textoExplosionFuerza;
    public TextMeshProUGUI textoExplosionRadio;

    [Header("Sliders — Magnetismo")]
    public Slider sliderMagnetismoG;
    public Slider sliderMagnetismoRadio;
    public TextMeshProUGUI textoMagnetismoG;
    public TextMeshProUGUI textoMagnetismoRadio;

    [Header("Sliders — Vórtice")]
    public Slider sliderVorticeRotacion;
    public Slider sliderVorticeRadio;
    public TextMeshProUGUI textoVorticeRotacion;
    public TextMeshProUGUI textoVorticeRadio;

    [Header("Sliders — Repulsión")]
    public Slider sliderRepulsionFuerza;
    public Slider sliderRepulsionRadio;
    public TextMeshProUGUI textoRepulsionFuerza;
    public TextMeshProUGUI textoRepulsionRadio;

    // Toggles internos
    bool vientoToggle;
    bool magnetismoToggle;
    bool vorticeToggle;
    bool repulsionToggle;
    bool panelToggle;

    void Start()
    {
        // Textos de teclas en UI — igual que PhysicsPropertyManager
        if (vientoKeyText)    vientoKeyText.text    = vientoKey.ToString().ToUpper();
        if (explosionKeyText) explosionKeyText.text  = explosionKey.ToString().ToUpper();
        if (magnetismoKeyText)magnetismoKeyText.text = magnetismoKey.ToString().ToUpper();
        if (vorticeKeyText)   vorticeKeyText.text    = vorticeKey.ToString().ToUpper();
        if (repulsionKeyText) repulsionKeyText.text  = repulsionKey.ToString().ToUpper();
        if (panelKeyText)     panelKeyText.text      = panelKey.ToString().ToUpper();

        // Todas inician desactivadas
        SetGenerador(vientoGenerador,     false);
        SetGenerador(magnetismoGenerador, false);
        SetGenerador(vorticeGenerador,    false);
        SetGenerador(repulsionGenerador,  false);
        SetGenerador(explosionGenerador,  false);

        // Iconos iniciales
        SetImages(vientoOnImage,     vientoOffImage,     false);
        SetImages(magnetismoOnImage, magnetismoOffImage, false);
        SetImages(vorticeOnImage,    vorticeOffImage,    false);
        SetImages(repulsionOnImage,  repulsionOffImage,  false);
        SetSingleImage(explosionImage, false);

        // Panel cerrado
        if (panelPropiedades) panelPropiedades.SetActive(false);

        // Sliders
        ConfigurarSliders();
    }

    void Update()
    {
        // Viento
        if (Input.GetKeyUp(vientoKey))
        {
            vientoToggle = !vientoToggle;
            SetGenerador(vientoGenerador, vientoToggle);
            SetImages(vientoOnImage, vientoOffImage, vientoToggle);
        }

        // Explosión — pulso único
        if (Input.GetKeyUp(explosionKey) && explosionGenerador != null)
        {
            explosionGenerador.Detonate();
            SetSingleImage(explosionImage, true);
            Invoke(nameof(ResetExplosionImage), 0.3f);
        }

        // Magnetismo
        if (Input.GetKeyUp(magnetismoKey))
        {
            magnetismoToggle = !magnetismoToggle;
            SetGenerador(magnetismoGenerador, magnetismoToggle);
            SetImages(magnetismoOnImage, magnetismoOffImage, magnetismoToggle);
        }

        // Vórtice
        if (Input.GetKeyUp(vorticeKey))
        {
            vorticeToggle = !vorticeToggle;
            SetGenerador(vorticeGenerador, vorticeToggle);
            SetImages(vorticeOnImage, vorticeOffImage, vorticeToggle);
        }

        // Repulsión
        if (Input.GetKeyUp(repulsionKey))
        {
            repulsionToggle = !repulsionToggle;
            SetGenerador(repulsionGenerador, repulsionToggle);
            SetImages(repulsionOnImage, repulsionOffImage, repulsionToggle);
        }

        // Panel de propiedades
        if (Input.GetKeyUp(panelKey))
        {
            panelToggle = !panelToggle;
            if (panelPropiedades) panelPropiedades.SetActive(panelToggle);
        }
    }

    // ─── Helpers ────────────────────────────────────────

    void SetGenerador(MonoBehaviour gen, bool activo)
    {
        if (gen != null) gen.enabled = activo;
    }

    /// Mismo patrón que setGravityImages en PhysicsPropertyManager
    void SetImages(Image onImg, Image offImg, bool activo)
    {
        if (onImg != null)
        {
            var c = onImg.color;
            c.a = activo ? 1f : 0.3f;
            onImg.color = c;
        }
        if (offImg != null)
        {
            var c = offImg.color;
            c.a = activo ? 0.3f : 1f;
            offImg.color = c;
        }
    }

    /// Mismo patrón que setTimeImage en PhysicsPropertyManager
    void SetSingleImage(Image img, bool activo)
    {
        if (img == null) return;
        var c = img.color;
        c.a = activo ? 1f : 0.3f;
        img.color = c;
    }

    void ResetExplosionImage() => SetSingleImage(explosionImage, false);

    // ─── Sliders ─────────────────────────────────────────

    void ConfigurarSliders()
    {
        // Viento
        if (vientoGenerador != null)
        {
            ConfigSlider(sliderVientoFuerza, 0, 30, vientoGenerador.strength, v => {
                vientoGenerador.strength = v;
                if (textoVientoFuerza) textoVientoFuerza.text = $"Fuerza: {v:F1}";
            });
            ConfigSlider(sliderVientoTurbulencia, 0, 10, vientoGenerador.turbulenceStrength, v => {
                vientoGenerador.turbulenceStrength = v;
                if (textoVientoTurbulencia) textoVientoTurbulencia.text = $"Turbulencia: {v:F1}";
            });
        }

        // Explosión
        if (explosionGenerador != null)
        {
            ConfigSlider(sliderExplosionFuerza, 100, 2000, explosionGenerador.strength, v => {
                explosionGenerador.strength = v;
                if (textoExplosionFuerza) textoExplosionFuerza.text = $"Fuerza: {v:F0}";
            });
            ConfigSlider(sliderExplosionRadio, 1, 20, explosionGenerador.radius, v => {
                explosionGenerador.radius = v;
                if (textoExplosionRadio) textoExplosionRadio.text = $"Radio: {v:F1}";
            });
        }

        // Magnetismo
        if (magnetismoGenerador != null)
        {
            ConfigSlider(sliderMagnetismoG, -50, 50, magnetismoGenerador.G, v => {
                magnetismoGenerador.G = v;
                if (textoMagnetismoG) textoMagnetismoG.text = $"G: {v:F1}";
            });
            ConfigSlider(sliderMagnetismoRadio, 1, 20, magnetismoGenerador.radius, v => {
                magnetismoGenerador.radius = v;
                if (textoMagnetismoRadio) textoMagnetismoRadio.text = $"Radio: {v:F1}";
            });
        }

        // Vórtice
        if (vorticeGenerador != null)
        {
            ConfigSlider(sliderVorticeRotacion, 0, 40, vorticeGenerador.tangentialStrength, v => {
                vorticeGenerador.tangentialStrength = v;
                if (textoVorticeRotacion) textoVorticeRotacion.text = $"Rotación: {v:F1}";
            });
            ConfigSlider(sliderVorticeRadio, 1, 20, vorticeGenerador.radius, v => {
                vorticeGenerador.radius = v;
                if (textoVorticeRadio) textoVorticeRadio.text = $"Radio: {v:F1}";
            });
        }

        // Repulsión
        if (repulsionGenerador != null)
        {
            ConfigSlider(sliderRepulsionFuerza, 0, 100, repulsionGenerador.strength, v => {
                repulsionGenerador.strength = v;
                if (textoRepulsionFuerza) textoRepulsionFuerza.text = $"Fuerza: {v:F1}";
            });
            ConfigSlider(sliderRepulsionRadio, 1, 20, repulsionGenerador.radius, v => {
                repulsionGenerador.radius = v;
                if (textoRepulsionRadio) textoRepulsionRadio.text = $"Radio: {v:F1}";
            });
        }
    }

    void ConfigSlider(Slider s, float min, float max, float valor, UnityEngine.Events.UnityAction<float> onChange)
    {
        if (s == null) return;
        s.minValue = min;
        s.maxValue = max;
        s.value = valor;
        s.onValueChanged.AddListener(onChange);
    }
}
