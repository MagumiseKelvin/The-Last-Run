using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Phase 6 — Animates HUD elements for a modern feel:
/// - Coin counter bounces when a coin is collected
/// - Distance text pulses every 100m milestone
/// - Speed text color shifts from white to red as speed increases
/// </summary>
public class HUDAnimator : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI speedText;

    [Header("Coin Bounce")]
    public float coinBounceScale    = 1.4f;
    public float coinBounceDuration = 0.2f;

    [Header("Speed Color")]
    public Color speedColorSlow = Color.white;
    public Color speedColorFast = new Color(1f, 0.35f, 0.35f);

    private int       _lastCoinCount   = 0;
    private float     _lastDistance    = 0f;
    private float     _lastMilestone   = 0f;
    private Coroutine _coinCoroutine;
    private Coroutine _distCoroutine;

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;
        if (ScoreManager.Instance == null) return;

        // Coin bounce
        int coins = ScoreManager.Instance.CoinsCollected;
        if (coins != _lastCoinCount)
        {
            _lastCoinCount = coins;
            if (coinText != null)
            {
                coinText.text = $"Coins: {coins}";
                if (_coinCoroutine != null) StopCoroutine(_coinCoroutine);
                _coinCoroutine = StartCoroutine(BounceText(coinText.transform, coinBounceScale, coinBounceDuration));
            }
        }

        // Distance milestone pulse every 100m
        float dist = ScoreManager.Instance.DistanceTraveled;
        if (distanceText != null)
            distanceText.text = $"{dist:F0}m";

        float milestone = Mathf.Floor(dist / 100f) * 100f;
        if (milestone > _lastMilestone && dist > 50f)
        {
            _lastMilestone = milestone;
            if (_distCoroutine != null) StopCoroutine(_distCoroutine);
            _distCoroutine = StartCoroutine(BounceText(distanceText.transform, 1.5f, 0.3f));
        }

        // Speed color shift
        if (speedText != null)
        {
            float speed    = GameManager.Instance.GetCurrentSpeed();
            float maxSpeed = GameManager.Instance.maxSpeed;
            float t        = Mathf.Clamp01(speed / maxSpeed);
            speedText.color = Color.Lerp(speedColorSlow, speedColorFast, t);
            speedText.text  = $"{speed:F0} m/s";
        }
    }

    private IEnumerator BounceText(Transform t, float targetScale, float duration)
    {
        Vector3 baseScale = Vector3.one;
        float   elapsed   = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            // Ease out bounce
            float s = Mathf.Lerp(targetScale, 1f, progress);
            t.localScale = baseScale * s;
            yield return null;
        }
        t.localScale = baseScale;
    }
}
