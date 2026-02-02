using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpSpeed = 15f;

    private Rigidbody2D rb;

    [SerializeField] private int health = 3;
    private Vector2 moveInput;

    private bool jumpHold = false;
    private bool jumped = false;

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

        if (keyboard.aKey.isPressed)
            moveInput.x -= 1;
        if (keyboard.dKey.isPressed)
            moveInput.x += 1;
        if (keyboard.spaceKey.isPressed)
        {
            if(jumpHold == false)
            {
                Debug.Log("Boyo");
                jumped = true;
            }
            jumpHold = true;
        }
        else
        {
            jumpHold = false;
        }

        // Y is processed as force

        // Normalize to prevent faster diagonal movement
        // moveInput = moveInput.normalized;
    }

    private void FixedUpdate()
    {

        if (rb != null)
        {
            rb.linearVelocityX = moveInput.x * moveSpeed;
            if(jumped)
            {
                rb.linearVelocityY = jumpSpeed;
                jumped = false;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Damage")) return;
        Debug.Log("The yahoo");
        if(other.GetComponent<DamageZone>().GetActive())
        {
            health--;
            other.GetComponent<DamageZone>().HealthReducedCoolDown();
        }
    }
}
