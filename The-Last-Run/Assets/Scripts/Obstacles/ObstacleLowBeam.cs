using UnityEngine;

/// <summary>
/// Low horizontal beam — player must slide under.
/// Tag: "Obstacle"
/// </summary>
public class ObstacleLowBeam : Obstacle
{
    private void Awake() => obstacleType = "LowBeam";

    public override void OnHit()
    {
        Debug.Log("[ObstacleLowBeam] Player hit a low beam — should have slid!");
        base.OnHit();
    }
}
