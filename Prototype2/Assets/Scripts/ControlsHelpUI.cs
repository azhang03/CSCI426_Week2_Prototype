using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlsHelpUI : MonoBehaviour
{
    [Header("Styling")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 1f);
    [SerializeField] private Color panelColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
    [SerializeField] private Color iconColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color highlightColor = new Color(0.4f, 0.7f, 1f, 1f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color exitButtonColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color sliderFillColor = new Color(0.2f, 0.8f, 0.4f, 1f); // Bright green
    [SerializeField] private int fontSize = 16;

    private const string VOLUME_PREF_KEY = "MasterVolume";
    
    private Canvas canvas;
    private GameObject pauseMenuRoot;
    private bool isPaused = false;
    private Slider volumeSlider;
    private Text volumeValueText;

    void Start()
    {
        // Load saved volume (saved as slider value 0-1, apply squared curve)
        float savedSliderValue = PlayerPrefs.GetFloat(VOLUME_PREF_KEY, 1f);
        AudioListener.volume = savedSliderValue * savedSliderValue;
        
        CreateUI();
        // Start hidden
        pauseMenuRoot.SetActive(false);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Developer keybind: R to restart
        if (keyboard.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuRoot.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        
        // Disable/enable player controls when paused
        SetPlayerControlsEnabled(!isPaused);
    }
    
    void SetPlayerControlsEnabled(bool enabled)
    {
        // Disable aiming
        PlayerAiming aiming = FindFirstObjectByType<PlayerAiming>();
        if (aiming != null)
        {
            aiming.enabled = enabled;
            // Also hide/show the aim indicator
            if (!enabled)
                aiming.HideAimIndicator();
            else
                aiming.ShowAimIndicator();
        }
        
        // Disable bow controller (prevents shooting while paused)
        BowController bow = FindFirstObjectByType<BowController>();
        if (bow != null)
        {
            bow.enabled = enabled;
        }
        
        // Disable movement
        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = enabled;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuRoot.SetActive(false);
        Time.timeScale = 1f;
        SetPlayerControlsEnabled(true);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void CreateUI()
    {
        // Find a Screen Space Overlay canvas, or create one
        canvas = null;
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

        // Create pause menu root (fullscreen darkened overlay)
        pauseMenuRoot = new GameObject("PauseMenu");
        pauseMenuRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = pauseMenuRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        Image rootBg = pauseMenuRoot.AddComponent<Image>();
        rootBg.color = backgroundColor;

        // Create center panel
        GameObject centerPanel = CreatePanel("CenterPanel", pauseMenuRoot.transform);
        RectTransform panelRect = centerPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(700, 400); // Taller to fit volume slider

        Image panelImage = centerPanel.GetComponent<Image>();
        panelImage.color = panelColor;

        // Add vertical layout to center panel
        VerticalLayoutGroup vlg = centerPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(30, 30, 25, 25);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Title
        CreateTitle(centerPanel.transform, "PAUSED");

        // Controls row (horizontal)
        CreateControlsRow(centerPanel.transform);
        
        // Volume slider
        CreateVolumeSlider(centerPanel.transform);

        // Exit button
        CreateExitButton(centerPanel.transform);
    }

    void CreateTitle(Transform parent, string text)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);

        RectTransform rect = titleObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 40);

        Text title = titleObj.AddComponent<Text>();
        title.text = text;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 32;
        title.color = textColor;
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;
    }

    void CreateControlsRow(Transform parent)
    {
        GameObject row = new GameObject("ControlsRow");
        row.transform.SetParent(parent, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(640, 120);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Create each control hint horizontally
        CreateMouseClickHint(row.transform);
        CreateMouseMoveHint(row.transform);
        CreateWASDHint(row.transform);
        CreateSpaceBarHint(row.transform);
    }

    void CreateMouseClickHint(Transform parent)
    {
        GameObject container = CreateControlContainer(parent, "Control_ChargeShoot");
        
        CreateLabel(container.transform, "LabelTop", "Left Click + Hold");
        CreateMouseIcon(container.transform, true);
        CreateLabel(container.transform, "LabelBottom", "Charge Shoot");
    }

    void CreateMouseMoveHint(Transform parent)
    {
        GameObject container = CreateControlContainer(parent, "Control_Aim");
        
        CreateLabel(container.transform, "LabelTop", "Mouse Move");
        CreateMouseWithArrowsIcon(container.transform);
        CreateLabel(container.transform, "LabelBottom", "Aim");
    }

    void CreateWASDHint(Transform parent)
    {
        GameObject container = CreateControlContainer(parent, "Control_Move");
        
        CreateLabel(container.transform, "LabelTop", "A / D");
        CreateADIcon(container.transform);
        CreateLabel(container.transform, "LabelBottom", "Move");
    }

    void CreateSpaceBarHint(Transform parent)
    {
        GameObject container = CreateControlContainer(parent, "Control_Jump");
        
        CreateLabel(container.transform, "LabelTop", "Space");
        CreateSpaceBarIcon(container.transform);
        CreateLabel(container.transform, "LabelBottom", "Double Jump");
    }

    GameObject CreateControlContainer(Transform parent, string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);
        
        RectTransform rect = container.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(130, 110);

        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 3;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        return container;
    }

    void CreateLabel(Transform parent, string name, string text)
    {
        GameObject labelObj = new GameObject(name);
        labelObj.transform.SetParent(parent, false);

        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(130, 20);

        Text label = labelObj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAnchor.MiddleCenter;
    }

    void CreateVolumeSlider(Transform parent)
    {
        // Container for label + slider + value
        GameObject container = new GameObject("VolumeContainer");
        container.transform.SetParent(parent, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(500, 40);
        
        HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 15;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        
        // Volume label
        GameObject labelObj = new GameObject("VolumeLabel");
        labelObj.transform.SetParent(container.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(80, 30);
        
        Text label = labelObj.AddComponent<Text>();
        label.text = "Volume";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 20;
        label.color = textColor;
        label.alignment = TextAnchor.MiddleRight;
        
        // Slider
        GameObject sliderObj = new GameObject("VolumeSlider");
        sliderObj.transform.SetParent(container.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(300, 30);
        
        volumeSlider = sliderObj.AddComponent<Slider>();
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = PlayerPrefs.GetFloat(VOLUME_PREF_KEY, 1f); // Use saved slider value
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        
        // Slider background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark background
        
        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.sizeDelta = new Vector2(-20, 0);
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = sliderFillColor;
        
        volumeSlider.fillRect = fillRect;
        
        // Handle slide area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);
        handleAreaRect.anchoredPosition = Vector2.zero;
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 30);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, 1f); // Bright white handle
        
        volumeSlider.handleRect = handleRect;
        volumeSlider.targetGraphic = handleImage;
        
        // Volume value text (percentage)
        GameObject valueObj = new GameObject("VolumeValue");
        valueObj.transform.SetParent(container.transform, false);
        
        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(60, 30);
        
        volumeValueText = valueObj.AddComponent<Text>();
        float savedValue = PlayerPrefs.GetFloat(VOLUME_PREF_KEY, 1f);
        volumeValueText.text = Mathf.RoundToInt(savedValue * 100) + "%";
        volumeValueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        volumeValueText.fontSize = 20;
        volumeValueText.color = textColor;
        volumeValueText.alignment = TextAnchor.MiddleLeft;
    }
    
    void OnVolumeChanged(float value)
    {
        // Apply volume (squared for more natural curve)
        AudioListener.volume = value * value;
        
        // Update display text
        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
        }
        
        // Save preference
        PlayerPrefs.SetFloat(VOLUME_PREF_KEY, value);
        PlayerPrefs.Save();
    }

    void CreateExitButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("ExitButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(200, 50);

        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = exitButtonColor;

        Button btn = buttonObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(ExitGame);

        // Button hover colors
        ColorBlock colors = btn.colors;
        colors.normalColor = exitButtonColor;
        colors.highlightedColor = new Color(exitButtonColor.r + 0.1f, exitButtonColor.g + 0.1f, exitButtonColor.b + 0.1f, 1f);
        colors.pressedColor = new Color(exitButtonColor.r - 0.1f, exitButtonColor.g - 0.1f, exitButtonColor.b - 0.1f, 1f);
        btn.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text btnText = textObj.AddComponent<Text>();
        btnText.text = "EXIT";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 24;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.fontStyle = FontStyle.Bold;
    }

    void CreateMouseIcon(Transform parent, bool highlightLeft)
    {
        GameObject iconContainer = new GameObject("MouseIcon");
        iconContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = iconContainer.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(40, 50);

        // Mouse body
        GameObject body = CreateUIRect(iconContainer.transform, "Body", new Vector2(36, 45), iconColor);
        
        // Left button
        GameObject leftBtn = CreateUIRect(iconContainer.transform, "LeftButton", new Vector2(16, 18), highlightLeft ? highlightColor : iconColor);
        RectTransform leftRect = leftBtn.GetComponent<RectTransform>();
        leftRect.anchoredPosition = new Vector2(-9, 12);

        // Right button
        GameObject rightBtn = CreateUIRect(iconContainer.transform, "RightButton", new Vector2(16, 18), iconColor);
        RectTransform rightRect = rightBtn.GetComponent<RectTransform>();
        rightRect.anchoredPosition = new Vector2(9, 12);

        // Scroll wheel (middle line)
        GameObject scroll = CreateUIRect(iconContainer.transform, "ScrollWheel", new Vector2(4, 12), new Color(0.3f, 0.3f, 0.3f, 1f));
        RectTransform scrollRect = scroll.GetComponent<RectTransform>();
        scrollRect.anchoredPosition = new Vector2(0, 12);
    }

    void CreateMouseWithArrowsIcon(Transform parent)
    {
        GameObject iconContainer = new GameObject("MouseMoveIcon");
        iconContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = iconContainer.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(60, 50);

        // Mouse body (smaller, centered)
        GameObject body = CreateUIRect(iconContainer.transform, "Body", new Vector2(28, 36), iconColor);

        // Left button
        GameObject leftBtn = CreateUIRect(iconContainer.transform, "LeftButton", new Vector2(12, 14), iconColor);
        RectTransform leftRect = leftBtn.GetComponent<RectTransform>();
        leftRect.anchoredPosition = new Vector2(-7, 10);

        // Right button
        GameObject rightBtn = CreateUIRect(iconContainer.transform, "RightButton", new Vector2(12, 14), iconColor);
        RectTransform rightRect = rightBtn.GetComponent<RectTransform>();
        rightRect.anchoredPosition = new Vector2(7, 10);

        // Arrows
        float arrowOffset = 24f;
        CreateArrow(iconContainer.transform, "ArrowUp", new Vector2(0, arrowOffset), 0);
        CreateArrow(iconContainer.transform, "ArrowDown", new Vector2(0, -arrowOffset), 180);
        CreateArrow(iconContainer.transform, "ArrowLeft", new Vector2(-arrowOffset, 0), 90);
        CreateArrow(iconContainer.transform, "ArrowRight", new Vector2(arrowOffset, 0), -90);
    }

    void CreateArrow(Transform parent, string name, Vector2 position, float rotation)
    {
        GameObject arrow = new GameObject(name);
        arrow.transform.SetParent(parent, false);

        RectTransform rect = arrow.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(10, 10);
        rect.anchoredPosition = position;
        rect.localRotation = Quaternion.Euler(0, 0, rotation);

        // Main arrow body (vertical line)
        GameObject stem = CreateUIRect(arrow.transform, "Stem", new Vector2(3, 8), highlightColor);
        RectTransform stemRect = stem.GetComponent<RectTransform>();
        stemRect.anchoredPosition = new Vector2(0, -2);

        // Arrow head
        GameObject head = CreateUIRect(arrow.transform, "Head", new Vector2(6, 6), highlightColor);
        RectTransform headRect = head.GetComponent<RectTransform>();
        headRect.anchoredPosition = new Vector2(0, 4);
        headRect.localRotation = Quaternion.Euler(0, 0, 45);
    }

    void CreateSpaceBarIcon(Transform parent)
    {
        GameObject iconContainer = new GameObject("SpaceBarIcon");
        iconContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = iconContainer.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(100, 30);

        // Space bar body
        GameObject body = CreateUIRect(iconContainer.transform, "Body", new Vector2(90, 26), iconColor);
        
        // Inner darker area to give depth
        GameObject inner = CreateUIRect(iconContainer.transform, "Inner", new Vector2(80, 18), new Color(0.7f, 0.7f, 0.7f, 1f));
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        innerRect.anchoredPosition = new Vector2(0, -1);
    }

    void CreateADIcon(Transform parent)
    {
        GameObject iconContainer = new GameObject("ADIcon");
        iconContainer.transform.SetParent(parent, false);
        
        RectTransform containerRect = iconContainer.AddComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(80, 55);

        float keySize = 24f;
        float keySpacing = 8f;
        float arrowOffset = 18f; // How far above the keys the arrows appear

        // A key (left)
        CreateKeyIcon(iconContainer.transform, "A", new Vector2(-keySize / 2 - keySpacing / 2, -5), keySize);
        
        // D key (right)
        CreateKeyIcon(iconContainer.transform, "D", new Vector2(keySize / 2 + keySpacing / 2, -5), keySize);
        
        // Left arrow above A key (pointing left, rotation = 90)
        CreateArrow(iconContainer.transform, "ArrowLeft", new Vector2(-keySize / 2 - keySpacing / 2, arrowOffset), 90);
        
        // Right arrow above D key (pointing right, rotation = -90)
        CreateArrow(iconContainer.transform, "ArrowRight", new Vector2(keySize / 2 + keySpacing / 2, arrowOffset), -90);
    }

    void CreateKeyIcon(Transform parent, string keyLabel, Vector2 position, float size)
    {
        GameObject key = new GameObject($"Key_{keyLabel}");
        key.transform.SetParent(parent, false);

        RectTransform keyRect = key.AddComponent<RectTransform>();
        keyRect.sizeDelta = new Vector2(size, size);
        keyRect.anchoredPosition = position;

        // Key background
        Image keyBg = key.AddComponent<Image>();
        keyBg.color = iconColor;

        // Key label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(key.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
        labelRect.anchoredPosition = Vector2.zero;

        Text label = labelObj.AddComponent<Text>();
        label.text = keyLabel;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        label.alignment = TextAnchor.MiddleCenter;
        label.fontStyle = FontStyle.Bold;
    }

    GameObject CreateUIRect(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = size;

        Image img = obj.AddComponent<Image>();
        img.color = color;

        return obj;
    }

    GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<Image>();
        return panel;
    }
}
