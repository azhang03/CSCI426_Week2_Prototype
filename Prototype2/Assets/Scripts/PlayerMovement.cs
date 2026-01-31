using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody2D component!");
        }
    }

    private void Update()
    {
        // Get WASD input using new Input System
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = Vector2.zero;

        if (keyboard.wKey.isPressed)
            moveInput.y += 1;
        if (keyboard.sKey.isPressed)
            moveInput.y -= 1;
        if (keyboard.aKey.isPressed)
            moveInput.x -= 1;
        if (keyboard.dKey.isPressed)
            moveInput.x += 1;

        // Normalize to prevent faster diagonal movement
        moveInput = moveInput.normalized;
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }
}
