using UnityEngine;

/// <summary>
/// Constructs the active-ragdoll stickman entirely from code.
/// No prefabs or scene references required.
///
/// Body graph:
///   Torso ← Head
///   Torso ← LUpperArm ← LForearm
///   Torso ← RUpperArm ← RForearm
///   Torso ← LThigh ← LShin
///   Torso ← RThigh ← RShin
/// </summary>
public static class StickmanBuilder
{
    // Body proportions (metres)
    private const float HeadRadius = 0.28f;
    private const float TorsoH     = 0.80f;
    private const float TorsoW     = 0.22f;
    private const float ArmSegH    = 0.36f;
    private const float ArmSegW    = 0.10f;
    private const float LegSegH    = 0.44f;
    private const float LegSegW    = 0.12f;

    // Masses
    private const float TorsoMass  = 3f;
    private const float HeadMass   = 0.8f;
    private const float ArmMass    = 0.3f;
    private const float LegMass    = 0.6f;

    public static GameObject Build(Vector2 spawnPos)
    {
        var root = new GameObject("Stickman");
        root.tag = "Player";

        // ── Controller (needed before body parts so BuildArm/BuildLeg can register) ──
        var ctrl  = root.AddComponent<StickmanController>();
        var input = root.AddComponent<PlayerInput>();

        // ── Torso ─────────────────────────────────────────────────────────────
        var torso = MakePart("Torso", root.transform,
                             spawnPos + new Vector2(0f, TorsoH * 0.5f),
                             new Vector2(TorsoW, TorsoH), TorsoMass, 0.2f);
        ctrl.torso = torso;

        // ── Head ──────────────────────────────────────────────────────────────
        var head = MakeHead("Head", root.transform,
                            spawnPos + new Vector2(0f, TorsoH + HeadRadius), HeadRadius, HeadMass);
        ConnectHinge(head, torso,
                     new Vector2(0f, -HeadRadius),
                     new Vector2(0f,  TorsoH * 0.5f),
                     new JointAngleLimits2D { min = -30f, max = 30f });

        // ── Arms ──────────────────────────────────────────────────────────────
        float shoulderY = TorsoH * 0.35f;
        float armOffX   = TorsoW * 0.5f + ArmSegW * 0.5f;

        BuildArm(ctrl, root, torso, spawnPos, left: true,  shoulderX: -armOffX, shoulderY);
        BuildArm(ctrl, root, torso, spawnPos, left: false, shoulderX:  armOffX, shoulderY);

        // ── Legs ──────────────────────────────────────────────────────────────
        float hipX = TorsoW * 0.25f;
        float hipY = -TorsoH * 0.45f;

        BuildLeg(ctrl, root, torso, spawnPos, left: true,  hipX: -hipX, hipY);
        BuildLeg(ctrl, root, torso, spawnPos, left: false, hipX:  hipX, hipY);

        // ── Ignore self-collisions ─────────────────────────────────────────────
        IgnoreInternalCollisions(root);

        return root;
    }

    // ── Sub-builders ─────────────────────────────────────────────────────────

    private static void BuildArm(StickmanController ctrl, GameObject root, Rigidbody2D torso,
                                  Vector2 spawnPos, bool left, float shoulderX, float shoulderY)
    {
        string side = left ? "L" : "R";
        float  dir  = left ? -1f : 1f;

        var upperArm = MakePart($"{side}UpperArm", root.transform,
                                spawnPos + new Vector2(shoulderX + dir * ArmSegH * 0.5f, shoulderY),
                                new Vector2(ArmSegH, ArmSegW), ArmMass, 0.05f);
        ConnectHinge(upperArm, torso,
                     new Vector2(-dir * ArmSegH * 0.5f, 0f),
                     new Vector2(shoulderX, shoulderY),
                     new JointAngleLimits2D { min = -80f, max = 80f });

        var forearm = MakePart($"{side}Forearm", root.transform,
                               spawnPos + new Vector2(shoulderX + dir * (ArmSegH + ArmSegH * 0.5f),
                                                      shoulderY - ArmSegH * 0.3f),
                               new Vector2(ArmSegH, ArmSegW), ArmMass, 0.05f);
        ConnectHinge(forearm, upperArm,
                     new Vector2(-dir * ArmSegH * 0.5f, 0f),
                     new Vector2(dir * ArmSegH * 0.5f, 0f),
                     new JointAngleLimits2D { min = -100f, max = 10f });

        if (left) { ctrl.lUpperArm = upperArm; ctrl.lForearm = forearm; }
        else       { ctrl.rUpperArm = upperArm; ctrl.rForearm = forearm; }
    }

