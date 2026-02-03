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

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    [SerializeField] private int health = 3;
    private Vector2 moveInput;

    private bool jumpHold = false;
    private bool jumped = false;
    private int airJumpsRemaining;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        airJumpsRemaining = maxAirJumps;
        
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody2D component!");
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
                }
                else if (airJumpsRemaining > 0)
                {
                    jumped = true;
                    airJumpsRemaining--;
                }
            }
            jumpHold = true;
        }
        else
        {
            jumpHold = false;
        }

        // Y is processed as force

        // Normalize to prevent faster diagonal movement
        // moveInput = moveInput.normalized;
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
        
        if (health <= 0)
        {
            // Trigger death
            PlayerDeath death = GetComponent<PlayerDeath>();
            if (death != null)
            {
                death.TriggerDeath();
            }
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
