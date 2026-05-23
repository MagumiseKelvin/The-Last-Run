using UnityEngine;

/// <summary>
/// Phase 6 — Adds subtle animation to obstacles to make them more visible.
/// Barriers pulse with a glow effect; low beams flash red.
/// </summary>
public class ObstacleAnimator : MonoBehaviour
{
    [Header("Pulse Settings")]
    public bool  pulse         = true;
    public float pulseSpeed    = 2.5f;
    public float pulseMinScale = 0.97f;
    public float pulseMaxScale = 1.03f;

    [Header("Flash Settings")]
    public bool  flash          = false;
    public float flashSpeed     = 4f;
    public Color flashColorA    = Color.red;
    public Color flashColorB    = new Color(1f, 0.5f, 0.5f);

    private Vector3    _baseScale;
    private Renderer[] _renderers;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _renderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        if (pulse)
        {
            float s = Mathf.Lerp(pulseMinScale, pulseMaxScale,
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            transform.localScale = _baseScale * s;
        }

        if (flash && _renderers != null)
        {
            float t   = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
            Color col = Color.Lerp(flashColorA, flashColorB, t);
            foreach (var r in _renderers)
            {
                if (r.material.HasProperty("_BaseColor"))
                    r.material.SetColor("_BaseColor", col);
                else
                    r.material.color = col;
            }
        }
    }
}
