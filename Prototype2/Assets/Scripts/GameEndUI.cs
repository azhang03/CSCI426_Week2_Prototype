using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameEndUI : MonoBehaviour
{
    [Header("Death Screen")]
    [SerializeField] private string deathTitle = "You met a premature end!";
    [SerializeField] private string deathButtonText = "Try Again";
    [SerializeField] private Color deathTitleColor = new Color(1f, 0.3f, 0.3f, 1f);
    
    [Header("Win Screen")]
    [SerializeField] private string winTitle = "You had a fruitful practice session!";
    [SerializeField] private string winButtonText = "Again!";
    [SerializeField] private Color winTitleColor = new Color(0.3f, 1f, 0.5f, 1f);
    
    [Header("UI Styling")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.85f);
    [SerializeField] private Color panelColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
    [SerializeField] private int titleFontSize = 36;
    [SerializeField] private int scoreFontSize = 72;
    [SerializeField] private int bestScoreFontSize = 28;
    [SerializeField] private Color scoreColor = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField] private Color bestScoreColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    
    private const string BEST_SCORE_KEY = "BestScore";
    
    private Canvas canvas;
    private GameObject endScreenRoot;
    private bool isShowing = false;
    
    // Singleton for easy access
    public static GameEndUI Instance { get; private set; }
    
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
        FindOrCreateCanvas();
    }
    
    void FindOrCreateCanvas()
    {
        // Find a Screen Space Overlay canvas, or create one
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in allCanvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = c;
                break;
            }
        }
        
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UICanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
    }
    
    /// <summary>
    /// Show the death screen with current score
    /// </summary>
    public void ShowDeathScreen(int finalScore)
    {
        if (isShowing) return;
        ShowEndScreen(false, finalScore);
    }
    
    /// <summary>
    /// Show the win screen with current score
    /// </summary>
    public void ShowWinScreen(int finalScore)
    {
        if (isShowing) return;
        ShowEndScreen(true, finalScore);
    }
    
    void ShowEndScreen(bool isWin, int finalScore)
    {
        isShowing = true;
        
        // Update best score
        int bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        if (finalScore > bestScore)
        {
            bestScore = finalScore;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, bestScore);
            PlayerPrefs.Save();
        }
        
        // Make sure we have a canvas
        if (canvas == null)
        {
            FindOrCreateCanvas();
        }
        
        // Create the end screen UI
        CreateEndScreenUI(isWin, finalScore, bestScore);
        
        // Ensure time is paused
        Time.timeScale = 0f;
    }
    
    void CreateEndScreenUI(bool isWin, int finalScore, int bestScore)
    {
        // Create root (fullscreen darkened overlay)
        endScreenRoot = new GameObject("EndScreen");
        endScreenRoot.transform.SetParent(canvas.transform, false);
        
        RectTransform rootRect = endScreenRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;
        
        Image rootBg = endScreenRoot.AddComponent<Image>();
        rootBg.color = backgroundColor;
        
        // Create center panel
        GameObject centerPanel = new GameObject("CenterPanel");
        centerPanel.transform.SetParent(endScreenRoot.transform, false);
        
        RectTransform panelRect = centerPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(700, 400);
        
        Image panelImage = centerPanel.AddComponent<Image>();
        panelImage.color = panelColor;
        
        // Add vertical layout
        VerticalLayoutGroup vlg = centerPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(40, 40, 40, 40);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        
        // Title
        string title = isWin ? winTitle : deathTitle;
        Color titleColor = isWin ? winTitleColor : deathTitleColor;
        CreateText(centerPanel.transform, "Title", title, titleFontSize, titleColor, 60);
        
        // Score (big)
        CreateText(centerPanel.transform, "Score", finalScore.ToString(), scoreFontSize, scoreColor, 90, FontStyle.Bold);
        
        // Best score label
        string bestScoreText = finalScore >= bestScore && finalScore > 0 
            ? "NEW BEST!" 
            : $"Best: {bestScore}";
        Color bestColor = finalScore >= bestScore && finalScore > 0 
            ? new Color(1f, 0.8f, 0.2f, 1f) 
            : bestScoreColor;
        CreateText(centerPanel.transform, "BestScore", bestScoreText, bestScoreFontSize, bestColor, 40);
        
        // Spacer
        CreateSpacer(centerPanel.transform, 20);
        
        // Button
        string buttonText = isWin ? winButtonText : deathButtonText;
        CreateButton(centerPanel.transform, buttonText);
    }
    
    void CreateText(Transform parent, string name, string content, int fontSize, Color color, float height, FontStyle style = FontStyle.Normal)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, height);
        
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = style;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }
    
    void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        
        RectTransform rect = spacer.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, height);
        
        LayoutElement layout = spacer.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
    }
    
    void CreateButton(Transform parent, string buttonText)
    {
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(250, 60);
        
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = buttonColor;
        
        Button btn = buttonObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(RestartGame);
        
        // Button hover colors
        ColorBlock colors = btn.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(buttonColor.r + 0.15f, buttonColor.g + 0.15f, buttonColor.b + 0.15f, 1f);
        colors.pressedColor = new Color(buttonColor.r - 0.1f, buttonColor.g - 0.1f, buttonColor.b - 0.1f, 1f);
        btn.colors = colors;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Text btnTextComponent = textObj.AddComponent<Text>();
        btnTextComponent.text = buttonText;
        btnTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnTextComponent.fontSize = 28;
        btnTextComponent.color = Color.white;
        btnTextComponent.alignment = TextAnchor.MiddleCenter;
        btnTextComponent.fontStyle = FontStyle.Bold;
    }
    
    void RestartGame()
    {
        Time.timeScale = 1f;
        isShowing = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
