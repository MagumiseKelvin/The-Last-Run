using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Phase 5 — A floating "+50" text that pops up at the coin position
/// and floats upward before fading out.
/// Attach to a Canvas-space prefab with a TextMeshProUGUI component.
/// </summary>
public class FloatingScoreText : MonoBehaviour
{
    [Header("Settings")]
    public float floatSpeed   = 80f;
    public float fadeDuration = 0.8f;
    public float scalePunch   = 1.4f;

    private TextMeshProUGUI _text;
    private CanvasGroup     _canvasGroup;
    private RectTransform   _rect;

    private void Awake()
    {
        _text        = GetComponent<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rect        = GetComponent<RectTransform>();

        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// Initialize the popup with a score value and world position.
    /// Call this right after instantiating the prefab.
    /// </summary>
    public void Initialize(int score, Vector3 worldPosition, Camera cam)
    {
        if (_text != null)
            _text.text = $"+{score}";

        // Convert world position to screen/canvas position
        if (cam != null)
        {
            Vector2 screenPos = cam.WorldToScreenPoint(worldPosition);
            _rect.position = screenPos;
        }

        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        float elapsed = 0f;

        // Scale punch
        transform.localScale = Vector3.one * scalePunch;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Float upward
            _rect.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;

            // Fade out in second half
            if (t > 0.5f)
                _canvasGroup.alpha = 1f - ((t - 0.5f) / 0.5f);

            // Scale back to normal
            float scale = Mathf.Lerp(scalePunch, 1f, t * 3f);
            transform.localScale = Vector3.one * Mathf.Max(scale, 1f);

            yield return null;
        }

        Destroy(gameObject);
    }
}
