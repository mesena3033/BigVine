using UnityEngine;

public class Pet : MonoBehaviour
{
    public Transform player;
    public Vector2 offset = new Vector2(1f, 0.1f);
    public float smoothTime = 0.05f;

    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        float dir = Mathf.Sign(player.localScale.x);
        Vector3 targetPos = player.position + new Vector3(offset.x * dir, offset.y, 0f);

        // 慣性付きで追従（Vector3版を使用）
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        transform.position = newPos;
    }
}