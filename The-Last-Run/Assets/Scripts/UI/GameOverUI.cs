using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Phase 5 — Standalone Game Over screen controller.
/// Shows final score, distance, coins, high score, and new best banner.
/// Animates in with a slide + fade effect.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Stats")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI coinsText;
    public GameObject      newBestBanner;

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button shareButton;       // optional — for WebGL share

    [Header("Animation")]
    public CanvasGroup canvasGroup;
    public RectTransform panelRect;
    public float slideInDistance = 80f;
    public float animDuration    = 0.5f;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        gameObject.SetActive(false);

        restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        mainMenuButton?.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
        shareButton?.onClick.AddListener(OnShare);
    }

    private void HandleGameOver()
    {
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return new WaitForSecondsRealtime(0.8f);

        gameObject.SetActive(true);
        PopulateStats();

        // Animate
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        Vector2 startPos = panelRect != null
            ? panelRect.anchoredPosition + Vector2.down * slideInDistance
            : Vector2.zero;
        Vector2 endPos = panelRect != null ? panelRect.anchoredPosition : Vector2.zero;

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);

            if (canvasGroup != null) canvasGroup.alpha = t;
            if (panelRect   != null) panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private void PopulateStats()
    {
        if (ScoreManager.Instance == null) return;

        if (finalScoreText != null)
            finalScoreText.text = $"{ScoreManager.Instance.CurrentScore:F0}";

        if (highScoreText != null)
            highScoreText.text = $"Best: {ScoreManager.Instance.HighScore:F0}";

        if (distanceText != null)
            distanceText.text = $"{ScoreManager.Instance.DistanceTraveled:F0}m";

        if (coinsText != null)
            coinsText.text = $"x{ScoreManager.Instance.CoinsCollected}";

        if (newBestBanner != null)
            newBestBanner.SetActive(ScoreManager.Instance.IsNewHighScore);
    }

    private void OnShare()
    {
        // WebGL share — copies score to clipboard
        if (ScoreManager.Instance != null)
        {
            string msg = $"I ran {ScoreManager.Instance.DistanceTraveled:F0}m and scored " +
                         $"{ScoreManager.Instance.CurrentScore:F0} in The Last Run!";
            GUIUtility.systemCopyBuffer = msg;
            Debug.Log("[GameOverUI] Score copied to clipboard: " + msg);
        }
    }
}
