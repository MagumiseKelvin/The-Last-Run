using UnityEngine;

/// <summary>
/// Bridges PlayerController to Unity's Animator component.
/// All animation parameter names are defined here as constants.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    // Animator parameter name constants
    private static readonly int PARAM_IS_RUNNING  = Animator.StringToHash("IsRunning");
    private static readonly int PARAM_IS_GROUNDED = Animator.StringToHash("IsGrounded");
    private static readonly int PARAM_JUMP        = Animator.StringToHash("Jump");
    private static readonly int PARAM_IS_SLIDING  = Animator.StringToHash("IsSliding");
    private static readonly int PARAM_LEAN        = Animator.StringToHash("Lean");   // -1, 0, 1
    private static readonly int PARAM_DEATH       = Animator.StringToHash("Death");

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart += HandleGameStart;
            GameManager.Instance.OnGameOver  += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStart -= HandleGameStart;
            GameManager.Instance.OnGameOver  -= HandleGameOver;
        }
    }

    private void HandleGameStart()
    {
        _animator.SetBool(PARAM_IS_RUNNING, true);
    }

    private void HandleGameOver()
    {
        _animator.SetBool(PARAM_IS_RUNNING, false);
    }

    /// <summary>Update grounded state every frame from PlayerController.</summary>
    public void SetGrounded(bool grounded)
    {
        _animator.SetBool(PARAM_IS_GROUNDED, grounded);
    }

    /// <summary>Trigger the jump animation.</summary>
    public void PlayJump()
    {
        _animator.SetTrigger(PARAM_JUMP);
    }

    /// <summary>Enable or disable the slide animation.</summary>
    public void PlaySlide(bool sliding)
    {
        _animator.SetBool(PARAM_IS_SLIDING, sliding);
    }

    /// <summary>Play a lean animation when switching lanes. direction: -1 left, 1 right.</summary>
    public void PlayLeanAnimation(int direction)
    {
        _animator.SetFloat(PARAM_LEAN, direction);
        // Reset lean after a short delay
        CancelInvoke(nameof(ResetLean));
        Invoke(nameof(ResetLean), 0.3f);
    }

    private void ResetLean()
    {
        _animator.SetFloat(PARAM_LEAN, 0f);
    }

    /// <summary>Trigger the death/collision animation.</summary>
    public void PlayDeath()
    {
        _animator.SetTrigger(PARAM_DEATH);
    }
}
