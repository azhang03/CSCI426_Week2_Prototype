using UnityEngine;
using System.Collections;

public class DamageablePlatform : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Number of blinks before platform restores")]
    [SerializeField] private int blinkCount = 3;
    
    [Tooltip("Time between blinks")]
    [SerializeField] private float blinkInterval = 0.3f;
    
    [Tooltip("Alpha value when translucent")]
    [SerializeField] private float translucentAlpha = 0.3f;
    
    [Tooltip("Color tint when damaged")]
    [SerializeField] private Color damagedColor = new Color(1f, 0.3f, 0.3f, 1f); // Red tint
    
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Color originalColor;
    private bool isDamaged = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Call this to trigger the damaged/blinking state
    /// </summary>
    public void TriggerDamage()
    {
        if (isDamaged) return; // Already damaged, don't stack
        
        StartCoroutine(DamageSequence());
    }

    IEnumerator DamageSequence()
    {
        isDamaged = true;
        
        // Disable collider so player falls through and arrows pass through
        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }
        
        // Blink sequence
        for (int i = 0; i < blinkCount; i++)
        {
            // Translucent
            SetTranslucent(true);
            yield return new WaitForSeconds(blinkInterval);
            
            // Solid (but still damaged/non-collidable)
            SetTranslucent(false);
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // Restore to normal
        RestorePlatform();
    }

    void SetTranslucent(bool translucent)
    {
        if (spriteRenderer == null) return;
        
        // Apply red tint with varying alpha for blink effect
        Color c = damagedColor;
        c.a = translucent ? translucentAlpha : damagedColor.a;
        spriteRenderer.color = c;
    }

    void RestorePlatform()
    {
        isDamaged = false;
        
        // Re-enable collider
        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }
        
        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Returns true if the platform is currently in damaged state
    /// </summary>
    public bool IsDamaged()
    {
        return isDamaged;
    }
}
