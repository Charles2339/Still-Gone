using UnityEngine;
using System.Collections.Generic;

public class CoinSpawner : MonoBehaviour
{
    private const float CoinY       = 1.8f;    // above ground
    private const float CoinSpacing = 1.8f;
    private const float MinGap      = 8f;
    private const float MaxGap      = 18f;

    private float _nextSpawnX = 15f;
    private Camera _cam;
    private List<GameObject> _active = new List<GameObject>();

    private void Start() => _cam = Camera.main;

    private void Update()
    {
        if (GameManager.Instance?.State != GameState.Playing) return;
        if (_cam == null) return;

        float camRight = _cam.transform.position.x + _cam.orthographicSize * _cam.aspect + 2f;
        float camLeft  = _cam.transform.position.x - _cam.orthographicSize * _cam.aspect - 2f;

        _active.RemoveAll(g => g == null);
        foreach (var c in _active)
            if (c != null && c.transform.position.x < camLeft)
                Destroy(c);
        _active.RemoveAll(g => g == null);

        if (camRight > _nextSpawnX)
        {
            SpawnCluster();
            _nextSpawnX += Random.Range(MinGap, MaxGap);
        }
    }

    private void SpawnCluster()
    {
        int count = Random.Range(2, 6);
        float sx  = _nextSpawnX + 3f;

        for (int i = 0; i < count; i++)
        {
            var go          = new GameObject("Coin");
            go.tag          = "Coin";
            go.transform.position = new Vector3(sx + i * CoinSpacing, GroundBuilder.Y + CoinY, 0f);

            var col        = go.AddComponent<CircleCollider2D>();
            col.radius     = 0.4f;
            col.isTrigger  = true;

            var sr          = go.AddComponent<SpriteRenderer>();
            sr.sprite       = MakeCircle();
            sr.color        = new Color(1f, 0.85f, 0.1f);
            sr.sortingOrder = 6;

            var pickup = go.AddComponent<CoinPickup>();

            // Pulse animation handled in CoinPickup
            _active.Add(go);
        }
    }

    private static Sprite MakeCircle()
    {
        int res = 32;
        var tex = new Texture2D(res, res);
        tex.filterMode = FilterMode.Bilinear;
        float cx = res * 0.5f, r = res * 0.5f - 2f;
        for (int x = 0; x < res; x++)
        for (int y = 0; y < res; y++)
        {
            float dx = x - cx, dy = y - cx;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float alpha = dist < r ? 1f : (dist < r + 2f ? (r + 2f - dist) / 2f : 0f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }
}
