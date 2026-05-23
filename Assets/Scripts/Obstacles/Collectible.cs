using UnityEngine;
using System.Collections;

/// <summary>
/// Collectible coin/relic. Spins, detects player proximity, awards score.
/// Uses both OnTriggerEnter AND distance-based detection for reliability
/// with Unity's CharacterController (which doesn't always fire OnTriggerEnter).
/// Tag this GameObject as "Collectible".
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Settings")]
    public string collectibleType = "Coin";
    public int    scoreValue      = 50;

    [Header("Rotation")]
    public bool  spin      = true;
    public float spinSpeed = 180f;

    [Header("Collection Distance")]
    [Tooltip("How close the player must be to auto-collect (fallback for CharacterController)")]
    public float collectRadius = 0.9f;

    [Header("Effects")]
    public GameObject collectParticlePrefab;
    public AudioClip  collectSound;

    protected bool _collected = false;
    private Transform _playerTransform;

    private void Start()
    {
        // Cache player transform for distance check
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Update()
    {
        if (_collected) return;

        // Spin
        if (spin)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        // Distance-based collection — reliable with CharacterController
        if (_playerTransform != null && GameManager.Instance != null
            && GameManager.Instance.IsGameRunning)
        {
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist <= collectRadius)
                TriggerCollection();
        }
    }

    // Also keep trigger-based as backup
    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (other.CompareTag("Player") || other.GetComponent<PlayerCollision>() != null)
            TriggerCollection();
    }

    private void TriggerCollection()
    {
        if (_collected) return;
        _collected = true;

        // Award score
        float multiplier = PowerUpManager.Instance != null
            ? PowerUpManager.Instance.GetScoreMultiplier() : 1f;
        ScoreManager.Instance?.AddCoinScore(multiplier);

        // Audio
        AudioManager.Instance?.PlayCoinPickup();

        // Camera shake
        CameraShake.Instance?.ShakeOnCoin();

        // Particle
        if (collectParticlePrefab != null)
            Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);

        StartCoroutine(DeactivateRoutine());
    }

    private IEnumerator DeactivateRoutine()
    {
        // Hide immediately
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        yield return new WaitForSeconds(0.1f);

        // Re-enable for pool reuse
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        _collected = false;
        gameObject.SetActive(false);
    }

    public virtual void OnCollected() => TriggerCollection();
}
