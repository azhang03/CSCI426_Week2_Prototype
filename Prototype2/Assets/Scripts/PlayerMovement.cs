using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpSpeed = 15f;
    
    [Header("Jump Settings")]
    [Tooltip("How many times can the player jump in the air (0 = no air jumps, 1 = double jump)")]
    [SerializeField] private int maxAirJumps = 1;
    
    [Header("Ground Detection")]
    [Tooltip("Layer(s) that count as ground")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("How far below the player to check for ground")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    
    [Header("Hitbox Settings")]
    [Tooltip("Scale of the hitbox relative to sprite (0.5 = half size, more forgiving)")]
    [Range(0.1f, 1f)]
    [SerializeField] private float hitboxScale = 0.7f;
    
    [Tooltip("Vertical offset for the hitbox (positive = up, useful to shrink from feet)")]
    [SerializeField] private float hitboxVerticalOffset = 0.1f;
    
    [Header("Audio")]
    [Tooltip("Sound when jumping")]
    [SerializeField] private AudioClip jumpSound;
    [Range(0f, 1f)]
    [SerializeField] private float jumpVolume = 0.5f;
    
    [Tooltip("Sound when taking damage")]
    [SerializeField] private AudioClip hurtSound;
    [Range(0f, 1f)]
    [SerializeField] private float hurtVolume = 0.7f;
    
    [Tooltip("Footstep sounds (multiple for variety, or just one)")]
    [SerializeField] private AudioClip[] walkSounds;
    [Range(0f, 1f)]
    [SerializeField] private float walkVolume = 0.4f;
    
    [Tooltip("Time between footstep sounds")]
    [SerializeField] private float footstepInterval = 0.35f;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    [SerializeField] private int health = 3;
    private Vector2 moveInput;

    private bool jumpHold = false;
    private bool jumped = false;
    private int airJumpsRemaining;
    private float footstepTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        airJumpsRemaining = maxAirJumps;
        
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody2D component!");
        }
        
        // Apply forgiving hitbox scaling
        if (boxCollider != null)
        {
            originalColliderSize = boxCollider.size;
            originalColliderOffset = boxCollider.offset;
            
            // Shrink the collider for more forgiving hits
            boxCollider.size = new Vector2(
                originalColliderSize.x * hitboxScale,
                originalColliderSize.y * hitboxScale
            );
            
            // Apply vertical offset (useful to pull hitbox up from feet)
            boxCollider.offset = new Vector2(
                originalColliderOffset.x,
                originalColliderOffset.y + hitboxVerticalOffset
            );
            
            Debug.Log($"Player hitbox scaled to {hitboxScale * 100}% of original size");
        }
    }
    
    private bool IsGrounded()
    {
        if (boxCollider == null) return false;
        
        // Cast a small box downward to check for ground
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        
        return hit.collider != null;
    }

    private void Update()
    {
        // Get WASD input using new Input System
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = Vector2.zero;

        if (keyboard.aKey.isPressed)
            moveInput.x -= 1;
        if (keyboard.dKey.isPressed)
            moveInput.x += 1;
        // Check if grounded and reset air jumps
        bool grounded = IsGrounded();
        if (grounded)
        {
            airJumpsRemaining = maxAirJumps;
        }
        
        if (keyboard.spaceKey.isPressed)
        {
            if(jumpHold == false)
            {
                // Can jump if grounded OR if we have air jumps remaining
                if (grounded)
                {
                    jumped = true;
                    PlayJumpSound();
                }
                else if (airJumpsRemaining > 0)
                {
                    jumped = true;
                    airJumpsRemaining--;
                    PlayJumpSound();
                }
            }
            jumpHold = true;
        }
        else
        {
            jumpHold = false;
        }

        // Handle footstep sounds
        HandleFootsteps(grounded);

        // Y is processed as force

        // Normalize to prevent faster diagonal movement
        // moveInput = moveInput.normalized;
    }
    
    private void HandleFootsteps(bool grounded)
    {
        // Only play footsteps when grounded and moving
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        
        if (grounded && isMoving)
        {
            footstepTimer -= Time.deltaTime;
            
            if (footstepTimer <= 0f)
            {
                PlayFootstepSound();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Reset timer when not moving or in air (so first step plays immediately when landing/starting)
            footstepTimer = 0f;
        }
    }
    
    private void PlayFootstepSound()
    {
        if (SoundManager.Instance == null || walkSounds == null || walkSounds.Length == 0) return;
        
        SoundManager.Instance.PlayRandomSound(walkSounds, transform.position, walkVolume);
    }

    private void FixedUpdate()
    {

        if (rb != null)
        {
            rb.linearVelocityX = moveInput.x * moveSpeed;
            if(jumped)
            {
                rb.linearVelocityY = jumpSpeed;
                jumped = false;
            }
        }
    }

    
    /// <summary>
    /// Apply damage to the player
    /// </summary>
    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"Player took {amount} damage! Health: {health}");
        
        // Play hurt sound
        if (SoundManager.Instance != null && hurtSound != null)
        {
            SoundManager.Instance.PlaySound(hurtSound, transform, hurtVolume);
        }
        
        if (health <= 0)
        {
            // Death will trigger its own stronger shake
            // Trigger death
            PlayerDeath death = GetComponent<PlayerDeath>();
            if (death != null)
            {
                death.TriggerDeath();
            }
        }
        else
        {
            // Light screen shake for non-lethal hit
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.HitShake();
            }
        }
    }
    
    private void PlayJumpSound()
    {
        if (SoundManager.Instance != null && jumpSound != null)
        {
            SoundManager.Instance.PlaySoundWithPitchVariation(jumpSound, transform.position, jumpVolume, 0.9f, 1.1f);
        }
    }
    
    /// <summary>
    /// Get current health
    /// </summary>
    public int GetHealth()
    {
        return health;
    }
}
