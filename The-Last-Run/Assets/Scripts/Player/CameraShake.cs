using UnityEngine;
using System.Collections;

/// <summary>
/// Phase 6 — Camera shake on collision and near-miss events.
/// Attach to the Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    public float collisionShakeDuration  = 0.4f;
    public float collisionShakeMagnitude = 0.25f;
    public float coinShakeDuration       = 0.08f;
    public float coinShakeMagnitude      = 0.04f;

    private Vector3 _originalLocalPos;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        Instance = this;
        _originalLocalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= OnGameOver;
    }

    private void OnGameOver() => Shake(collisionShakeDuration, collisionShakeMagnitude);

    public void Shake(float duration, float magnitude)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    public void ShakeOnCoin()
    {
        // Intentionally disabled — coin shake causes UI to vibrate
        // Only game-over shake is used
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            transform.localPosition = _originalLocalPos + Random.insideUnitSphere * strength;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalLocalPos;
    }
}
