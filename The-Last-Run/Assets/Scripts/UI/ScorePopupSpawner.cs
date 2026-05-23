using UnityEngine;

/// <summary>
/// Phase 5 — Spawns floating score text popups when coins are collected.
/// Place this on the HUD Canvas. Assign the FloatingScoreText prefab.
/// </summary>
public class ScorePopupSpawner : MonoBehaviour
{
    public static ScorePopupSpawner Instance { get; private set; }

    [Header("Prefab")]
    [Tooltip("FloatingScoreText prefab — must be a UI element (child of Canvas)")]
    public GameObject floatingScoreTextPrefab;

    [Header("References")]
    public Canvas hudCanvas;
    public Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    /// <summary>
    /// Spawns a floating score popup at the given world position.
    /// </summary>
    public void SpawnPopup(int score, Vector3 worldPosition)
    {
        if (floatingScoreTextPrefab == null || hudCanvas == null) return;

        GameObject popup = Instantiate(floatingScoreTextPrefab, hudCanvas.transform);
        FloatingScoreText fst = popup.GetComponent<FloatingScoreText>();
        fst?.Initialize(score, worldPosition, mainCamera);
    }
}
