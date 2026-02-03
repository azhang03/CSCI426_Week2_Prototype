using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Score : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private int fontSize = 36;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Vector2 position = new Vector2(20, -20);
    
    [Header("Point Value Settings")]
    [Tooltip("Base points for hitting a target")]
    [SerializeField] private int basePointValue = 100;
    
    [Tooltip("Points added every 15 seconds during normal phase (first minute)")]
    [SerializeField] private int normalPhasePointIncrease = 50;
    
    [Tooltip("Points added every 15 seconds during danger phase (after 1 minute)")]
    [SerializeField] private int dangerPhasePointIncrease = 75;
    
    [Tooltip("How often point value increases (in seconds)")]
    [SerializeField] private float pointIncreaseInterval = 15f;
    
    [Tooltip("Time when danger phase begins (in seconds elapsed)")]
    [SerializeField] private float dangerPhaseStartTime = 60f;
    
    [Header("Floating Text Settings")]
    [SerializeField] private int floatingTextFontSize = 28;
    [SerializeField] private Color floatingTextColor = new Color(1f, 1f, 0.3f, 1f); // Yellow
    [SerializeField] private Color dangerPhaseFloatingTextColor = new Color(1f, 0.85f, 0f, 1f); // Gold
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatDuration = 1f;
    
    private Text scoreText;
    private Canvas uiCanvas;
    private Camera mainCamera;
    private int score;
    private float elapsedTime = 0f;
    private int currentPointValue;
    private bool inDangerPhase = false;

    void Start()
    {
        score = 0;
        currentPointValue = basePointValue;
        mainCamera = Camera.main;
        CreateScoreUI();
        UpdateScoreText();
    }
    
    void Update()
    {
        // Only track elapsed time after game has started (first target hit)
        if (GameTimer.Instance != null && GameTimer.Instance.HasGameStarted())
        {
            elapsedTime += Time.deltaTime;
        }
        
        // Check for danger phase
        if (!inDangerPhase && elapsedTime >= dangerPhaseStartTime)
        {
            inDangerPhase = true;
            Debug.Log("Danger phase started! Points now increase by " + dangerPhasePointIncrease);
        }
        
        // Calculate current point value based on elapsed time
        UpdatePointValue();
    }
    
    void UpdatePointValue()
    {
        // Calculate how many intervals have passed
        int intervals = Mathf.FloorToInt(elapsedTime / pointIncreaseInterval);
        
        // Calculate how many intervals were in normal phase vs danger phase
        int dangerPhaseStartInterval = Mathf.FloorToInt(dangerPhaseStartTime / pointIncreaseInterval);
        
        int normalIntervals = Mathf.Min(intervals, dangerPhaseStartInterval);
        int dangerIntervals = Mathf.Max(0, intervals - dangerPhaseStartInterval);
        
        // Calculate total point value
        currentPointValue = basePointValue 
            + (normalIntervals * normalPhasePointIncrease)
            + (dangerIntervals * dangerPhasePointIncrease);
    }

    void CreateScoreUI()
    {
        // Find a Screen Space Overlay canvas, or create one
        uiCanvas = null;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in allCanvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                uiCanvas = c;
                break;
            }
        }
        
        if (uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("UICanvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create score text object
        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(uiCanvas.transform, false);

        // Set up RectTransform - anchor to top-left
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 50);

        // Add and configure Text component
        scoreText = textObj.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = fontSize;
        scoreText.color = textColor;
        scoreText.alignment = TextAnchor.UpperLeft;
        scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
        scoreText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    /// <summary>
    /// Add points and show floating indicator at world position
    /// </summary>
    public void AddPoints(int points, Vector3 worldPosition)
    {
        // Start the timer on first score (first target hit)
        if (score == 0 && GameTimer.Instance != null)
        {
            GameTimer.Instance.StartCountdown();
        }
        
        score += points;
        UpdateScoreText();
        
        // Create floating text indicator
        CreateFloatingText(points, worldPosition);
    }
    
    /// <summary>
    /// Get current point value for hitting a target
    /// </summary>
    public int GetCurrentPointValue()
    {
        return currentPointValue;
    }
    
    /// <summary>
    /// Returns true if in danger phase (after 1 minute)
    /// </summary>
    public bool IsInDangerPhase()
    {
        return inDangerPhase;
    }
    
    void CreateFloatingText(int points, Vector3 worldPosition)
    {
        if (uiCanvas == null || mainCamera == null) return;
        
        // Create floating text object
        GameObject floatObj = new GameObject("FloatingPoints");
        floatObj.transform.SetParent(uiCanvas.transform, false);
        
        // Set up RectTransform
        RectTransform rect = floatObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        
        // Convert world position to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        rect.position = screenPos;
        
        // Add Text component
        Text floatText = floatObj.AddComponent<Text>();
        floatText.text = "+" + points.ToString();
        floatText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        floatText.fontSize = floatingTextFontSize;
        floatText.color = inDangerPhase ? dangerPhaseFloatingTextColor : floatingTextColor;
        floatText.alignment = TextAnchor.MiddleCenter;
        floatText.horizontalOverflow = HorizontalWrapMode.Overflow;
        floatText.verticalOverflow = VerticalWrapMode.Overflow;
        
        // Add outline for visibility
        Outline outline = floatObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
        
        // Start floating animation
        StartCoroutine(AnimateFloatingText(floatObj, rect, floatText));
    }
    
    IEnumerator AnimateFloatingText(GameObject obj, RectTransform rect, Text text)
    {
        float elapsed = 0f;
        Vector3 startPos = rect.position;
        Color startColor = text.color;
        
        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / floatDuration;
            
            // Float upward
            rect.position = startPos + Vector3.up * (floatSpeed * 50f * progress);
            
            // Fade out (faster in second half)
            float alpha = 1f - Mathf.Pow(progress, 2);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            // Scale up slightly then down
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.3f;
            rect.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        Destroy(obj);
    }

    /// <summary>
    /// Legacy method - adds 1 point (kept for compatibility)
    /// </summary>
    public void ScorePlus()
    {
        score++;
    }
    
    public int GetScore()
    {
        return score;
    }
    
    /// <summary>
    /// Developer tool: Add to elapsed time for testing
    /// </summary>
    public void AddElapsedTime(float seconds)
    {
        elapsedTime += seconds;
    }
}
