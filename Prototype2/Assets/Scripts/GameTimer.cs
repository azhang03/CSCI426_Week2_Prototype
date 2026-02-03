using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Starting time in seconds (120 = 2 minutes)")]
    [SerializeField] private float startTime = 120f;
    
    [Header("Display Settings")]
    [SerializeField] private int fontSize = 200;
    [SerializeField] private Color textColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float topOffset = 0.8f; // How far from center (0 = center, 1 = top edge)
    
    [Header("Sorting")]
    [Tooltip("Sorting order for the timer (lower = further back)")]
    [SerializeField] private int sortingOrder = -10;
    [SerializeField] private string sortingLayerName = "Default";

    private Text timerText;
    private Canvas timerCanvas;
    private Camera mainCamera;
    private float currentTime;
    private bool isRunning = true;

    void Start()
    {
        mainCamera = Camera.main;
        currentTime = startTime;
        CreateTimerUI();
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isRunning) return;
        
        currentTime -= Time.deltaTime;
        
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            OnTimerEnd();
        }
        
        UpdateTimerDisplay();
    }

    void CreateTimerUI()
    {
        // Create a World Space canvas so it renders behind sprites
        GameObject canvasObj = new GameObject("TimerCanvas");
        timerCanvas = canvasObj.AddComponent<Canvas>();
        timerCanvas.renderMode = RenderMode.WorldSpace;
        timerCanvas.sortingLayerName = sortingLayerName;
        timerCanvas.sortingOrder = sortingOrder;
        
        // Set up canvas size
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1000, 300);
        canvasObj.transform.localScale = Vector3.one * 0.01f;
        
        // Position will be set in LateUpdate

        // Create timer text object
        GameObject textObj = new GameObject("TimerText");
        textObj.transform.SetParent(canvasObj.transform, false);

        // Set up RectTransform - fill the canvas
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Add and configure Text component
        timerText = textObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = fontSize;
        timerText.color = textColor;
        timerText.alignment = TextAnchor.MiddleCenter;
        timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        timerText.verticalOverflow = VerticalWrapMode.Overflow;
    }
    
    void LateUpdate()
    {
        // Position the timer at the top center of the camera view
        if (timerCanvas != null && mainCamera != null)
        {
            float camHeight = mainCamera.orthographicSize;
            Vector3 camPos = mainCamera.transform.position;
            
            // Position at top of camera view
            timerCanvas.transform.position = new Vector3(
                camPos.x,
                camPos.y + camHeight * topOffset,
                camPos.z + 10f
            );
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }

    void OnTimerEnd()
    {
        // TODO: Handle game over / time's up logic
        Debug.Log("Time's up!");
    }

    /// <summary>
    /// Returns the current time remaining in seconds
    /// </summary>
    public float GetTimeRemaining()
    {
        return currentTime;
    }

    /// <summary>
    /// Returns true if the timer is still running
    /// </summary>
    public bool IsRunning()
    {
        return isRunning;
    }

    /// <summary>
    /// Pause the timer
    /// </summary>
    public void Pause()
    {
        isRunning = false;
    }

    /// <summary>
    /// Resume the timer
    /// </summary>
    public void Resume()
    {
        if (currentTime > 0f)
            isRunning = true;
    }

    /// <summary>
    /// Reset the timer to the starting time
    /// </summary>
    public void ResetTimer()
    {
        currentTime = startTime;
        isRunning = true;
        UpdateTimerDisplay();
    }
}
