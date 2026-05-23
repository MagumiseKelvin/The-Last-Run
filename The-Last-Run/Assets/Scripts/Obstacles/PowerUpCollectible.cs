using UnityEngine;

/// <summary>
/// Phase 4 — A special collectible that activates a power-up when picked up.
/// Extends the base Collectible class.
/// </summary>
public class PowerUpCollectible : Collectible
{
    [Header("Power-Up")]
    [Tooltip("Which power-up this collectible activates")]
    public PowerUpManager.PowerUpType powerUpType = PowerUpManager.PowerUpType.ScoreMultiplier;

    private void Awake()
    {
        collectibleType = "PowerUp";
        scoreValue = 0; // Power-ups don't give direct score
    }

    public new void OnCollected()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.ActivatePowerUp(powerUpType);
        }

        // Play relic pickup sound for power-ups
        AudioManager.Instance?.PlayRelicPickup();

        base.OnCollected();
    }
}
