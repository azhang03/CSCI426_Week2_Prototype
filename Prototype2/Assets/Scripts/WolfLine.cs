using UnityEngine;

public class WolfLine : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Initial horizontal speed (can be overridden by spawner)")]
    [SerializeField] private float initialMoveSpeed = 15f;
    
    [Header("Audio")]
    [Tooltip("Sound when wolf hits the player")]
    [SerializeField] private AudioClip attackSound;
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 0.8f;
    
    private float moveSpeed;
    private int moveDirection = 1; // 1 = right, -1 = left
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        moveSpeed = initialMoveSpeed;
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Ensure it renders on top of everything
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
        }
    }

    void Update()
    {
        // Don't move while game is paused (e.g., during death animation)
        if (Time.timeScale == 0f) return;
        
        // Move horizontally
        transform.position += Vector3.right * moveDirection * moveSpeed * Time.deltaTime;
        
        // Destroy if off-screen
        if (mainCamera != null)
        {
            float cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
            float cameraX = mainCamera.transform.position.x;
            
            // Check if we've passed the opposite side of the screen
            if (moveDirection > 0 && transform.position.x > cameraX + cameraHalfWidth + 5f)
            {
                Destroy(gameObject);
            }
            else if (moveDirection < 0 && transform.position.x < cameraX - cameraHalfWidth - 5f)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Set the movement speed (called by spawner to apply difficulty scaling)
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    /// <summary>
    /// Set the movement direction (1 = moving right, -1 = moving left)
    /// </summary>
    public void SetDirection(int direction)
    {
        moveDirection = direction;
        
        // Flip the sprite if moving left (wolf faces right by default)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (direction < 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't process during death animation (time frozen)
        if (Time.timeScale == 0f) return;
        
        if (other.CompareTag("Player"))
        {
            // Play attack sound
            if (SoundManager.Instance != null && attackSound != null)
            {
                SoundManager.Instance.PlaySound(attackSound, transform, attackVolume);
            }
            
            // Instant kill - trigger death
            PlayerDeath death = other.GetComponent<PlayerDeath>();
            if (death != null)
            {
                death.TriggerDeath();
            }
            else
            {
                // Fallback: deal massive damage
                PlayerMovement player = other.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.TakeDamage(999);
                }
            }
        }
    }
    
    /// <summary>
    /// Set the attack sound (called by spawner to share sound across all wolves)
    /// </summary>
    public void SetAttackSound(AudioClip sound, float volume)
    {
        attackSound = sound;
        attackVolume = volume;
    }
}
