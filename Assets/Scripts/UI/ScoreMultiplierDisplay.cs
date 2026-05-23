using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Phase 6 — Animates the score text when points are earned.
/// Punches the scale up briefly on each score change.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreMultiplierDisplay : MonoBehaviour
{
    [Header("Punch Settings")]
    public float punchScale    = 1.25f;
    public float punchDuration = 0.15f;

    private TextMeshProUGUI _text;
    private Vector3         _baseScale;
    private Coroutine       _punchCoroutine;

    private void Awake()
    {
        _text      = GetComponent<TextMeshProUGUI>();
        _baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(float score)
    {
        _text.text = $"{score:F0}";
        if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
        _punchCoroutine = StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            float t = elapsed / punchDuration;
            float s = Mathf.Lerp(punchScale, 1f, t);
            transform.localScale = _baseScale * s;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = _baseScale;
    }
}
