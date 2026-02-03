using UnityEngine;

public class FallingRock : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Initial fall speed (can be overridden by spawner)")]
    [SerializeField] private float initialFallSpeed = 10f;
    
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    
    [Header("Audio")]
    [Tooltip("Sound when rock hits the ground")]
    [SerializeField] private AudioClip impactSound;
    [Range(0f, 1f)]
    [SerializeField] private float impactVolume = 0.6f;
    
    [Tooltip("Sound when rock hits and breaks a platform")]
    [SerializeField] private AudioClip platformHitSound;
    [Range(0f, 1f)]
    [SerializeField] private float platformHitVolume = 0.7f;
    
    private float fallSpeed;
    
    private Rigidbody2D rb;
    private Camera mainCamera;

    void Awake()
    {
        // Initialize fall speed from the serialized initial value
        fallSpeed = initialFallSpeed;
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        // Set falling velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * fallSpeed;
        }
    }
    
    /// <summary>
    /// Set the fall speed (called by spawner to override initial value)
    /// </summary>
    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.down * fallSpeed;
        }
    }

    void Update()
    {
        // Don't destroy anything while game is paused (e.g., during death animation)
        if (Time.timeScale == 0f) return;
        
        // Destroy if fallen below camera view
        if (mainCamera != null)
        {
            float cameraBottom = mainCamera.transform.position.y - mainCamera.orthographicSize - 2f;
            if (transform.position.y < cameraBottom)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't process during death animation (time frozen)
        if (Time.timeScale == 0f) return;
        
        if (other.CompareTag("Player"))
        {
            // Damage the player
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            // Only destroy if death wasn't triggered (timeScale would be 0)
            if (Time.timeScale > 0f)
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Don't process during death animation (time frozen)
        if (Time.timeScale == 0f) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            // Damage the player
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            // Only destroy if death wasn't triggered (timeScale would be 0)
            if (Time.timeScale > 0f)
            {
                Destroy(gameObject);
            }
        }
        // Arrows don't destroy rocks - they just get blocked
        else if (!collision.gameObject.CompareTag("Target") && !collision.gameObject.CompareTag("Arrow"))
        {
            // Check if we hit a platform (has "Platform" in name) - damage it
            if (collision.gameObject.name.Contains("Platform"))
            {
                DamageablePlatform platform = collision.gameObject.GetComponent<DamageablePlatform>();
                if (platform != null)
                {
                    platform.TriggerDamage();
                }
                
                // Play platform break sound
                if (SoundManager.Instance != null && platformHitSound != null)
                {
                    SoundManager.Instance.PlaySoundWithPitchVariation(platformHitSound, transform.position, platformHitVolume, 0.9f, 1.1f);
                }
            }
            else
            {
                // Play regular ground impact sound
                if (SoundManager.Instance != null && impactSound != null)
                {
                    SoundManager.Instance.PlaySoundWithPitchVariation(impactSound, transform.position, impactVolume, 0.85f, 1.15f);
                }
            }
            
            // Destroy when hitting ground or platforms
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Set the impact sound (called by spawner to share sound across all rocks)
    /// </summary>
    public void SetImpactSound(AudioClip sound, float volume)
    {
        impactSound = sound;
        impactVolume = volume;
    }
    
    /// <summary>
    /// Set the platform hit sound (called by spawner)
    /// </summary>
    public void SetPlatformHitSound(AudioClip sound, float volume)
    {
        platformHitSound = sound;
        platformHitVolume = volume;
    }
}
