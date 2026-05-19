using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a collectible item (coin or relic).
/// Handles visual feedback, score popup, and pool-safe deactivation.
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    public string collectibleType = "Coin";
    public int scoreValue = 50;

    [Header("Rotation")]
    [Tooltip("Spin the collectible for visual appeal")]
    public bool spin = true;
    public float spinSpeed = 180f;

    [Header("Effects")]
    public GameObject collectParticlePrefab;
    public AudioClip  collectSound;

    protected bool _collected = false;

    private void Update()
    {
        if (spin && !_collected)
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    /// <summary>Called by PlayerCollision when this item is picked up.</summary>
    public virtual void OnCollected()
    {
        if (_collected) return;
        _collected = true;

        // Spawn particle effect
        if (collectParticlePrefab != null)
            Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);

        // Play sound via AudioManager if available, otherwise fallback
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCoinPickup();
        else if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Floating score popup
        ScorePopupSpawner.Instance?.SpawnPopup(scoreValue, transform.position);

        StartCoroutine(DeactivateAfterDelay(0.1f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        // Hide mesh immediately
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        yield return new WaitForSeconds(delay);

        _collected = false;

        // Re-enable renderers for pool reuse
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        gameObject.SetActive(false);
    }
}
