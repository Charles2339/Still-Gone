using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the entire HUD and game-over screen from code — no prefabs needed.
/// </summary>
public class GameUI : MonoBehaviour
{
    private Canvas   _canvas;
    private Text     _scoreText;
    private Text     _coinText;
    private Text     _speedText;
    private GameObject _gameOverPanel;

    private void Awake()
    {
        BuildCanvas();
        BuildHUD();
        BuildGameOverPanel();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        _scoreText.text = $"{(int)GameManager.Instance.Score} m";
        _coinText.text  = $"🪙 {GameManager.Instance.Coins}";
        _speedText.text = $"{GameManager.Instance.Speed:F0} km/h";
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void ShowGameOver(int score, int best, int sessionCoins, int totalCoins)
    {
        _gameOverPanel.SetActive(true);
        var labels = _gameOverPanel.GetComponentsInChildren<Text>();
        foreach (var l in labels)
        {
            if (l.name == "GOScore") l.text  = $"{score} m";
            if (l.name == "GOBest")  l.text  = $"BEST  {best} m";
            if (l.name == "GOCoins") l.text  = $"COINS  {sessionCoins}  (total {totalCoins})";
        }
    }

    // ── Build helpers ──────────────────────────────────────────────────────

    private void BuildCanvas()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var cs = gameObject.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);

        gameObject.AddComponent<GraphicRaycaster>();
    }

    private void BuildHUD()
    {
        _scoreText = MakeText("ScoreText", _canvas.transform,
                              new Vector2(0f, 1f), new Vector2(0f, 1f),
                              new Vector2(20f, -20f), new Vector2(500f, 80f),
                              48, TextAnchor.UpperLeft);
        _scoreText.color = Color.white;

        _coinText = MakeText("CoinText", _canvas.transform,
                              new Vector2(1f, 1f), new Vector2(1f, 1f),
                              new Vector2(-20f, -20f), new Vector2(300f, 80f),
                              42, TextAnchor.UpperRight);
        _coinText.color = new Color(1f, 0.85f, 0.1f);

        _speedText = MakeText("SpeedText", _canvas.transform,
                               new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                               new Vector2(0f, 20f), new Vector2(300f, 60f),
                               32, TextAnchor.LowerCenter);
        _speedText.color = new Color(0.3f, 1f, 0.6f, 0.6f);
    }

    private void BuildGameOverPanel()
    {
        _gameOverPanel = new GameObject("GameOverPanel");
        _gameOverPanel.transform.SetParent(_canvas.transform, false);

        // Dark overlay
        var bg  = _gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        var bgRect = _gameOverPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;

        // Title
        var title = MakeText("GOTitle", _gameOverPanel.transform,
                              new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
                              Vector2.zero, new Vector2(800f, 120f),
                              80, TextAnchor.MiddleCenter);
        title.text  = "STILL GONE";
        title.color = new Color(1f, 0.3f, 0.4f);

        // Score
        var score = MakeText("GOScore", _gameOverPanel.transform,
                              new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
                              Vector2.zero, new Vector2(600f, 80f),
                              56, TextAnchor.MiddleCenter);
        score.color = Color.white;

        // Best
        var best = MakeText("GOBest", _gameOverPanel.transform,
                              new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f),
                              Vector2.zero, new Vector2(600f, 60f),
                              36, TextAnchor.MiddleCenter);
        best.color = new Color(0.3f, 1f, 0.6f);

        // Coins
        var coins = MakeText("GOCoins", _gameOverPanel.transform,
                              new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f),
                              Vector2.zero, new Vector2(600f, 60f),
                              34, TextAnchor.MiddleCenter);
        coins.color = new Color(1f, 0.85f, 0.1f);

        // Restart button
        BuildButton(_gameOverPanel.transform, "PLAY AGAIN",
                     new Vector2(0.5f, 0.3f), new Vector2(400f, 90f),
                     new Color(0.2f, 1f, 0.6f),
                     () => GameManager.Instance?.RestartGame());

        _gameOverPanel.SetActive(false);
    }

    private static Text MakeText(string name, Transform parent,
                                  Vector2 pivot, Vector2 anchor,
                                  Vector2 anchoredPos, Vector2 size,
                                  int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt             = go.AddComponent<RectTransform>();
        rt.pivot           = pivot;
        rt.anchorMin       = anchor;
        rt.anchorMax       = anchor;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta       = size;

        var txt         = go.AddComponent<Text>();
        txt.font        = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize    = fontSize;
        txt.alignment   = alignment;
        txt.text        = "";
        return txt;
    }

    private static void BuildButton(Transform parent, string label,
                                     Vector2 anchor, Vector2 size,
                                     Color color, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);

        var rt           = go.AddComponent<RectTransform>();
        rt.anchorMin     = anchor;
        rt.anchorMax     = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta     = size;

        var img   = go.AddComponent<Image>();
        img.color = color * 0.25f;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        // Label text
        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var txtRt       = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = txtRt.offsetMax = Vector2.zero;

        var txt       = txtGo.AddComponent<Text>();
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text      = label;
        txt.fontSize  = 42;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color     = color;
    }
}
