using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private int fontSize = 36;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Vector2 position = new Vector2(20, -20);
    
    private Text scoreText;
    private int score;
    
    void Start()
    {
        score = 0;
        CreateScoreUI();
        UpdateScoreText();
    }

    void CreateScoreUI()
    {
        // Find a Screen Space Overlay canvas, or create one
        Canvas canvas = null;
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

        // Create score text object
        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(canvas.transform, false);

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

    public void ScorePlus()
    {
        score++;
    }
    
    public int GetScore()
    {
        return score;
    }
}
