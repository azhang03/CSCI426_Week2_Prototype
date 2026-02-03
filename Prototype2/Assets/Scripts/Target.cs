using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float rightSpawnX = 22.0f;
    [SerializeField] private float leftSpawnX = -22.0f;
    [SerializeField] private float minSpawnY = -7f;
    [SerializeField] private float maxSpawnY = 9f;
    
    [Header("Visual Settings")]
    [Tooltip("Color tint applied during danger phase (golden)")]
    [SerializeField] private Color dangerPhaseColor = new Color(1f, 0.85f, 0.2f, 1f);
    
    [Tooltip("How long the color transition takes")]
    [SerializeField] private float colorTransitionDuration = 0.5f;
    
    [Header("Audio")]
    [Tooltip("Sound when target is hit by arrow")]
    [SerializeField] private AudioClip hitSound;
    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 0.8f;
    
    private Score scoreManager;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool hasAppliedDangerColor = false;
    private float colorTransitionProgress = 0f;
    
    void Start()
    {
        // Find the Score component in the scene
        scoreManager = FindFirstObjectByType<Score>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Check if we should start with danger color (spawned during danger phase)
        if (scoreManager != null && scoreManager.IsInDangerPhase())
        {
            ApplyDangerColorInstant();
        }
    }
    
    void Update()
    {
        // Check for danger phase transition
        if (!hasAppliedDangerColor && scoreManager != null && scoreManager.IsInDangerPhase())
        {
            hasAppliedDangerColor = true;
        }
        
        // Smoothly transition to danger color
        if (hasAppliedDangerColor && colorTransitionProgress < 1f && spriteRenderer != null)
        {
            colorTransitionProgress += Time.deltaTime / colorTransitionDuration;
            colorTransitionProgress = Mathf.Clamp01(colorTransitionProgress);
            spriteRenderer.color = Color.Lerp(originalColor, dangerPhaseColor, colorTransitionProgress);
        }
    }
    
    void ApplyDangerColorInstant()
    {
        hasAppliedDangerColor = true;
        colorTransitionProgress = 1f;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = dangerPhaseColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to arrows
        if (other.CompareTag("Arrow"))
        {
            Debug.Log("HIT");
            
            // Play hit sound
            if (SoundManager.Instance != null && hitSound != null)
            {
                SoundManager.Instance.PlaySoundWithPitchVariation(hitSound, transform.position, hitVolume, 0.95f, 1.05f);
            }
            
            // Choose random side for new target
            bool spawnOnRight = Random.Range(0, 2) == 1;
            float X = spawnOnRight ? rightSpawnX : leftSpawnX;
            float Y = Random.Range(minSpawnY, maxSpawnY);
            
            // Spawn new target
            Target newTarget = Instantiate(this, new Vector3(X, Y, 0), Quaternion.identity);
            
            // Flip target to face the player (center of screen)
            // If on left side, flip horizontally so it faces right
            // If on right side, don't flip so it faces left
            SpriteRenderer newSR = newTarget.GetComponent<SpriteRenderer>();
            if (newSR != null)
            {
                newSR.flipX = !spawnOnRight; // Flip if on left side
            }
            
            // Update score with current point value
            if (scoreManager != null)
            {
                int points = scoreManager.GetCurrentPointValue();
                scoreManager.AddPoints(points, transform.position);
                Debug.Log($"Target hit! +{points} points");
            }
            
            // Destroy arrow and this target
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
