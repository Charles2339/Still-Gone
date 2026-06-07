using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private bool _collected;
    private float _bobPhase;
    private Vector3 _basePos;

    private void Start() => _basePos = transform.position;

    private void Update()
    {
        _bobPhase += Time.deltaTime * 3f;
        transform.position = _basePos + new Vector3(0f, Mathf.Sin(_bobPhase) * 0.12f, 0f);

        // Spin
        transform.eulerAngles += new Vector3(0f, 0f, Time.deltaTime * 90f);
    }

    public void Collect()
    {
        if (_collected) return;
        _collected = true;
        GameManager.Instance?.AddCoin();
        Destroy(gameObject);
    }
}
