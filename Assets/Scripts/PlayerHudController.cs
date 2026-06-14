using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[ExecuteAlways]
public class PlayerHudController : MonoBehaviour
{
    private struct LoadoutSlot
    {
        public Image Icon;
        public TextMeshProUGUI Label;
        public GameObject Root;
    }

    private Canvas canvas;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI timerText;
    private RectTransform hpFillRect;
    private RectTransform xpFillRect;
    private Button[] levelUpButtons;
    private TextMeshProUGUI[] levelUpOptions;
    private GameObject levelUpPanel;
    private GameObject gameOverPanel;
    private TextMeshProUGUI gameOverText;
    private GameObject chestPanel;
    private TextMeshProUGUI chestText;
    private GameObject pausePanel;
    private TextMeshProUGUI pauseHeaderText;
    private LoadoutSlot[] weaponSlots;
    private LoadoutSlot[] passiveSlots;
    private bool initialized;

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        EnsureCanvas();
        BuildHud();
        BuildLevelUpPanel();
        BuildChestPanel();
        BuildGameOverPanel();
        BuildPausePanel();
        initialized = true;
    }

    private void OnEnable()
    {
        Initialize();

        if (!Application.isPlaying)
        {
            SetLevel(1);
            SetHp(5, 5);
            SetXp(0f, 1f);
            SetRunTime(0f);
            SetWeapons(new[] { "Hunter Bow Lv.1" });
            SetPassives(new string[0]);
            HidePauseMenu();
            HideLevelUp();
            HideChestReward();
        }
    }

    public void SetScore(int score)
    {
    }

    public void SetLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = "Level " + level;
        }

        if (pauseHeaderText != null)
        {
            pauseHeaderText.text = "Paused";
        }
    }

    public void SetHp(int currentHp, int maxHp)
    {
        SetBarFill(hpFillRect, maxHp <= 0 ? 0f : (float)currentHp / maxHp);
    }

    public void SetXp(float currentXp, float requiredXp)
    {
        SetBarFill(xpFillRect, requiredXp <= 0f ? 0f : currentXp / requiredXp);
    }

    public void SetRunTime(float runTime)
    {
        int totalSeconds = Mathf.FloorToInt(runTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string formatted = minutes.ToString("00") + ":" + seconds.ToString("00");

        if (timerText != null)
        {
            timerText.text = formatted;
        }
    }

    public void SetWeapons(string[] weapons)
    {
        UpdateLoadoutSlots(weaponSlots, weapons, true);
    }

    public void SetPassives(string[] passives)
    {
        UpdateLoadoutSlots(passiveSlots, passives, false);
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void ShowLevelUp(string[] options)
    {
        if (levelUpPanel == null)
        {
            return;
        }

        levelUpPanel.SetActive(true);
        for (int i = 0; i < levelUpOptions.Length; i++)
        {
            bool enabled = i < options.Length;
            levelUpButtons[i].gameObject.SetActive(enabled);
            if (enabled)
            {
                int optionIndex = i;
                levelUpOptions[i].text = options[i];
                levelUpButtons[i].onClick.RemoveAllListeners();
                levelUpButtons[i].onClick.AddListener(() => GameManager.Instance?.SelectUpgradeOption(optionIndex));
            }
        }
    }

    public void HideLevelUp()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    public void ShowChestReward(string message)
    {
        if (chestPanel == null)
        {
            return;
        }

        chestText.text = message;
        chestPanel.SetActive(true);
    }

    public void HideChestReward()
    {
        if (chestPanel != null)
        {
            chestPanel.SetActive(false);
        }
    }

    public void ShowGameOver(int score, int level)
    {
        if (gameOverPanel == null)
        {
            return;
        }

        gameOverText.text = "GAME OVER\n\nFinal Level: " + level + "\nKills: " + score;
        gameOverPanel.SetActive(true);
    }

    private void EnsureCanvas()
    {
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void BuildHud()
    {
        levelText = FindOrCreateText("LevelHud", new Vector2(0f, 30f), new Vector2(240f, 42f), 30f, TextAlignmentOptions.Center, HudAnchor.BottomCenter);
        timerText = FindOrCreateText("RunTimerHud", new Vector2(0f, -18f), new Vector2(180f, 40f), 32f, TextAlignmentOptions.Center, HudAnchor.TopCenter);

        CreateAnchoredBar(
            "HpBar",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -20f),
            new Vector2(320f, 24f),
            new Color(0.12f, 0.06f, 0.06f, 0.98f),
            new Color(0.96f, 0.18f, 0.24f, 1f),
            out hpFillRect
        );

        CreateAnchoredBar(
            "XpBar",
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 24f),
            new Color(0.06f, 0.09f, 0.14f, 0.98f),
            new Color(0.15f, 0.8f, 1f, 1f),
            out xpFillRect
        );

        CreateButton(canvas.transform, "PauseHudButton", new Vector2(-22f, -22f), new Vector2(96f, 40f), "Pause", () => PauseMenu.Instance?.TogglePause(), HudAnchor.TopRight);

        HideLegacyText("LifeText");
        HideLegacyText("ScoreText");
    }

    private void BuildLevelUpPanel()
    {
        levelUpPanel = CreatePanel("LevelUpPanel", new Vector2(0.5f, 0.5f), new Vector2(900f, 560f), new Color(0f, 0f, 0f, 0.84f));
        CreateChildText(levelUpPanel.transform, "LevelUpTitle", new Vector2(0f, 210f), new Vector2(760f, 60f), 42f, "LEVEL UP", TextAlignmentOptions.Center);

        levelUpButtons = new Button[3];
        levelUpOptions = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            Button button = CreateButton(levelUpPanel.transform, "OptionCard" + i, new Vector2(0f, 90f - i * 130f), new Vector2(720f, 94f), "", null, HudAnchor.Center);
            levelUpButtons[i] = button;
            levelUpOptions[i] = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        levelUpPanel.SetActive(false);
    }

    private void BuildChestPanel()
    {
        chestPanel = CreatePanel("ChestPanel", new Vector2(0.5f, 0.5f), new Vector2(720f, 260f), new Color(0f, 0f, 0f, 0.82f));
        chestText = CreateChildText(chestPanel.transform, "ChestText", new Vector2(0f, 28f), new Vector2(640f, 150f), 30f, "", TextAlignmentOptions.Center);
        CreateButton(chestPanel.transform, "ChestContinueButton", new Vector2(0f, -78f), new Vector2(220f, 56f), "Continue", () => GameManager.Instance?.ContinueChestReward(), HudAnchor.Center);
        chestPanel.SetActive(false);
    }

    private void BuildGameOverPanel()
    {
        gameOverPanel = CreatePanel("GameOverPanel", new Vector2(0.5f, 0.5f), new Vector2(760f, 360f), new Color(0.02f, 0.02f, 0.02f, 0.92f));
        gameOverText = CreateChildText(gameOverPanel.transform, "GameOverText", new Vector2(0f, 44f), new Vector2(660f, 200f), 34f, "", TextAlignmentOptions.Center);
        CreateButton(gameOverPanel.transform, "RestartButton", new Vector2(-130f, -112f), new Vector2(220f, 60f), "Restart", () => GameManager.Instance?.RestartFromUi(), HudAnchor.Center);
        CreateButton(gameOverPanel.transform, "MainMenuButton", new Vector2(130f, -112f), new Vector2(220f, 60f), "Main Menu", () => GameManager.Instance?.LoadMainMenuFromUi(), HudAnchor.Center);
        gameOverPanel.SetActive(false);
    }

    private void BuildPausePanel()
    {
        pausePanel = CreatePanel("PausePanel", new Vector2(0.5f, 0.5f), new Vector2(1020f, 680f), new Color(0f, 0f, 0f, 0.88f));
        pauseHeaderText = CreateChildText(pausePanel.transform, "PauseHeader", new Vector2(0f, 286f), new Vector2(500f, 80f), 40f, "Paused", TextAlignmentOptions.Center);
        CreateChildText(pausePanel.transform, "WeaponsTitle", new Vector2(-230f, 224f), new Vector2(220f, 34f), 28f, "Weapons", TextAlignmentOptions.Left);
        CreateChildText(pausePanel.transform, "PassivesTitle", new Vector2(230f, 224f), new Vector2(220f, 34f), 28f, "Passives", TextAlignmentOptions.Left);

        weaponSlots = BuildLoadoutColumn(pausePanel.transform, "WeaponSlot", -230f, true);
        passiveSlots = BuildLoadoutColumn(pausePanel.transform, "PassiveSlot", 230f, false);

        CreateButton(pausePanel.transform, "PauseResumeButton", new Vector2(0f, -228f), new Vector2(220f, 58f), "Resume", () => PauseMenu.Instance?.Resume(), HudAnchor.Center);
        CreateButton(pausePanel.transform, "PauseRestartButton", new Vector2(-140f, -304f), new Vector2(220f, 58f), "Restart", () => PauseMenu.Instance?.RestartLevel(), HudAnchor.Center);
        CreateButton(pausePanel.transform, "PauseMenuButton", new Vector2(140f, -304f), new Vector2(220f, 58f), "Main Menu", () => PauseMenu.Instance?.LoadMainMenu(), HudAnchor.Center);
        pausePanel.SetActive(false);
    }

    private LoadoutSlot[] BuildLoadoutColumn(Transform parent, string prefix, float x, bool weapons)
    {
        LoadoutSlot[] slots = new LoadoutSlot[4];
        for (int i = 0; i < slots.Length; i++)
        {
            GameObject slotRoot = FindChildByName(parent, prefix + i);
            if (slotRoot == null)
            {
                slotRoot = new GameObject(prefix + i, typeof(RectTransform), typeof(Image));
            }

            RectTransform rect = slotRoot.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 124f - i * 86f);
            rect.sizeDelta = new Vector2(360f, 74f);
            slotRoot.GetComponent<Image>().color = new Color(0.14f, 0.14f, 0.16f, 1f);

            GameObject iconObject = FindChildByName(slotRoot.transform, prefix + i + "_Icon");
            if (iconObject == null)
            {
                iconObject = new GameObject(prefix + i + "_Icon", typeof(RectTransform), typeof(Image));
            }

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(slotRoot.transform, false);
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(54f, 54f);

            Image icon = iconObject.GetComponent<Image>();
            icon.sprite = GetPlaceholderIcon(weapons ? i : i + 10, weapons);
            icon.color = weapons ? GetWeaponColor(i) : GetPassiveColor(i);

            TextMeshProUGUI label = FindTextByName(prefix + i + "_Label");
            if (label == null || label.transform.parent != slotRoot.transform)
            {
                label = CreateChildText(slotRoot.transform, prefix + i + "_Label", new Vector2(80f, 0f), new Vector2(244f, 54f), 20f, "---", TextAlignmentOptions.Left);
            }
            else
            {
                label.rectTransform.anchoredPosition = new Vector2(80f, 0f);
                label.rectTransform.sizeDelta = new Vector2(244f, 54f);
                label.fontSize = 20f;
            }

            label.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            label.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            label.rectTransform.pivot = new Vector2(0f, 0.5f);

            slots[i] = new LoadoutSlot
            {
                Root = slotRoot,
                Icon = icon,
                Label = label
            };
        }

        return slots;
    }

    private void UpdateLoadoutSlots(LoadoutSlot[] slots, string[] entries, bool weapons)
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            bool occupied = entries != null && i < entries.Length;
            slots[i].Label.text = occupied ? entries[i] : "---";
            slots[i].Icon.color = occupied
                ? (weapons ? GetWeaponColor(i) : GetPassiveColor(i))
                : new Color(0.26f, 0.26f, 0.29f, 1f);
        }
    }

    private void CreateAnchoredBar(
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color backgroundColor,
        Color fillColor,
        out RectTransform fillRect)
    {
        GameObject root = FindChildByName(canvas.transform, name);
        if (root == null)
        {
            root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
        }

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = anchorMin;
        rootRect.anchorMax = anchorMax;
        rootRect.pivot = pivot;
        rootRect.anchoredPosition = anchoredPosition;
        rootRect.sizeDelta = sizeDelta;
        root.GetComponent<Image>().color = backgroundColor;
        Outline outline = root.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject fill = FindChildByName(root.transform, name + "_Fill");
        if (fill == null)
        {
            fill = new GameObject(name + "_Fill", typeof(RectTransform), typeof(Image));
        }

        fillRect = fill.GetComponent<RectTransform>();
        fillRect.SetParent(root.transform, false);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.localScale = Vector3.one;
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);
        fill.GetComponent<Image>().color = fillColor;
    }

    private void SetBarFill(RectTransform fillRect, float normalized)
    {
        if (fillRect == null)
        {
            return;
        }

        float clamped = Mathf.Clamp01(normalized);
        fillRect.localScale = new Vector3(clamped, 1f, 1f);
        fillRect.gameObject.SetActive(clamped > 0.001f);
    }

    private TextMeshProUGUI FindOrCreateText(string name, Vector2 anchoredPosition, Vector2 size, float fontSize, TextAlignmentOptions alignment, HudAnchor anchor)
    {
        TextMeshProUGUI existing = FindTextByName(name);
        if (existing != null)
        {
            RectTransform existingRect = existing.rectTransform;
            existingRect.SetParent(canvas.transform, false);
            ApplyAnchor(existingRect, anchor);
            existingRect.anchoredPosition = anchoredPosition;
            existingRect.sizeDelta = size;
            existing.fontSize = fontSize;
            existing.alignment = alignment;
            existing.color = Color.white;
            return existing;
        }

        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        ApplyAnchor(rect, anchor);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = "";
        return text;
    }

    private TextMeshProUGUI CreateChildText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float fontSize, string textValue, TextAlignmentOptions alignment)
    {
        GameObject textObject = FindChildByName(parent, name);
        if (textObject == null)
        {
            textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        }

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = textValue;
        return text;
    }

    private GameObject CreatePanel(string name, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject panel = FindChildByName(canvas.transform, name);
        if (panel == null)
        {
            panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        }

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private Button CreateButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, string label, UnityEngine.Events.UnityAction onClick, HudAnchor anchor)
    {
        GameObject buttonObject = FindChildByName(parent, name);
        if (buttonObject == null)
        {
            buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        ApplyAnchor(rect, anchor);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.22f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.8f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        button.colors = colors;

        button.onClick.RemoveAllListeners();
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        TextMeshProUGUI existingLabel = FindTextByName(name + "_Text");
        if (existingLabel == null || existingLabel.transform.parent != buttonObject.transform)
        {
            existingLabel = CreateChildText(buttonObject.transform, name + "_Text", Vector2.zero, new Vector2(size.x - 20f, size.y - 10f), 24f, label, TextAlignmentOptions.Center);
        }
        else
        {
            existingLabel.text = label;
            existingLabel.rectTransform.anchoredPosition = Vector2.zero;
            existingLabel.rectTransform.sizeDelta = new Vector2(size.x - 20f, size.y - 10f);
            existingLabel.fontSize = 24f;
            existingLabel.alignment = TextAlignmentOptions.Center;
        }

        return button;
    }

    private GameObject FindChildByName(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        Transform child = parent.Find(name);
        return child != null ? child.gameObject : null;
    }

    private void ApplyAnchor(RectTransform rect, HudAnchor anchor)
    {
        switch (anchor)
        {
            case HudAnchor.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                break;
            case HudAnchor.TopRight:
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                break;
            case HudAnchor.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                break;
            case HudAnchor.Center:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
            default:
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                break;
        }
    }

    private void HideLegacyText(string name)
    {
        TextMeshProUGUI oldText = FindTextByName(name);
        if (oldText != null)
        {
            oldText.gameObject.SetActive(false);
        }

        Text[] legacyTexts = FindObjectsByType<Text>(FindObjectsInactive.Include);
        for (int i = 0; i < legacyTexts.Length; i++)
        {
            if (legacyTexts[i].name == name)
            {
                legacyTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private static Color GetWeaponColor(int index)
    {
        Color[] colors =
        {
            new Color(0.95f, 0.48f, 0.28f, 1f),
            new Color(0.28f, 0.83f, 1f, 1f),
            new Color(1f, 0.82f, 0.32f, 1f),
            new Color(0.38f, 0.95f, 0.64f, 1f)
        };
        return colors[index % colors.Length];
    }

    private static Color GetPassiveColor(int index)
    {
        Color[] colors =
        {
            new Color(0.76f, 0.46f, 1f, 1f),
            new Color(0.98f, 0.58f, 0.18f, 1f),
            new Color(0.3f, 0.9f, 0.54f, 1f),
            new Color(1f, 0.32f, 0.48f, 1f)
        };
        return colors[index % colors.Length];
    }

    private static Sprite GetPlaceholderIcon(int variant, bool weapon)
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool filled;
                if (weapon)
                {
                    switch (variant % 4)
                    {
                        case 0:
                            filled = x >= 8 && x <= 24 && y >= 8 && y <= 24;
                            break;
                        case 1:
                            filled = Mathf.Abs(x - 16) + Mathf.Abs(y - 16) <= 10;
                            break;
                        case 2:
                            filled = (x - 16) * (x - 16) + (y - 16) * (y - 16) <= 96;
                            break;
                        default:
                            filled = y >= 8 && y <= 24 && x >= 10 && x <= 22 && Mathf.Abs(x - 16) <= y - 8;
                            break;
                    }
                }
                else
                {
                    switch (variant % 4)
                    {
                        case 0:
                            filled = x >= 6 && x <= 25 && y >= 12 && y <= 19;
                            break;
                        case 1:
                            filled = x >= 12 && x <= 19 && y >= 6 && y <= 25;
                            break;
                        case 2:
                            filled = Mathf.Abs(x - 16) <= 3 || Mathf.Abs(y - 16) <= 3;
                            break;
                        default:
                            filled = Mathf.Abs(x - 16) + Mathf.Abs(y - 16) <= 12 && Mathf.Abs(x - 16) + Mathf.Abs(y - 16) >= 5;
                            break;
                    }
                }

                pixels[y * size + x] = filled ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    private TextMeshProUGUI FindTextByName(string name)
    {
        TextMeshProUGUI[] texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name == name)
            {
                return text;
            }
        }

        return null;
    }

    private enum HudAnchor
    {
        TopLeft,
        TopRight,
        TopCenter,
        BottomCenter,
        Center
    }
}
