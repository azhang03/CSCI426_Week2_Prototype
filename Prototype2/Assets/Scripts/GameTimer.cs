using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Starting time in seconds (120 = 2 minutes)")]
    [SerializeField] private float startTime = 120f;
    
    [Header("Display Settings")]
    [SerializeField] private int fontSize = 200;
    [SerializeField] private Color textColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private float topOffset = 0.8f; // How far from center (0 = center, 1 = top edge)
    
    [Header("Danger Phase Settings")]
    [Tooltip("Time elapsed (in seconds) when danger phase begins and color changes")]
    [SerializeField] private float dangerPhaseStartTime = 60f;
    
    [Tooltip("Timer color after danger phase begins (wolves start spawning)")]
    [SerializeField] private Color dangerPhaseColor = new Color(1f, 0.3f, 0.3f, 0.6f);
    
    [Tooltip("How long the color transition takes")]
    [SerializeField] private float colorTransitionDuration = 1f;
    
    [Header("Sorting")]
    [Tooltip("Sorting order for the timer (lower = further back)")]
    [SerializeField] private int sortingOrder = -10;
    [SerializeField] private string sortingLayerName = "Default";

    private Text timerText;
    private Canvas timerCanvas;
    private Camera mainCamera;
    private float currentTime;
    private bool isRunning = false; // Start paused until first target hit
    private bool hasStarted = false; // Track if game has truly started
    private float elapsedTime = 0f;
    private bool inDangerPhase = false;
    private float colorTransitionProgress = 0f;
    
    // Singleton for easy access
    public static GameTimer Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        currentTime = startTime;
        CreateTimerUI();
        UpdateTimerDisplay();
    }

    void Update()
    {
        // Developer keybind: Press 1 to fast-forward 10 seconds
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.digit1Key.wasPressedThisFrame && hasStarted)
        {
            FastForward(10f);
        }
        
        if (!isRunning) return;
        
        currentTime -= Time.deltaTime;
        elapsedTime += Time.deltaTime;
        
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            OnTimerEnd();
        }
        
        // Check for danger phase transition
        if (!inDangerPhase && elapsedTime >= dangerPhaseStartTime)
        {
            inDangerPhase = true;
            Debug.Log("Danger phase started! Timer color changing.");
        }
        
        // Smoothly transition color if in danger phase
        if (inDangerPhase && colorTransitionProgress < 1f)
        {
            colorTransitionProgress += Time.deltaTime / colorTransitionDuration;
            colorTransitionProgress = Mathf.Clamp01(colorTransitionProgress);
            
            if (timerText != null)
            {
                timerText.color = Color.Lerp(textColor, dangerPhaseColor, colorTransitionProgress);
            }
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
        Debug.Log("Time's up! You survived!");
        
        // Disable player controls
        DisablePlayerControls();
        
        // Get final score
        Score scoreManager = FindFirstObjectByType<Score>();
        int finalScore = scoreManager != null ? scoreManager.GetScore() : 0;
        
        // Show win screen via GameEndUI
        if (GameEndUI.Instance != null)
        {
            GameEndUI.Instance.ShowWinScreen(finalScore);
        }
        else
        {
            Debug.LogWarning("GameEndUI not found! Add a GameEndUI component to the scene.");
        }
    }
    
    void DisablePlayerControls()
    {
        // Find player and disable controls
        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
            
            // Also disable aiming
            PlayerAiming aiming = movement.GetComponent<PlayerAiming>();
            if (aiming != null)
            {
                aiming.HideAimIndicator();
                aiming.enabled = false;
            }
            
            // Disable bow
            BowController bow = movement.GetComponent<BowController>();
            if (bow != null)
                bow.enabled = false;
            
            // Stop rigidbody
            Rigidbody2D rb = movement.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
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
    /// Start the countdown (called when first target is hit)
    /// </summary>
    public void StartCountdown()
    {
        if (hasStarted) return; // Already started
        
        hasStarted = true;
        isRunning = true;
        Debug.Log("Timer started! First target hit.");
    }
    
    /// <summary>
    /// Developer tool: Fast-forward time by specified seconds
    /// </summary>
    public void FastForward(float seconds)
    {
        // Update timer
        currentTime -= seconds;
        elapsedTime += seconds;
        
        // Clamp to prevent negative time
        if (currentTime < 0f)
        {
            currentTime = 0f;
        }
        
        // Also fast-forward Score's elapsed time
        Score scoreManager = FindFirstObjectByType<Score>();
        if (scoreManager != null)
        {
            scoreManager.AddElapsedTime(seconds);
        }
        
        // Also fast-forward FallingRockSpawner's elapsed time
        FallingRockSpawner spawner = FindFirstObjectByType<FallingRockSpawner>();
        if (spawner != null)
        {
            spawner.AddElapsedTime(seconds);
        }
        
        Debug.Log($"[DEV] Fast-forwarded {seconds} seconds! Elapsed: {elapsedTime:F1}s, Remaining: {currentTime:F1}s");
        
        UpdateTimerDisplay();
    }
    
    /// <summary>
    /// Returns true if the game has started (first target was hit)
    /// </summary>
    public bool HasGameStarted()
    {
        return hasStarted;
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
        if (currentTime > 0f && hasStarted)
            isRunning = true;
    }

    /// <summary>
    /// Reset the timer to the starting time
    /// </summary>
    public void ResetTimer()
    {
        currentTime = startTime;
        elapsedTime = 0f;
        isRunning = false;
        hasStarted = false;
        inDangerPhase = false;
        colorTransitionProgress = 0f;
        
        if (timerText != null)
        {
            timerText.color = textColor;
        }
        
        UpdateTimerDisplay();
    }
    
    /// <summary>
    /// Returns true if the game is in danger phase (wolves spawning)
    /// </summary>
    public bool IsInDangerPhase()
    {
        return inDangerPhase;
    }
}
