using UnityEngine;
using System.Collections;

public class FallingRockSpawner : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("The warning icon sprite (includes the up arrow)")]
    [SerializeField] private Sprite warningSprite;
    
    [Tooltip("The rock sprite")]
    [SerializeField] private Sprite rockSprite;
    
    [Header("Spawn Settings")]
    [Tooltip("Minimum X position for spawning")]
    [SerializeField] private float minSpawnX = -13f;
    
    [Tooltip("Maximum X position for spawning")]
    [SerializeField] private float maxSpawnX = 17f;
    
    [Tooltip("How far above the camera to spawn the rock")]
    [SerializeField] private float spawnHeightOffset = 2f;
    
    [Header("Warning Settings")]
    [Tooltip("Time between blinks")]
    [SerializeField] private float blinkInterval = 0.4f;
    
    [Tooltip("Number of blinks before rock falls")]
    [SerializeField] private int blinksBeforeDrop = 3;
    
    [Tooltip("Size of the warning icon")]
    [SerializeField] private float warningScale = 1f;
    
    [Header("Rock Settings")]
    [Tooltip("Scale of the falling rock")]
    [SerializeField] private float rockScale = 1f;
    
    [Tooltip("Fall speed of the rock")]
    [SerializeField] private float rockFallSpeed = 10f;
    
    [Header("Timing")]
    [Tooltip("Delay between rock cycles")]
    [SerializeField] private float delayBetweenRocks = 2f;
    
    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int warningSortingOrder = 10;
    [SerializeField] private int rockSortingOrder = 5;

    private Camera mainCamera;
    private GameObject currentWarning;

    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnCycle());
    }

    IEnumerator SpawnCycle()
    {
        while (true)
        {
            // Pick a random X position
            float spawnX = Random.Range(minSpawnX, maxSpawnX);
            
            // Get center Y of screen
            float centerY = mainCamera.transform.position.y;
            
            // Create warning indicator
            CreateWarningIndicator(spawnX, centerY);
            
            // Blink sequence
            for (int i = 0; i < blinksBeforeDrop; i++)
            {
                // Show
                SetWarningVisible(true);
                yield return new WaitForSeconds(blinkInterval);
                
                // Hide (except on last blink)
                if (i < blinksBeforeDrop - 1)
                {
                    SetWarningVisible(false);
                    yield return new WaitForSeconds(blinkInterval * 0.5f);
                }
            }
            
            // Spawn the rock from above
            SpawnRock(spawnX);
            
            // Clean up warning
            DestroyWarningIndicator();
            
            // Wait before next cycle
            yield return new WaitForSeconds(delayBetweenRocks);
        }
    }

    void CreateWarningIndicator(float x, float y)
    {
        // Create warning icon
        currentWarning = new GameObject("WarningIndicator");
        currentWarning.transform.position = new Vector3(x, y, 0f);
        
        SpriteRenderer warningSR = currentWarning.AddComponent<SpriteRenderer>();
        if (warningSprite != null)
        {
            warningSR.sprite = warningSprite;
        }
        else
        {
            // Fallback: create a simple colored square
            warningSR.sprite = CreateFallbackSprite();
            warningSR.color = Color.yellow;
        }
        warningSR.sortingLayerName = sortingLayerName;
        warningSR.sortingOrder = warningSortingOrder;
        currentWarning.transform.localScale = Vector3.one * warningScale;
    }

    void SetWarningVisible(bool visible)
    {
        if (currentWarning != null)
        {
            currentWarning.SetActive(visible);
        }
    }

    void DestroyWarningIndicator()
    {
        if (currentWarning != null)
        {
            Destroy(currentWarning);
            currentWarning = null;
        }
    }

    void SpawnRock(float x)
    {
        // Calculate spawn position above camera
        float spawnY = mainCamera.transform.position.y + mainCamera.orthographicSize + spawnHeightOffset;
        
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
            // Fallback: grey square
            rockSR.sprite = CreateFallbackSprite();
            rockSR.color = Color.grey;
        }
        rockSR.sortingLayerName = sortingLayerName;
        rockSR.sortingOrder = rockSortingOrder;
        
        // Add physics
        Rigidbody2D rb = rock.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // We control velocity manually
        rb.freezeRotation = true;
        
        // Add collider that conforms to sprite shape
        PolygonCollider2D col = rock.AddComponent<PolygonCollider2D>();
        col.isTrigger = false;
        
        // Add FallingRock script and set its speed
        FallingRock fallingRock = rock.AddComponent<FallingRock>();
        fallingRock.SetFallSpeed(rockFallSpeed);
        
        // Tag it for collision detection
        rock.tag = "Damage";
    }

    Sprite CreateFallbackSprite()
    {
        // Create a simple 4x4 white texture as fallback
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
