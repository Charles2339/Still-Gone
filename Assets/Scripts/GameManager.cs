using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Playing, GameOver, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── State ──────────────────────────────────────────────────────────────
    public GameState State { get; private set; } = GameState.Playing;
    public float     Score { get; private set; }
    public int       Coins { get; private set; }          // session coins
    public float     Speed { get; private set; }

    // ── Config ─────────────────────────────────────────────────────────────
    [Header("Speed")]
    public float baseSpeed     = 6f;
    public float maxSpeed      = 18f;
    public float speedRampRate = 0.5f;

    // ── Internal refs (created in code) ────────────────────────────────────
    private StickmanController _player;
    private ObstacleSpawner    _obstacles;
    private CoinSpawner        _coinSpawner;
    private BackgroundTiler    _background;
    private CameraFollow       _cam;
    private GameUI             _ui;
    private GroundBuilder      _ground;

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;
        Speed = baseSpeed;

        BuildScene();
    }

    private void BuildScene()
    {
        // Ground
        _ground = new GameObject("Ground").AddComponent<GroundBuilder>();
        _ground.Build();

        // Background
        _background = new GameObject("Background").AddComponent<BackgroundTiler>();
        _background.Initialize();

        // Stickman
        var playerGo = StickmanBuilder.Build(new Vector2(-3f, 2f));
        _player = playerGo.GetComponent<StickmanController>();

        // Camera
        var camGo = Camera.main?.gameObject ?? new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        if (camGo.GetComponent<Camera>() == null)
        {
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 10f;
            cam.backgroundColor  = new Color(0.008f, 0.024f, 0.047f);
            cam.transform.position = new Vector3(0f, 3f, -10f);
        }
        _cam = camGo.AddComponent<CameraFollow>();
        _cam.target = playerGo.transform;

        // Spawners
        var spawnGo = new GameObject("Spawners");
        _obstacles  = spawnGo.AddComponent<ObstacleSpawner>();
        _coinSpawner = spawnGo.AddComponent<CoinSpawner>();

        // UI
        _ui = new GameObject("GameUI").AddComponent<GameUI>();
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        Speed  = Mathf.Min(Speed + speedRampRate * Time.deltaTime, maxSpeed);
        Score += Speed * Time.deltaTime * 0.5f;   // score = distance-ish metres
    }

    // ── Events ──────────────────────────────────────────────────────────────

    public void AddCoin()
    {
        Coins++;
        GamePrefs.AddCoins(1);
    }

    public void TriggerGameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;
        GamePrefs.SaveHighScore((int)Score);
        Time.timeScale = 0.4f;    // dramatic slow-mo on death
        Invoke(nameof(ShowGameOver), 0.8f);
    }

    private void ShowGameOver()
    {
        Time.timeScale = 1f;
        _ui.ShowGameOver((int)Score, GamePrefs.GetHighScore(), Coins, GamePrefs.GetTotalCoins());
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Instance = null;
    }
}
