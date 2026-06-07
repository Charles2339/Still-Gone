using UnityEngine;

/// <summary>
/// Converts touch / keyboard input into StickmanController calls.
/// Tap = jump (double-tap = double-jump).
/// Swipe down = slide.
/// </summary>
[RequireComponent(typeof(StickmanController))]
public class PlayerInput : MonoBehaviour
{
    private StickmanController _ctrl;

    private Vector2 _touchStart;
    private float   _swipeThreshold = 100f;  // pixels

    private void Awake() => _ctrl = GetComponent<StickmanController>();

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        // ── Keyboard (editor / desktop) ────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            _ctrl.Jump();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            _ctrl.Slide();

        // ── Touch ──────────────────────────────────────────────────────────
        if (Input.touchCount == 0) return;

        var touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                _touchStart = touch.position;
                break;

            case TouchPhase.Ended:
                var delta = touch.position - _touchStart;
                if (Mathf.Abs(delta.y) > _swipeThreshold && delta.y < 0)
                    _ctrl.Slide();
                else
                    _ctrl.Jump();
                break;
        }
    }
}
