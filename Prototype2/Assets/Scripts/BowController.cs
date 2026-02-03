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
    
    [Tooltip("Vertical offset above player's head (in world units)")]
    [SerializeField] private float chargeBarHeightOffset = 1.5f;
    
    [Tooltip("Size of the charge bar (width x height in pixels)")]
    [SerializeField] private Vector2 chargeBarSize = new Vector2(80f, 12f);

    private PlayerAiming playerAiming;
    private Camera mainCamera;
    private RectTransform chargeUIRect;
    private Canvas parentCanvas;
    private float currentChargeTime;
    private bool isCharging;

    private void Start()
    {
        playerAiming = GetComponent<PlayerAiming>();
        mainCamera = Camera.main;
        
        if (playerAiming == null)
        {
            Debug.LogError("BowController requires PlayerAiming component!");
        }

        // Get RectTransform for positioning
        if (chargeUIContainer != null)
        {
            chargeUIRect = chargeUIContainer.GetComponent<RectTransform>();
            parentCanvas = chargeUIContainer.GetComponentInParent<Canvas>();
            
            // Set the size of the container
            if (chargeUIRect != null)
            {
                chargeUIRect.sizeDelta = chargeBarSize;
            }
            
            chargeUIContainer.SetActive(false);
        }
        
        // Set the charge indicator to fill horizontally (left to right)
        if (chargeIndicator != null)
        {
            chargeIndicator.type = Image.Type.Filled;
            chargeIndicator.fillMethod = Image.FillMethod.Horizontal;
            chargeIndicator.fillOrigin = (int)Image.OriginHorizontal.Left;
            
            // Set the size of the fill image and make it stretch to fill the container
            RectTransform indicatorRect = chargeIndicator.GetComponent<RectTransform>();
            if (indicatorRect != null)
            {
                // Reset anchors to stretch mode (fill entire parent)
                indicatorRect.anchorMin = Vector2.zero;
                indicatorRect.anchorMax = Vector2.one;
                indicatorRect.offsetMin = Vector2.zero;
                indicatorRect.offsetMax = Vector2.zero;
            }
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
        
        // Position the charge bar above the player's head
        if (chargeUIRect != null && mainCamera != null)
        {
            // Convert player's world position (with height offset) to screen position
            Vector3 worldPos = transform.position + Vector3.up * chargeBarHeightOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            
            // Handle different canvas render modes
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                chargeUIRect.position = screenPos;
            }
            else if (parentCanvas != null)
            {
                // For Screen Space - Camera or World Space canvases
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    screenPos,
                    parentCanvas.worldCamera,
                    out Vector2 localPoint
                );
                chargeUIRect.localPosition = localPoint;
            }
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
