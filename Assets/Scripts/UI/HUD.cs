using UnityEngine;
using UnityEngine.UI;

// The on-screen HUD. It is an OBSERVER: it never asks the GameManager for
// numbers, it only reacts when GameEvents says something changed.
//
// It builds its own Canvas in code, so all you do is put this script on an
// empty GameObject called "HUD".
public class HUD : MonoBehaviour
{
    [Header("Look")]
    public Color mainColor = new Color(1f, 0.86f, 0.35f);      // pirate gold
    public Color heartColor = new Color(0.9f, 0.15f, 0.15f);
    public bool showPlayerState = true;                         // handy for the demo (toggle with F3)

    private Text scoreText;
    private Text treasureText;
    private Text comboText;
    private Text heartsText;
    private Text messageText;
    private Text achievementText;
    private Text bigText;
    private Text stateText;
    private Text controlsText;

    private float messageUntil;
    private float achievementUntil;
    private float comboUntil;
    private Font font;

    void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildCanvas();
    }

    void OnEnable()
    {
        GameEvents.ScoreChanged += OnScoreChanged;
        GameEvents.TreasureChanged += OnTreasureChanged;
        GameEvents.ComboChanged += OnComboChanged;
        GameEvents.HealthChanged += OnHealthChanged;
        GameEvents.Message += OnMessage;
        GameEvents.AchievementUnlocked += OnAchievement;
        GameEvents.PlayerDied += OnPlayerDied;
        GameEvents.GameWon += OnGameWon;
        GameEvents.PlayerStateChanged += OnPlayerStateChanged;
    }

    void OnDisable()
    {
        GameEvents.ScoreChanged -= OnScoreChanged;
        GameEvents.TreasureChanged -= OnTreasureChanged;
        GameEvents.ComboChanged -= OnComboChanged;
        GameEvents.HealthChanged -= OnHealthChanged;
        GameEvents.Message -= OnMessage;
        GameEvents.AchievementUnlocked -= OnAchievement;
        GameEvents.PlayerDied -= OnPlayerDied;
        GameEvents.GameWon -= OnGameWon;
        GameEvents.PlayerStateChanged -= OnPlayerStateChanged;
    }

    void Update()
    {
        // Hide timed text when its time is up.
        messageText.enabled = Time.time < messageUntil;
        achievementText.enabled = Time.time < achievementUntil;
        comboText.enabled = Time.time < comboUntil;

        if (Input.GetKeyDown(KeyCode.F3)) showPlayerState = !showPlayerState;
        stateText.enabled = showPlayerState;
    }

    // ---- Observer reactions ----

    void OnScoreChanged(int score) { scoreText.text = "Score: " + score; }

    void OnTreasureChanged(int collected, int goal)
    {
        treasureText.text = collected >= goal
            ? "Treasure: " + collected + " / " + goal + "  - chest unlocked!"
            : "Treasure: " + collected + " / " + goal;
    }

    void OnComboChanged(int combo)
    {
        if (combo <= 1) { comboUntil = 0f; return; }
        comboText.text = "Combo x" + combo;
        comboUntil = Time.time + 3f;   // the combo window in the C++ plugin is 3 seconds
    }

    void OnHealthChanged(int current, int max)
    {
        // Full hearts in red, lost hearts in faded grey.
        string hearts = "";
        for (int i = 0; i < max; i++)
        {
            hearts += (i < current) ? "♥ " : "<color=#66666699>♥</color> ";
        }
        heartsText.text = hearts.Trim();
    }

    void OnMessage(string text)
    {
        messageText.text = text;
        messageUntil = Time.time + 2.5f;
    }

    void OnAchievement(string title)
    {
        achievementText.text = "Achievement unlocked!\n" + title;
        achievementUntil = Time.time + 3.5f;
    }

    void OnPlayerDied()
    {
        bigText.text = "You were sunk!\n<size=28>Press R to try again</size>";
        bigText.enabled = true;
    }

    void OnGameWon()
    {
        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        bigText.text = "Treasure claimed!\n<size=28>Final score: " + score + "   -   Press R to play again</size>";
        bigText.enabled = true;
    }

    void OnPlayerStateChanged(string stateName) { stateText.text = "Player state: " + stateName; }

    // ---- Building the UI in code ----

    void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("HUD Canvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Top left: score, treasure, combo
        scoreText = MakeText(canvasObject.transform, "Score: 0", 44, TextAnchor.UpperLeft,
                             new Vector2(0f, 1f), new Vector2(40f, -30f), mainColor);
        treasureText = MakeText(canvasObject.transform, "Treasure: 0", 34, TextAnchor.UpperLeft,
                                new Vector2(0f, 1f), new Vector2(40f, -85f), Color.white);
        comboText = MakeText(canvasObject.transform, "", 40, TextAnchor.UpperLeft,
                             new Vector2(0f, 1f), new Vector2(40f, -132f), new Color(1f, 0.55f, 0.1f));

        // Top right: hearts
        heartsText = MakeText(canvasObject.transform, "", 56, TextAnchor.UpperRight,
                              new Vector2(1f, 1f), new Vector2(-40f, -22f), heartColor);

        // Middle top: short messages
        messageText = MakeText(canvasObject.transform, "", 36, TextAnchor.UpperCenter,
                               new Vector2(0.5f, 1f), new Vector2(0f, -170f), Color.white);

        // Bottom right: achievement pop-ups
        achievementText = MakeText(canvasObject.transform, "", 32, TextAnchor.LowerRight,
                                   new Vector2(1f, 0f), new Vector2(-40f, 40f), mainColor);

        // Centre: win / lose
        bigText = MakeText(canvasObject.transform, "", 72, TextAnchor.MiddleCenter,
                           new Vector2(0.5f, 0.5f), Vector2.zero, mainColor);
        bigText.enabled = false;

        // Bottom left: current player state (for showing the State pattern) and controls
        stateText = MakeText(canvasObject.transform, "Player state: Idle", 26, TextAnchor.LowerLeft,
                             new Vector2(0f, 0f), new Vector2(40f, 75f), new Color(0.75f, 0.9f, 1f));
        controlsText = MakeText(canvasObject.transform,
                                "WASD move   Space jump   Right mouse look   F2 rebind jump   F3 show state",
                                22, TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(40f, 35f),
                                new Color(1f, 1f, 1f, 0.7f));
    }

    Text MakeText(Transform parent, string startText, int size, TextAnchor anchor,
                  Vector2 corner, Vector2 offset, Color color)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = corner;
        rect.anchorMax = corner;
        rect.pivot = corner;
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(1400f, 300f);

        Text text = go.AddComponent<Text>();
        text.font = font;
        text.text = startText;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = anchor;
        text.color = color;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        // A dark outline so the text reads on bright sand and sky.
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        return text;
    }
}
