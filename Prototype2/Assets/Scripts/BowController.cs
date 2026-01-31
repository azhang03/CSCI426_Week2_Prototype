using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BowController : MonoBehaviour
{
    [Header("Charge Settings")]
    [Tooltip("Time in seconds to reach full charge")]
    [SerializeField] private float maxChargeTime = 2f;
    
    [Tooltip("Minimum power multiplier (when just tapping)")]
    [SerializeField] private float minPower = 0.2f;
    
    [Tooltip("Maximum power multiplier (at full charge)")]
    [SerializeField] private float maxPower = 1f;

    [Header("Arrow Settings")]
    [Tooltip("Arrow prefab to spawn when firing")]
    [SerializeField] private GameObject arrowPrefab;
    
    [Tooltip("Base speed of the arrow")]
    [SerializeField] private float baseArrowSpeed = 10f;
    
    [Tooltip("Spawn offset from player center")]
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("UI")]
    [Tooltip("The charge indicator image (should be set to Image Type: Filled)")]
    [SerializeField] private Image chargeIndicator;
    
    [Tooltip("Parent object of the charge UI (to show/hide)")]
    [SerializeField] private GameObject chargeUIContainer;

    private PlayerAiming playerAiming;
    private float currentChargeTime;
    private bool isCharging;

    private void Start()
    {
        playerAiming = GetComponent<PlayerAiming>();
        
        if (playerAiming == null)
        {
            Debug.LogError("BowController requires PlayerAiming component!");
        }

        // Hide charge UI initially
        if (chargeUIContainer != null)
        {
            chargeUIContainer.SetActive(false);
        }
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Start charging on left mouse button press
        if (mouse.leftButton.wasPressedThisFrame)
        {
            StartCharging();
        }

        // Continue charging while held
        if (isCharging && mouse.leftButton.isPressed)
        {
            UpdateCharge();
        }

        // Fire on release
        if (mouse.leftButton.wasReleasedThisFrame && isCharging)
        {
            Fire();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;

        // Show charge UI
        if (chargeUIContainer != null)
        {
            chargeUIContainer.SetActive(true);
        }

        UpdateChargeUI();
    }

    private void UpdateCharge()
    {
        currentChargeTime += Time.deltaTime;
        currentChargeTime = Mathf.Min(currentChargeTime, maxChargeTime);

        UpdateChargeUI();
    }

    private void UpdateChargeUI()
    {
        if (chargeIndicator != null)
        {
            float chargePercent = currentChargeTime / maxChargeTime;
            chargeIndicator.fillAmount = chargePercent;
        }
    }

    private void Fire()
    {
        isCharging = false;

        // Calculate power based on charge time
        float chargePercent = currentChargeTime / maxChargeTime;
        float power = Mathf.Lerp(minPower, maxPower, chargePercent);

        // Get aim direction
        Vector2 aimDirection = playerAiming != null ? playerAiming.GetAimDirection() : Vector2.right;

        // Spawn arrow
        if (arrowPrefab != null)
        {
            Vector3 spawnPos = transform.position + (Vector3)(aimDirection * spawnOffset);
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            
            GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));
            
            // Set arrow velocity
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = aimDirection * baseArrowSpeed * power;
            }
        }

        // Hide charge UI
        if (chargeUIContainer != null)
        {
            chargeUIContainer.SetActive(false);
        }

        // Reset charge
        currentChargeTime = 0f;

        Debug.Log($"Fired arrow with {chargePercent * 100:F0}% charge, power: {power:F2}");
    }

    /// <summary>
    /// Returns current charge as a value from 0 to 1
    /// </summary>
    public float GetChargePercent()
    {
        return currentChargeTime / maxChargeTime;
    }
}
