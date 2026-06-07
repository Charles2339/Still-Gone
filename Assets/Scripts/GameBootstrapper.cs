using UnityEngine;

/// <summary>
/// Entry point. Uses RuntimeInitializeOnLoadMethod so nothing needs to be
/// placed in the scene manually — the game creates itself from pure code.
/// </summary>
public static class GameBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
        Object.DontDestroyOnLoad(go);
    }
}
