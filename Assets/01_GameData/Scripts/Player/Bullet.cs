using UnityEngine;

public class Bullet : MonoBehaviour
{
    public System.Action<Bullet> OnDestroyed;

    void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        if (viewportPos.x < 0f || viewportPos.x > 1f ||
            viewportPos.y < 0f || viewportPos.y > 1f)
        {
            Destroy(gameObject);
        }
    }

}
