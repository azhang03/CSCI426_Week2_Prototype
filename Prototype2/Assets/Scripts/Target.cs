using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject textObj;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the player
        if (other.CompareTag("Arrow"))
        {
            Debug.Log("HIT");
            float X;
            if(Random.Range(0,2) == 1)
            {
                X = 22.0f;
            }
            else
            {
                X = -22.0f;
            }
            Instantiate(this, new Vector3(X, Random.Range(-7f, 9f), 0), Quaternion.identity);
            textObj.GetComponent<Score>().ScorePlus();
            textObj.GetComponent<Score>().UpdateScoreText();
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
