using UnityEngine;

/// <summary>
/// Spawns a simple tiling background built from solid-colour quads.
/// No textures needed — everything is procedural.
/// </summary>
public class BackgroundTiler : MonoBehaviour
{
    private const float TileW = 20f;
    private const int   PoolSize = 4;

    private Transform[] _tiles;
    private float        _camHalfW;

    public void Initialize()
    {
        _camHalfW = Camera.main != null ? Camera.main.orthographicSize * Camera.main.aspect : 10f;
        _tiles    = new Transform[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            var go = CreateTile(i * TileW - TileW);
            _tiles[i] = go.transform;
        }
    }

    private GameObject CreateTile(float x)
    {
        var go = new GameObject("BGTile");
        go.transform.parent = transform;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeSquareSprite();
        sr.color        = new Color(0.01f, 0.03f, 0.06f);
        sr.sortingOrder = -10;

        go.transform.position   = new Vector3(x, 0f, 5f);
        go.transform.localScale = new Vector3(TileW, 40f, 1f);
        return go;
    }

    private void Update()
    {
        if (Camera.main == null) return;
        float camX = Camera.main.transform.position.x;

        foreach (var t in _tiles)
        {
            if (t.position.x + TileW < camX - _camHalfW)
                t.position = new Vector3(t.position.x + TileW * PoolSize, t.position.y, t.position.z);
        }
    }

    private static Sprite MakeSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
