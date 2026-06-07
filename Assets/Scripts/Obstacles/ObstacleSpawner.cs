using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    private enum ObstacleType { LowBox, TallBox, FloatingPlatform, WideLow }

    private float _spawnX;
    private float _minGap = 6f;
    private float _maxGap = 14f;
    private float _nextSpawnX;

    private Camera _cam;
    private List<GameObject> _pool = new List<GameObject>();

    private void Start()
    {
        _cam = Camera.main;
        _nextSpawnX = 20f;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (_cam == null) return;

        float camRight = _cam.transform.position.x + _cam.orthographicSize * _cam.aspect + 2f;
        float camLeft  = _cam.transform.position.x - _cam.orthographicSize * _cam.aspect - 2f;

        // Recycle off-screen obstacles
        for (int i = _pool.Count - 1; i >= 0; i--)
        {
            if (_pool[i] == null) { _pool.RemoveAt(i); continue; }
            if (_pool[i].transform.position.x < camLeft)
                Destroy(_pool[i]);
        }
        _pool.RemoveAll(g => g == null);

        // Spawn new
        if (camRight > _nextSpawnX)
        {
            SpawnNext();
            _nextSpawnX += Random.Range(_minGap, _maxGap);
            _minGap = Mathf.Max(3.5f, _minGap - 0.02f);  // gap shrinks over time
            _maxGap = Mathf.Max(7f,   _maxGap - 0.03f);
        }
    }

    private void SpawnNext()
    {
        int roll = Random.Range(0, 10);
        ObstacleType type;

        if (roll < 3)       type = ObstacleType.LowBox;
        else if (roll < 6)  type = ObstacleType.TallBox;
        else if (roll < 8)  type = ObstacleType.WideLow;
        else                type = ObstacleType.FloatingPlatform;

        float spawnX = _nextSpawnX + 5f;
        GameObject go = null;

        switch (type)
        {
            case ObstacleType.LowBox:
                go = SpawnBox(spawnX, GroundBuilder.Y, 1.2f, 1.0f);
                break;
            case ObstacleType.TallBox:
                go = SpawnBox(spawnX, GroundBuilder.Y, 0.8f, 2.2f);
                break;
            case ObstacleType.WideLow:
                go = SpawnBox(spawnX, GroundBuilder.Y, 2.8f, 0.7f);
                break;
            case ObstacleType.FloatingPlatform:
                go = SpawnBox(spawnX, GroundBuilder.Y + 2.5f, 2.5f, 0.25f, isTrigger: false);
                break;
        }

        if (go != null) _pool.Add(go);
    }

    private GameObject SpawnBox(float x, float groundY, float w, float h,
                                 bool isTrigger = false)
    {
        var go = new GameObject("Obstacle");
        go.tag = "Obstacle";

        go.transform.position = new Vector3(x, groundY + h * 0.5f, 0f);

        var rb       = go.AddComponent<Rigidbody2D>();
        rb.bodyType  = RigidbodyType2D.Static;

        var col  = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeSquare();
        sr.color        = new Color(0.9f, 0.2f, 0.3f, 0.85f);
        sr.sortingOrder = 3;

        go.transform.localScale = new Vector3(w, h, 1f);

        // Neon edge glow (child sprite)
        var edge = new GameObject("EdgeGlow");
        edge.transform.parent        = go.transform;
        edge.transform.localPosition = Vector3.zero;
        edge.transform.localScale    = Vector3.one;
        var edgeSr          = edge.AddComponent<SpriteRenderer>();
        edgeSr.sprite       = MakeSquare();
        edgeSr.color        = new Color(1f, 0.3f, 0.4f, 0.3f);
        edgeSr.sortingOrder = 4;

        return go;
    }

    private static Sprite MakeSquare()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
