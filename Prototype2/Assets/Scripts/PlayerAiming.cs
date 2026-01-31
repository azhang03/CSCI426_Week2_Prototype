using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    [Header("Aim Indicator")]
    [Tooltip("The arrow sprite that shows aim direction")]
    [SerializeField] private Transform aimArrow;
    
    [Tooltip("Distance from player center to place the arrow")]
    [SerializeField] private float arrowOffset = 1f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        
        if (aimArrow == null)
        {
            Debug.LogWarning("PlayerAiming: No aim arrow assigned!");
        }
    }

    private void Update()
    {
        if (aimArrow == null || mainCamera == null) return;

        // Get mouse position in world space
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));
        mouseWorldPos.z = 0f;

        // Calculate direction from player to mouse
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // Position the arrow at a fixed offset from the player
        aimArrow.position = (Vector2)transform.position + direction * arrowOffset;

        // Rotate the arrow to face the mouse direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimArrow.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Returns the current aim direction (normalized)
    /// </summary>
    public Vector2 GetAimDirection()
    {
        if (mainCamera == null) return Vector2.right;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));
        mouseWorldPos.z = 0f;

        return ((Vector2)(mouseWorldPos - transform.position)).normalized;
    }
}
