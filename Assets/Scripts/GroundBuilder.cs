using UnityEngine;

/// <summary>
/// Builds the static infinite-looking ground strip.
/// Uses a wide BoxCollider2D and a SpriteRenderer strip.
/// </summary>
public class GroundBuilder : MonoBehaviour
{
    public static float Y { get; private set; } = 0f;   // world-Y of ground surface

    public void Build()
    {
        Y = 0f;
        gameObject.tag = "Ground";
        transform.position = new Vector3(0f, Y - 0.5f, 0f);

        // Visual strip
        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeSquareSprite();
        sr.color        = new Color(0.07f, 0.08f, 0.12f);
        sr.sortingOrder = -5;

        var scale = gameObject.transform.localScale;
        scale.x = 10000f;
        scale.y = 1f;
        gameObject.transform.localScale = scale;

        // Collider (triggers off)
        var bc = gameObject.AddComponent<BoxCollider2D>();
        bc.size   = new Vector2(1f, 1f);
        bc.isTrigger = false;

        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // Neon line at top
        var line = new GameObject("GroundLine");
        line.transform.parent = transform;
        var lineSr = line.AddComponent<SpriteRenderer>();
        lineSr.sprite       = MakeSquareSprite();
        lineSr.color        = new Color(0.2f, 1f, 0.6f, 0.6f);
        lineSr.sortingOrder = -4;
        line.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        line.transform.localScale    = new Vector3(1f, 0.04f, 1f);
    }

    private static Sprite MakeSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
