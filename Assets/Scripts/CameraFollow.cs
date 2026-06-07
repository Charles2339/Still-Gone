using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    private float _smoothX = 8f;
    private float _smoothY = 4f;
    private float _offsetX = 2f;     // keep runner left of centre
    private float _offsetY = 3f;
    private float _fixedZ  = -10f;

    private void LateUpdate()
    {
        if (target == null) return;

        var desired = new Vector3(
            target.position.x + _offsetX,
            _offsetY,
            _fixedZ
        );

        var cur = transform.position;
        transform.position = new Vector3(
            Mathf.Lerp(cur.x, desired.x, _smoothX * Time.deltaTime),
            Mathf.Lerp(cur.y, desired.y, _smoothY * Time.deltaTime),
            _fixedZ
        );
    }
}
