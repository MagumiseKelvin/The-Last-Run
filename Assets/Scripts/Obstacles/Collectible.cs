using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a collectible item (coin or relic).
/// Handles visual feedback on collection and deactivation.
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
    public AudioClip collectSound;

    private bool _collected = false;

    private void Update()
    {
        if (spin && !_collected)
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    /// <summary>Called by PlayerCollision when this item is picked up.</summary>
    public void OnCollected()
    {
        if (_collected) return;
        _collected = true;

        // Spawn particle effect
        if (collectParticlePrefab != null)
            Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);

        // Play sound
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Hide and deactivate
        StartCoroutine(DeactivateAfterDelay(0.1f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        // Hide mesh immediately
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        yield return new WaitForSeconds(delay);

        _collected = false;

        // Re-enable renderers for when this is reused from pool
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = true;

        gameObject.SetActive(false);
    }
}
