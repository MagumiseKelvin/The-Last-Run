using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Phase 5 — Visual speed lines effect on the HUD.
/// Increases opacity as the game speed increases, giving a sense of velocity.
/// Attach to a UI Image with a radial speed lines sprite.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SpeedLinesEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Speed at which lines start appearing")]
    public float minSpeed = 10f;
    [Tooltip("Speed at which lines reach full opacity")]
    public float maxSpeed = 25f;
    [Tooltip("Maximum alpha of the speed lines")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.5f;
    [Tooltip("How fast the alpha transitions")]
    public float smoothSpeed = 3f;

    private CanvasGroup _canvasGroup;
    private float _targetAlpha;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning)
        {
            _targetAlpha = 0f;
        }
        else
        {
            float speed = GameManager.Instance.GetCurrentSpeed();
            float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
            _targetAlpha = t * maxAlpha;
        }

        _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _targetAlpha, smoothSpeed * Time.deltaTime);
    }
}
