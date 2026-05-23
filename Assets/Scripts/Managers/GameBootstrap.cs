using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Phase 8 prep — Bootstraps the game on first load.
/// Ensures AudioManager persists across scenes.
/// Place this on a GameObject in the first scene (MainMenu or Game).
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    private static bool _initialized = false;

    private void Awake()
    {
        if (_initialized) { Destroy(gameObject); return; }
        _initialized = true;
        DontDestroyOnLoad(gameObject);

        // Set target frame rate for smooth gameplay
        Application.targetFrameRate = 60;

        // WebGL specific settings
#if UNITY_WEBGL
        // Disable cursor lock on WebGL
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
#endif

        Debug.Log("[GameBootstrap] Game initialized.");
    }
}
