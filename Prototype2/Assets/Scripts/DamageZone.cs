using UnityEngine;

public class DamageZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isActive = false;

    private bool isInterval = false;
    private int theCountUp = 0;
    private float interval = 0.3f;

    private float coolDown = 2.5f;

    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.red;
        Color tmp = spriteRenderer.color;
        tmp.a = 0.5f;
        spriteRenderer.color = tmp;
    }

    // Update is called once per frame
    void Update()
    {
        if(interval > 0)
        {
            // Start counting
            interval -= Time.deltaTime;
        }
        else
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            interval = 0.3f;
            theCountUp++;
            if(theCountUp == 5)
            {
                isActive = true;
                Color tmp = spriteRenderer.color;
                tmp.a = 1f;
                spriteRenderer.color = tmp;
            }
            if(theCountUp == 6)
            {
                theCountUp = 0;
                isActive = false;
                Color tmp = spriteRenderer.color;
                tmp.a = 0.5f;
                spriteRenderer.color = tmp;
                transform.position = new Vector2(Random.Range(-13.34f, 17.34f), transform.position.y);
            }
        }
    }

    public void HealthReducedCoolDown()
    {
        isActive = false;
    }

    public bool GetActive()
    {
        return isActive;
    }
}
