using UnityEngine;

/// <summary>
/// A low obstacle (e.g. fallen pillar, low beam) that the player must slide under.
/// If the player is not sliding when they hit this, it triggers game over.
/// </summary>
public class ObstacleLowBeam : Obstacle
{
    private void Awake()
    {
        obstacleType = "LowBeam";
    }

    protected override void TriggerCollision(GameObject playerObject)
    {
        Debug.Log("[ObstacleLowBeam] Player hit a low beam — should have slid!");
        base.TriggerCollision(playerObject);
    }
}
