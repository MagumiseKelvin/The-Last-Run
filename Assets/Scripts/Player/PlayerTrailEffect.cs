using UnityEngine;

/// <summary>
/// Phase 6 — Adds a speed trail behind the player using a TrailRenderer.
/// The trail intensity increases with game speed.
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class PlayerTrailEffect : MonoBehaviour
{
    [Header("Trail Settings")]
    public Color trailStartColor = new Color(0.2f, 0.7f, 1f, 0.8f);
    public Color trailEndColor   = new Color(0.2f, 0.7f, 1f, 0f);
    public float maxTrailTime    = 0.35f;
    public float minTrailTime    = 0.05f;

    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
        _trail.startWidth = 0.3f;
        _trail.endWidth   = 0f;
        _trail.time       = minTrailTime;
        _trail.material   = new Material(Shader.Find("Sprites/Default"));

        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(trailStartColor, 0f),
                new GradientColorKey(trailEndColor,   1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f,   1f)
            }
        );
        _trail.colorGradient = gradient;
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        float speed      = GameManager.Instance.GetCurrentSpeed();
        float maxSpeed   = GameManager.Instance.maxSpeed;
        float t          = Mathf.Clamp01(speed / maxSpeed);
        _trail.time      = Mathf.Lerp(minTrailTime, maxTrailTime, t);
        _trail.emitting  = GameManager.Instance.IsGameRunning;
    }
}
