using UnityEngine;

/// <summary>
/// Smooth camera that follows the player.
/// Stays behind and slightly above the player for a classic runner perspective.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    [Tooltip("Camera position relative to the player")]
    public Vector3 offset = new Vector3(0f, 3f, -6f);

    [Header("Smoothing")]
    [Tooltip("How smoothly the camera follows on X (lane switching)")]
    public float horizontalSmoothSpeed = 10f;
    [Tooltip("How smoothly the camera follows on Y (jumping)")]
    public float verticalSmoothSpeed = 8f;

    [Header("Look At")]
    [Tooltip("Offset from player that the camera looks toward")]
    public Vector3 lookAtOffset = new Vector3(0f, 1f, 5f);

    private Vector3 _currentVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        // Desired position
        Vector3 desiredPosition = target.position + offset;

        // Smooth X separately (follows lane switches)
        float smoothX = Mathf.Lerp(transform.position.x, desiredPosition.x, horizontalSmoothSpeed * Time.deltaTime);
        // Smooth Y (follows jumps)
        float smoothY = Mathf.Lerp(transform.position.y, desiredPosition.y, verticalSmoothSpeed * Time.deltaTime);
        // Z stays fixed relative to player
        float smoothZ = desiredPosition.z;

        transform.position = new Vector3(smoothX, smoothY, smoothZ);

        // Look slightly ahead of the player
        transform.LookAt(target.position + lookAtOffset);
    }
}
