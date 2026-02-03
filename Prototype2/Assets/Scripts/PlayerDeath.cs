using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Death Trigger")]
    [Tooltip("How much of the player (0-1) must pass below camera before death triggers. 0.5 = half the player")]
    [Range(0f, 1f)]
    [SerializeField] private float deathThreshold = 0.5f;

    [Header("Phase 1: Hitstop (Everything Freezes)")]
    [Tooltip("Duration of the complete freeze (Time.timeScale = 0)")]
    [SerializeField] private float hitstopDuration = 1f;

    [Header("Phase 2: Shake + Red Flash")]
    [Tooltip("Duration of the shake and color transition")]
    [SerializeField] private float shakeDuration = 1f;
    
    [Tooltip("Intensity of the shake/vibration")]
    [SerializeField] private float shakeIntensity = 0.1f;
    
    [Tooltip("Speed of the shake oscillation")]
    [SerializeField] private float shakeSpeed = 50f;
    
    [Tooltip("Target color to fade to")]
    [SerializeField] private Color deathColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Phase 3: Explosion")]
    [Tooltip("Explosion effect prefab (optional - will create default particles if not assigned)")]
    [SerializeField] private GameObject explosionEffectPrefab;
    
    [Tooltip("Scale of the explosion effect")]
    [SerializeField] private float explosionScale = 1.5f;
    
    [Tooltip("How long after explosion before restarting")]
    [SerializeField] private float restartDelay = 1f;
    
    [Header("Audio")]
    [Tooltip("Sound at the moment of death (hitstop)")]
    [SerializeField] private AudioClip deathHitSound;
    [Range(0f, 1f)]
    [SerializeField] private float deathHitVolume = 1f;
    
    [Tooltip("Sound for the explosion")]
    [SerializeField] private AudioClip explosionSound;
    [Range(0f, 1f)]
    [SerializeField] private float explosionVolume = 0.8f;

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private bool isDying = false;
    private Color originalColor;
    private Vector3 originalPosition;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        playerCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isDying) return;
        
        // Developer keybind: M to trigger death manually
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.mKey.wasPressedThisFrame)
        {
            TriggerDeath();
            return;
        }
        
        CheckDeathCondition();
    }

    void CheckDeathCondition()
    {
        if (mainCamera == null || playerCollider == null) return;

        // Get camera's bottom edge in world space
        float cameraBottom = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        
        // Get player bounds
        Bounds playerBounds = playerCollider.bounds;
        float playerHeight = playerBounds.size.y;
        float playerBottom = playerBounds.min.y;
        float playerTop = playerBounds.max.y;
        
        // Calculate how much of the player is below the camera
        // If playerTop is at cameraBottom, 0% is visible
        // If playerBottom is at cameraBottom, 100% is visible
        float amountBelowCamera = cameraBottom - playerBottom;
        float percentBelowCamera = Mathf.Clamp01(amountBelowCamera / playerHeight);
        
        // Trigger death when enough of the player is below camera
        if (percentBelowCamera >= deathThreshold)
        {
            TriggerDeath();
        }
    }

    public void TriggerDeath()
    {
        if (isDying) return;
        isDying = true;
        
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        originalPosition = transform.position;
        
        // Disable player controls immediately
        DisablePlayerControls();
        
        // Play death hit sound
        if (SoundManager.Instance != null && deathHitSound != null)
        {
            SoundManager.Instance.PlaySound(deathHitSound, transform, deathHitVolume);
        }
        
        // Camera shake for impact
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.DeathShake();
        }
        
        // === PHASE 1: HITSTOP - FREEZE EVERYTHING ===
        Time.timeScale = 0f;
        
        // Wait using realtime since timeScale is 0
        yield return new WaitForSecondsRealtime(hitstopDuration);
        
        // === PHASE 2: SHAKE + RED FLASH ===
        // Keep everything frozen, but animate player using unscaled time
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / shakeDuration;
            
            // Shake the player (using unscaledTime so it works while frozen)
            float shakeX = Mathf.Sin(Time.unscaledTime * shakeSpeed) * shakeIntensity * (1f - progress * 0.3f);
            float shakeY = Mathf.Cos(Time.unscaledTime * shakeSpeed * 1.3f) * shakeIntensity * (1f - progress * 0.3f);
            transform.position = originalPosition + new Vector3(shakeX, shakeY, 0f);
            
            // Fade to red
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, deathColor, progress);
            }
            
            yield return null;
        }
        
        // Reset position before explosion
        transform.position = originalPosition;
        
        // === PHASE 3: EXPLOSION ===
        // Hide the player
        SetPlayerVisible(false);
        
        // Spawn explosion effect
        SpawnExplosion();
        
        // Wait before showing death screen (still using realtime since game is frozen)
        yield return new WaitForSecondsRealtime(restartDelay);
        
        // Show death screen instead of auto-restarting
        ShowDeathScreen();
    }

    void DisablePlayerControls()
    {
        // Disable movement
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;
        
        // Disable aiming and hide indicator
        PlayerAiming aiming = GetComponent<PlayerAiming>();
        if (aiming != null)
        {
            aiming.HideAimIndicator();
            aiming.enabled = false;
        }
        
        // Disable bow
        BowController bow = GetComponent<BowController>();
        if (bow != null)
            bow.enabled = false;
        
        // Stop rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
    }

    void SetPlayerVisible(bool visible)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.enabled = visible;
        }
    }

    void SpawnExplosion()
    {
        // Play explosion sound
        if (SoundManager.Instance != null && explosionSound != null)
        {
            SoundManager.Instance.PlaySound(explosionSound, transform, explosionVolume);
        }
        
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = Vector3.one * explosionScale;
        }
        else
        {
            // Create a simple particle explosion if no prefab assigned
            CreateDefaultExplosion();
        }
    }

    void CreateDefaultExplosion()
    {
        // Create simple sprite-based explosion (more reliable than particle systems in URP)
        int particleCount = 20;
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = new GameObject($"ExplosionParticle_{i}");
            particle.transform.position = transform.position;
            
            SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSquareSprite();
            sr.color = deathColor;
            sr.sortingOrder = 100;
            
            // Random direction and speed
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(5f, 12f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float size = Random.Range(0.2f, 0.5f);
            
            StartCoroutine(AnimateExplosionParticle(particle, sr, direction, speed, size));
        }
    }
    
    Sprite CreateWhiteSquareSprite()
    {
        // Create a simple 4x4 white texture
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
    
    IEnumerator AnimateExplosionParticle(GameObject particle, SpriteRenderer sr, Vector2 direction, float speed, float startSize)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = particle.transform.position;
        Color startColor = deathColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time since game is frozen
            float t = elapsed / duration;
            
            // Move outward
            particle.transform.position = startPos + (Vector3)(direction * speed * t);
            
            // Scale: grow quickly then shrink
            float scale = startSize * (1f + t * 2f) * (1f - t);
            particle.transform.localScale = Vector3.one * scale;
            
            // Color: fade from white -> yellow -> red -> transparent
            Color currentColor;
            if (t < 0.2f)
                currentColor = Color.Lerp(Color.white, Color.yellow, t / 0.2f);
            else if (t < 0.5f)
                currentColor = Color.Lerp(Color.yellow, deathColor, (t - 0.2f) / 0.3f);
            else
                currentColor = deathColor;
            
            currentColor.a = 1f - t; // Fade out
            sr.color = currentColor;
            
            yield return null;
        }
        
        Destroy(particle);
    }
    
    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (obj != null)
            Destroy(obj);
    }

    void ShowDeathScreen()
    {
        // Get final score
        Score scoreManager = FindFirstObjectByType<Score>();
        int finalScore = scoreManager != null ? scoreManager.GetScore() : 0;
        
        // Show death screen via GameEndUI
        if (GameEndUI.Instance != null)
        {
            GameEndUI.Instance.ShowDeathScreen(finalScore);
        }
        else
        {
            // Fallback: just restart if no GameEndUI exists
            Debug.LogWarning("GameEndUI not found! Add a GameEndUI component to the scene.");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}
