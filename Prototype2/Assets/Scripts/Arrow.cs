using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Lifetime")]
    [Tooltip("How long the arrow lives before being destroyed")]
    [SerializeField] private float lifetime = 5f;

    [Header("Behavior")]
    [Tooltip("Should the arrow be destroyed on hitting something?")]
    [SerializeField] private bool destroyOnHit = true;
    
    [Tooltip("Should the arrow rotate to face its travel direction?")]
    [SerializeField] private bool rotateWithVelocity = true;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Destroy after lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Rotate arrow to face the direction it's moving
        if (rotateWithVelocity && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the player
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Target")) return;

        if (other.CompareTag("Damage")) return;

        // Handle hit (we'll expand this later for targets)
        Debug.Log($"Arrow hit: {other.gameObject.name}");

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Don't hit the player
        if (collision.gameObject.CompareTag("Player")) return;

        if (collision.gameObject.CompareTag("Target")) return;

        // Handle hit
        Debug.Log($"Arrow hit: {collision.gameObject.name}");

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