    private static void BuildLeg(StickmanController ctrl, GameObject root, Rigidbody2D torso,
                                  Vector2 spawnPos, bool left, float hipX, float hipY)
    {
        string side = left ? "L" : "R";

        var thigh = MakePart($"{side}Thigh", root.transform,
                             spawnPos + new Vector2(hipX, hipY - LegSegH * 0.5f),
                             new Vector2(LegSegW, LegSegH), LegMass, 0.05f);
        ConnectHinge(thigh, torso,
                     new Vector2(0f,  LegSegH * 0.5f),
                     new Vector2(hipX, hipY),
                     new JointAngleLimits2D { min = -60f, max = 80f });

        var shin = MakePart($"{side}Shin", root.transform,
                            spawnPos + new Vector2(hipX, hipY - LegSegH - LegSegH * 0.5f),
                            new Vector2(LegSegW, LegSegH), LegMass, 0.05f);
        ConnectHinge(shin, thigh,
                     new Vector2(0f,  LegSegH * 0.5f),
                     new Vector2(0f, -LegSegH * 0.5f),
                     new JointAngleLimits2D { min = -5f, max = 120f });

        if (left) { ctrl.lThigh = thigh; ctrl.lShin = shin; }
        else       { ctrl.rThigh = thigh; ctrl.rShin = shin; }
    }

    // ── Part factories ────────────────────────────────────────────────────────

    private static Rigidbody2D MakePart(string name, Transform parent, Vector2 pos,
                                         Vector2 size, float mass, float drag)
    {
        var go = new GameObject(name);
        go.transform.parent   = parent;
        go.transform.position = pos;
        go.tag                = "Player";

        var rb           = go.AddComponent<Rigidbody2D>();
        rb.mass          = mass;
        rb.drag          = drag;          // Unity 2022 API (not linearDamping)
        rb.angularDrag   = drag * 5f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col  = go.AddComponent<BoxCollider2D>();
        col.size = size;

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeSquareSprite();
        sr.color        = new Color(0.85f, 0.92f, 1.0f);
        sr.sortingOrder = 5;

        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        // Bubble collisions up to StickmanController
        go.AddComponent<BodyPartSensor>();

        return rb;
    }

    private static Rigidbody2D MakeHead(string name, Transform parent, Vector2 pos,
                                         float radius, float mass)
    {
        var go = new GameObject(name);
        go.transform.parent   = parent;
        go.transform.position = pos;
        go.tag                = "Player";

        var rb           = go.AddComponent<Rigidbody2D>();
        rb.mass          = mass;
        rb.drag          = 0.3f;
        rb.angularDrag   = 2f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col    = go.AddComponent<CircleCollider2D>();
        col.radius = radius;

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeCircleSprite(radius);
        sr.color        = new Color(0.85f, 0.92f, 1.0f);
        sr.sortingOrder = 5;

        go.transform.localScale = Vector3.one;

        go.AddComponent<BodyPartSensor>();

        return rb;
    }

    private static void ConnectHinge(Rigidbody2D body, Rigidbody2D connected,
                                      Vector2 anchorLocal, Vector2 connectedAnchor,
                                      JointAngleLimits2D limits)
    {
        var joint              = body.gameObject.AddComponent<HingeJoint2D>();
        joint.connectedBody    = connected;
        joint.anchor           = anchorLocal;
        joint.connectedAnchor  = connectedAnchor;
        joint.useLimits        = true;
        joint.limits           = limits;
        joint.enableCollision  = false;
    }

    private static void IgnoreInternalCollisions(GameObject root)
    {
        var cols = root.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < cols.Length; i++)
        for (int j = i + 1; j < cols.Length; j++)
            Physics2D.IgnoreCollision(cols[i], cols[j]);
    }

    // ── Sprite utilities ─────────────────────────────────────────────────────

    private static Sprite MakeSquareSprite()
    {
        var tex = new Texture2D(2, 2);
        tex.filterMode = FilterMode.Bilinear;
        var c = Color.white;
        tex.SetPixel(0, 0, c); tex.SetPixel(1, 0, c);
        tex.SetPixel(0, 1, c); tex.SetPixel(1, 1, c);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
    }

    private static Sprite MakeCircleSprite(float radius)
    {
        int   res = 64;
        var   tex = new Texture2D(res, res);
        tex.filterMode = FilterMode.Bilinear;
        float cx = res * 0.5f, r = res * 0.5f - 1f;
        for (int x = 0; x < res; x++)
        for (int y = 0; y < res; y++)
        {
            float dx = x - cx, dy = y - cx;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float a = dist <= r ? 1f : (dist <= r + 2f ? (r + 2f - dist) * 0.5f : 0f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res / (radius * 2f));
    }
}
