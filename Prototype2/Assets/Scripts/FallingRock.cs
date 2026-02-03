using UnityEngine;

public class FallingRock : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float fallSpeed = 10f;
    
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    
    private Rigidbody2D rb;
    private Camera mainCamera;

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
    /// Set the fall speed (called by spawner)
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
            }
            
            // Destroy when hitting ground or platforms
            Destroy(gameObject);
        }
    }
}
