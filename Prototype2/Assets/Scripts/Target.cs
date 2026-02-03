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
            
            // Choose random side for new target
            float X = Random.Range(0, 2) == 1 ? rightSpawnX : leftSpawnX;
            float Y = Random.Range(minSpawnY, maxSpawnY);
            
            // Spawn new target
            Instantiate(this, new Vector3(X, Y, 0), Quaternion.identity);
            
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
