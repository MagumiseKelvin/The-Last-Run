using UnityEngine;

/// <summary>
/// Phase 6 — Scrolls background environment objects (clouds, distant mountains)
/// to give a sense of depth and speed without affecting gameplay.
/// Attach to any background decoration parent.
/// </summary>
public class EnvironmentScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("How fast this object scrolls relative to game speed (0-1)")]
    [Range(0f, 1f)]
    public float speedMultiplier = 0.3f;

    [Tooltip("Z position to reset to when object goes too far behind")]
    public float resetZ = 200f;
    [Tooltip("Z position that triggers a reset")]
    public float despawnZ = -50f;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        float speed = GameManager.Instance.GetCurrentSpeed() * speedMultiplier;
        transform.Translate(0f, 0f, -speed * Time.deltaTime, Space.World);

        if (transform.position.z < despawnZ)
        {
            Vector3 pos = transform.position;
            pos.z = resetZ;
            transform.position = pos;
        }
    }
}
