using UnityEngine;

/// <summary>
/// Attached to every body-part child so collision events bubble up to the
/// StickmanController on the root GameObject.
/// Also kills the player on obstacle hit.
/// </summary>
public class BodyPartSensor : MonoBehaviour
{
    private StickmanController _ctrl;

    private void Awake()
    {
        _ctrl = GetComponentInParent<StickmanController>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        _ctrl?.OnChildCollisionEnter(col.gameObject);

        if (col.gameObject.CompareTag("Obstacle"))
        {
            // Only count as death if hit is from the front (relative velocity)
            float relVX = col.relativeVelocity.x;
            if (relVX > 2f)
                _ctrl?.Die();
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        _ctrl?.OnChildCollisionExit(col.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Coin"))
        {
            col.GetComponent<CoinPickup>()?.Collect();
        }
    }
}
