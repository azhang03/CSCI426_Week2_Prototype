using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallingRockSpawner : MonoBehaviour
{
    [Header("Rock Sprites")]
    [Tooltip("The warning icon sprite for rocks (pointing up)")]
    [SerializeField] private Sprite rockWarningSprite;
    
    [Tooltip("The rock sprite")]
    [SerializeField] private Sprite rockSprite;
    
    [Header("Wolf Sprites")]
    [Tooltip("The warning icon sprite for wolves (pointing right)")]
    [SerializeField] private Sprite wolfWarningSprite;
    
    [Tooltip("The wolf sprite (facing right by default)")]
    [SerializeField] private Sprite wolfSprite;
    
    [Header("Rock Spawn Settings")]
    [Tooltip("Minimum X position for rock spawning")]
    [SerializeField] private float minRockSpawnX = -13f;
    
    [Tooltip("Maximum X position for rock spawning")]
    [SerializeField] private float maxRockSpawnX = 17f;
    
    [Tooltip("How far above the camera to spawn the rock")]
    [SerializeField] private float rockSpawnHeightOffset = 2f;
    
    [Header("Wolf Spawn Settings")]
    [Tooltip("Time in seconds before wolves start spawning")]
    [SerializeField] private float wolfSpawnStartTime = 60f;
    
    [Tooltip("X position for wolf warning when coming from left")]
    [SerializeField] private float wolfWarningLeftX = -18f;
    
    [Tooltip("X position for wolf warning when coming from right")]
    [SerializeField] private float wolfWarningRightX = 22f;
    
    [Tooltip("Minimum Y position for wolf spawning")]
    [SerializeField] private float minWolfSpawnY = -5f;
    
    [Tooltip("Maximum Y position for wolf spawning")]
    [SerializeField] private float maxWolfSpawnY = 7f;
    
    [Tooltip("How far off-screen to spawn the wolf")]
    [SerializeField] private float wolfSpawnOffsetX = 5f;
    
    [Header("Warning Settings")]
    [Tooltip("Base time between blinks (decreases with difficulty)")]
    [SerializeField] private float baseBlinkInterval = 0.4f;
    
    [Tooltip("Number of blinks before hazards spawn (stays constant)")]
    [SerializeField] private int blinksBeforeDrop = 3;
    
    [Tooltip("Size of the warning icons")]
    [SerializeField] private float warningScale = 1f;
    
    [Header("Rock Settings")]
    [Tooltip("Scale of the falling rock")]
    [SerializeField] private float rockScale = 1f;
    
    [Tooltip("Base fall speed of the rock (increases with difficulty)")]
    [SerializeField] private float baseRockFallSpeed = 10f;
    
    [Header("Wolf Settings")]
    [Tooltip("Scale of the wolf")]
    [SerializeField] private float wolfScale = 1f;
    
    [Tooltip("Base movement speed of the wolf (increases with difficulty)")]
    [SerializeField] private float baseWolfMoveSpeed = 12f;
    
    [Tooltip("Number of wolves in a line")]
    [SerializeField] private int wolvesPerLine = 3;
    
    [Tooltip("Spacing between wolves in a line")]
    [SerializeField] private float wolfSpacing = 2f;
    
    [Header("Timing")]
    [Tooltip("Base delay between spawn cycles (decreases with difficulty)")]
    [SerializeField] private float baseDelayBetweenSpawns = 2f;
    
    [Header("Difficulty Scaling")]
    [Tooltip("How often difficulty increases (in seconds)")]
    [SerializeField] private float difficultyIncreaseInterval = 15f;
    
    [Tooltip("Multiplier applied to rock fall speed each interval (e.g., 1.15 = 15% faster)")]
    [SerializeField] private float fallSpeedMultiplierPerInterval = 1.15f;
    
    [Tooltip("Multiplier applied to wolf move speed each interval")]
    [SerializeField] private float wolfSpeedMultiplierPerInterval = 1.15f;
    
    [Tooltip("Multiplier applied to blink speed each interval (e.g., 0.85 = 15% faster blinks)")]
    [SerializeField] private float blinkSpeedMultiplierPerInterval = 0.85f;
    
    [Tooltip("Multiplier applied to delay between spawns each interval (e.g., 0.9 = 10% shorter delay)")]
    [SerializeField] private float delayMultiplierPerInterval = 0.9f;
    
    [Tooltip("Minimum blink interval (won't go faster than this)")]
    [SerializeField] private float minBlinkInterval = 0.1f;
    
    [Tooltip("Maximum rock fall speed (won't go faster than this)")]
    [SerializeField] private float maxRockFallSpeed = 30f;
    
    [Tooltip("Maximum wolf move speed (won't go faster than this)")]
    [SerializeField] private float maxWolfMoveSpeed = 25f;
    
    [Tooltip("Minimum delay between spawns (won't go shorter than this)")]
    [SerializeField] private float minDelayBetweenSpawns = 0.5f;
    
    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int warningSortingOrder = 10;
    [SerializeField] private int rockSortingOrder = 5;
    [SerializeField] private int wolfSortingOrder = 100;

    private Camera mainCamera;
    private List<GameObject> currentWarnings = new List<GameObject>();
    
    // Current scaled values
    private float currentBlinkInterval;
    private float currentRockFallSpeed;
    private float currentWolfMoveSpeed;
    private float currentDelayBetweenSpawns;
    private int currentDifficultyLevel = 0;
    private float elapsedTime = 0f;
    private bool wolvesEnabled = false;

    void Start()
    {
        mainCamera = Camera.main;
        
        // Initialize current values from base values
        currentBlinkInterval = baseBlinkInterval;
        currentRockFallSpeed = baseRockFallSpeed;
        currentWolfMoveSpeed = baseWolfMoveSpeed;
        currentDelayBetweenSpawns = baseDelayBetweenSpawns;
        
        StartCoroutine(SpawnCycle());
    }
    
    void Update()
    {
        // Only track elapsed time after game has started (first target hit)
        if (GameTimer.Instance != null && GameTimer.Instance.HasGameStarted())
        {
            elapsedTime += Time.deltaTime;
        }
        
        // Check if wolves should be enabled
        if (!wolvesEnabled && elapsedTime >= wolfSpawnStartTime)
        {
            wolvesEnabled = true;
            Debug.Log("Wolves are now spawning!");
        }
        
        // Check if it's time to increase difficulty
        int newDifficultyLevel = Mathf.FloorToInt(elapsedTime / difficultyIncreaseInterval);
        
        if (newDifficultyLevel > currentDifficultyLevel)
        {
            currentDifficultyLevel = newDifficultyLevel;
            IncreaseDifficulty();
        }
    }
    
    void IncreaseDifficulty()
    {
        // Increase rock fall speed (capped at max)
        currentRockFallSpeed = Mathf.Min(
            currentRockFallSpeed * fallSpeedMultiplierPerInterval,
            maxRockFallSpeed
        );
        
        // Increase wolf move speed (capped at max)
        currentWolfMoveSpeed = Mathf.Min(
            currentWolfMoveSpeed * wolfSpeedMultiplierPerInterval,
            maxWolfMoveSpeed
        );
        
        // Decrease blink interval for faster warnings (capped at min)
        currentBlinkInterval = Mathf.Max(
            currentBlinkInterval * blinkSpeedMultiplierPerInterval,
            minBlinkInterval
        );
        
        // Decrease delay between spawns (capped at min)
        currentDelayBetweenSpawns = Mathf.Max(
            currentDelayBetweenSpawns * delayMultiplierPerInterval,
            minDelayBetweenSpawns
        );
        
        Debug.Log($"Difficulty increased to level {currentDifficultyLevel}! " +
                  $"Rock speed: {currentRockFallSpeed:F1}, " +
                  $"Wolf speed: {currentWolfMoveSpeed:F1}, " +
                  $"Blink interval: {currentBlinkInterval:F2}s, " +
                  $"Spawn delay: {currentDelayBetweenSpawns:F2}s");
    }

    IEnumerator SpawnCycle()
    {
        while (true)
        {
            // === DETERMINE WHAT TO SPAWN THIS CYCLE ===
            
            // Rock always spawns
            float rockSpawnX = Random.Range(minRockSpawnX, maxRockSpawnX);
            float rockWarningY = mainCamera.transform.position.y;
            
            // Wolf spawns after wolfSpawnStartTime
            bool spawnWolf = wolvesEnabled;
            float wolfSpawnY = 0f;
            bool wolfFromRight = false;
            
            if (spawnWolf)
            {
                wolfSpawnY = Random.Range(minWolfSpawnY, maxWolfSpawnY);
                wolfFromRight = Random.Range(0, 2) == 1;
            }
            
            // === CREATE WARNING INDICATORS (synchronized) ===
            
            // Rock warning (always)
            CreateRockWarning(rockSpawnX, rockWarningY);
            
            // Wolf warning (if enabled)
            if (spawnWolf)
            {
                CreateWolfWarning(wolfSpawnY, wolfFromRight);
            }
            
            // Cache the current blink interval at the start of this cycle
            float cycleBlinkInterval = currentBlinkInterval;
            
            // === BLINK SEQUENCE (synchronized for all warnings) ===
            for (int i = 0; i < blinksBeforeDrop; i++)
            {
                // Show all warnings
                SetAllWarningsVisible(true);
                yield return new WaitForSeconds(cycleBlinkInterval);
                
                // Hide all warnings (except on last blink)
                if (i < blinksBeforeDrop - 1)
                {
                    SetAllWarningsVisible(false);
                    yield return new WaitForSeconds(cycleBlinkInterval * 0.5f);
                }
            }
            
            // === SPAWN HAZARDS ===
            
            // Spawn rock
            SpawnRock(rockSpawnX);
            
            // Spawn wolf if enabled
            if (spawnWolf)
            {
                SpawnWolf(wolfSpawnY, wolfFromRight);
            }
            
            // Clean up all warnings
            DestroyAllWarnings();
            
            // Wait before next cycle
            yield return new WaitForSeconds(currentDelayBetweenSpawns);
        }
    }

    void CreateRockWarning(float x, float y)
    {
        GameObject warning = new GameObject("RockWarning");
        warning.transform.position = new Vector3(x, y, 0f);
        
        SpriteRenderer sr = warning.AddComponent<SpriteRenderer>();
        if (rockWarningSprite != null)
        {
            sr.sprite = rockWarningSprite;
        }
        else
        {
            sr.sprite = CreateFallbackSprite();
            sr.color = Color.yellow;
        }
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = warningSortingOrder;
        warning.transform.localScale = Vector3.one * warningScale;
        
        currentWarnings.Add(warning);
    }
    
    void CreateWolfWarning(float y, bool fromRight)
    {
        GameObject warning = new GameObject("WolfWarning");
        
        // Position based on which side wolf is coming from
        float warningX = fromRight ? wolfWarningRightX : wolfWarningLeftX;
        warning.transform.position = new Vector3(warningX, y, 0f);
        
        SpriteRenderer sr = warning.AddComponent<SpriteRenderer>();
        if (wolfWarningSprite != null)
        {
            sr.sprite = wolfWarningSprite;
        }
        else
        {
            sr.sprite = CreateFallbackSprite();
            sr.color = Color.red;
        }
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = warningSortingOrder;
        
        // Flip horizontally if wolf is coming from the right
        // (warning should point toward where wolf is coming from)
        sr.flipX = fromRight;
        
        warning.transform.localScale = Vector3.one * warningScale;
        
        currentWarnings.Add(warning);
    }

    void SetAllWarningsVisible(bool visible)
    {
        foreach (var warning in currentWarnings)
        {
            if (warning != null)
            {
                warning.SetActive(visible);
            }
        }
    }

    void DestroyAllWarnings()
    {
        foreach (var warning in currentWarnings)
        {
            if (warning != null)
            {
                Destroy(warning);
            }
        }
        currentWarnings.Clear();
    }

    void SpawnRock(float x)
    {
        // Calculate spawn position above camera
        float spawnY = mainCamera.transform.position.y + mainCamera.orthographicSize + rockSpawnHeightOffset;
        
        // Create rock object
        GameObject rock = new GameObject("FallingRock");
        rock.transform.position = new Vector3(x, spawnY, 0f);
        rock.transform.localScale = Vector3.one * rockScale;
        
        // Add sprite renderer
        SpriteRenderer rockSR = rock.AddComponent<SpriteRenderer>();
        if (rockSprite != null)
        {
            rockSR.sprite = rockSprite;
        }
        else
        {
            rockSR.sprite = CreateFallbackSprite();
            rockSR.color = Color.grey;
        }
        rockSR.sortingLayerName = sortingLayerName;
        rockSR.sortingOrder = rockSortingOrder;
        
        // Add physics
        Rigidbody2D rb = rock.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        // Add collider
        PolygonCollider2D col = rock.AddComponent<PolygonCollider2D>();
        col.isTrigger = false;
        
        // Add FallingRock script and set its speed
        FallingRock fallingRock = rock.AddComponent<FallingRock>();
        fallingRock.SetFallSpeed(currentRockFallSpeed);
        
        rock.tag = "Damage";
    }
    
    void SpawnWolf(float y, bool fromRight)
    {
        // Calculate spawn position off-screen
        float cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float cameraX = mainCamera.transform.position.x;
        
        float baseSpawnX;
        int moveDirection;
        
        if (fromRight)
        {
            // Spawn on right, move left
            baseSpawnX = cameraX + cameraHalfWidth + wolfSpawnOffsetX;
            moveDirection = -1;
        }
        else
        {
            // Spawn on left, move right
            baseSpawnX = cameraX - cameraHalfWidth - wolfSpawnOffsetX;
            moveDirection = 1;
        }
        
        // Spawn a line of wolves
        for (int i = 0; i < wolvesPerLine; i++)
        {
            // Offset each wolf behind the previous one (in the direction they came from)
            // So if moving right, wolves are staggered to the left of the base position
            float offsetX = i * wolfSpacing * -moveDirection;
            float spawnX = baseSpawnX + offsetX;
            
            CreateSingleWolf(spawnX, y, moveDirection);
        }
    }
    
    void CreateSingleWolf(float x, float y, int moveDirection)
    {
        // Create wolf object
        GameObject wolf = new GameObject("Wolf");
        wolf.transform.position = new Vector3(x, y, 0f);
        wolf.transform.localScale = Vector3.one * wolfScale;
        
        // Add sprite renderer
        SpriteRenderer wolfSR = wolf.AddComponent<SpriteRenderer>();
        if (wolfSprite != null)
        {
            wolfSR.sprite = wolfSprite;
        }
        else
        {
            wolfSR.sprite = CreateFallbackSprite();
            wolfSR.color = Color.red;
        }
        wolfSR.sortingLayerName = sortingLayerName;
        wolfSR.sortingOrder = wolfSortingOrder;
        
        // Flip sprite if moving left (wolf faces right by default)
        wolfSR.flipX = (moveDirection < 0);
        
        // Add trigger collider that conforms to sprite shape (no physics collision, just detects player)
        PolygonCollider2D col = wolf.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;
        
        // Add WolfLine script
        WolfLine wolfLine = wolf.AddComponent<WolfLine>();
        wolfLine.SetMoveSpeed(currentWolfMoveSpeed);
        wolfLine.SetDirection(moveDirection);
        
        wolf.tag = "Damage";
    }

    Sprite CreateFallbackSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
    
    /// <summary>
    /// Returns the elapsed time since the game started
    /// </summary>
    public float GetElapsedTime()
    {
        return elapsedTime;
    }
    
    /// <summary>
    /// Returns true if wolves are currently spawning
    /// </summary>
    public bool AreWolvesEnabled()
    {
        return wolvesEnabled;
    }
}
