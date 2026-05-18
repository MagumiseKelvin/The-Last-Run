using UnityEngine;
using System.Collections;

/// <summary>
/// Handles all player input and movement:
/// lane switching (left/right), jumping, and sliding.
/// The player does not move forward — the world moves toward the player.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ── Lane Settings ────────────────────────────────────────────────────────
    [Header("Lane Settings")]
    [Tooltip("Horizontal distance between lanes")]
    public float laneWidth = 2.5f;
    [Tooltip("Total number of lanes (odd number recommended, e.g. 3)")]
    public int laneCount = 3;
    [Tooltip("Speed at which the player slides between lanes")]
    public float laneSwitchSpeed = 12f;

    // ── Jump Settings ────────────────────────────────────────────────────────
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float gravity = -25f;

    // ── Slide Settings ───────────────────────────────────────────────────────
    [Header("Slide Settings")]
    [Tooltip("Duration of the slide in seconds")]
    public float slideDuration = 0.8f;
    [Tooltip("Collider height when sliding")]
    public float slideColliderHeight = 0.6f;
    [Tooltip("Collider center Y when sliding")]
    public float slideColliderCenterY = 0.3f;

    // ── Private State ────────────────────────────────────────────────────────
    private CharacterController _controller;
    private PlayerAnimator _animator;

    private int _currentLane;           // 0 = left, 1 = center, 2 = right
    private int _centerLane;
    private float _targetX;
    private float _verticalVelocity;
    private bool _isSliding;
    private Coroutine _slideCoroutine;

    // Default collider values (restored after slide)
    private float _defaultColliderHeight;
    private Vector3 _defaultColliderCenter;

    // Input cooldown to prevent double-tap spam
    private float _inputCooldown = 0.1f;
    private float _lastInputTime;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<PlayerAnimator>();

        _centerLane = laneCount / 2;
        _currentLane = _centerLane;
        _targetX = LaneToX(_currentLane);

        _defaultColliderHeight = _controller.height;
        _defaultColliderCenter = _controller.center;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameRunning) return;

        HandleInput();
        MovePlayer();
    }

    // ── Input ────────────────────────────────────────────────────────────────

    private void HandleInput()
    {
        if (Time.time - _lastInputTime < _inputCooldown) return;

        // Lane switch — Left
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            ShiftLane(-1);
        }
        // Lane switch — Right
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            ShiftLane(1);
        }
        // Jump
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            TryJump();
        }
        // Slide
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            TrySlide();
        }
    }

    private void ShiftLane(int direction)
    {
        int newLane = Mathf.Clamp(_currentLane + direction, 0, laneCount - 1);
        if (newLane == _currentLane) return;

        _currentLane = newLane;
        _targetX = LaneToX(_currentLane);
        _lastInputTime = Time.time;

        _animator?.PlayLeanAnimation(direction);
    }

    private void TryJump()
    {
        if (!_controller.isGrounded) return;
        if (_isSliding) StopSlide();

        _verticalVelocity = jumpForce;
        _lastInputTime = Time.time;
        _animator?.PlayJump();
    }

    private void TrySlide()
    {
        if (_isSliding) return;
        if (!_controller.isGrounded) return;

        _lastInputTime = Time.time;

        if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
        _slideCoroutine = StartCoroutine(SlideRoutine());
    }

    // ── Movement ─────────────────────────────────────────────────────────────

    private void MovePlayer()
    {
        // Horizontal — smoothly slide to target lane
        Vector3 pos = transform.position;
        float newX = Mathf.MoveTowards(pos.x, _targetX, laneSwitchSpeed * Time.deltaTime);

        // Vertical — apply gravity
        if (_controller.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f; // keep grounded
        }
        _verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = new Vector3(newX - pos.x, _verticalVelocity * Time.deltaTime, 0f);
        _controller.Move(move);

        // Update animator grounded state
        _animator?.SetGrounded(_controller.isGrounded);
    }

    // ── Slide Coroutine ───────────────────────────────────────────────────────

    private IEnumerator SlideRoutine()
    {
        _isSliding = true;
        _animator?.PlaySlide(true);

        // Shrink collider
        _controller.height = slideColliderHeight;
        _controller.center = new Vector3(0f, slideColliderCenterY, 0f);

        yield return new WaitForSeconds(slideDuration);

        StopSlide();
    }

    private void StopSlide()
    {
        _isSliding = false;
        _animator?.PlaySlide(false);

        // Restore collider
        _controller.height = _defaultColliderHeight;
        _controller.center = _defaultColliderCenter;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Converts a lane index to a world X position.</summary>
    private float LaneToX(int lane)
    {
        // Center lane is at X=0, lanes spread out by laneWidth
        return (lane - _centerLane) * laneWidth;
    }

    /// <summary>Called by obstacle collision to trigger game over.</summary>
    public void OnHitObstacle()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver();

        _animator?.PlayDeath();
    }
}
