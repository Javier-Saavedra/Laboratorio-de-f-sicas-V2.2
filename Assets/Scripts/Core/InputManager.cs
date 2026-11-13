using System;
using UnityEngine;

/// <summary>
/// Version minima del InputManager para ESTE proyecto (usa el Input clasico, no el Input System).
/// Solo expone lo que necesita SimulationManager: OnPause y OnRestart.
/// En tu proyecto PhysicsSandbox se sigue usando el InputManager original.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action OnPause;
    public event Action OnRestart;

    [Tooltip("Pausa/reanuda la simulacion del motor.")]
    public KeyCode pauseKey = KeyCode.None;
    [Tooltip("Reinicia la simulacion del motor.")]
    public KeyCode restartKey = KeyCode.R;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey)) OnPause?.Invoke();
        if (Input.GetKeyDown(restartKey)) OnRestart?.Invoke();
    }
}
