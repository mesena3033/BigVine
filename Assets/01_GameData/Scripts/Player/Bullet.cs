using UnityEngine;

public class Bullet : MonoBehaviour
{
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