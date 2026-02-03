using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float rightSpawnX = 22.0f;
    [SerializeField] private float leftSpawnX = -22.0f;
    [SerializeField] private float minSpawnY = -7f;
    [SerializeField] private float maxSpawnY = 9f;
    
    private Score scoreManager;
    
    void Start()
    {
        // Find the Score component in the scene
        scoreManager = FindFirstObjectByType<Score>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to arrows
        if (other.CompareTag("Arrow"))
        {
            Debug.Log("HIT");
            
            // Choose random side for new target
            float X = Random.Range(0, 2) == 1 ? rightSpawnX : leftSpawnX;
            float Y = Random.Range(minSpawnY, maxSpawnY);
            
            // Spawn new target
            Instantiate(this, new Vector3(X, Y, 0), Quaternion.identity);
            
            // Update score
            if (scoreManager != null)
            {
                scoreManager.ScorePlus();
                scoreManager.UpdateScoreText();
            }
            
            // Destroy arrow and this target
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
