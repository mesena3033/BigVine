using UnityEngine;

public class Pet : MonoBehaviour
{
    public Transform player;
    public Vector2 offset = new Vector2(1.5f, 0.5f);
    public float smoothTime = 0.2f; // 遅れ具合（大きいほどゆっくり）
    public float floatAmplitude = 0.05f;
    public float floatFrequency = 1.5f;

    private Vector2 velocity = Vector2.zero;

    void Update()
    {
        // プレイヤーの向きに応じて後ろに配置
        float dir = Mathf.Sign(player.localScale.x);
        Vector2 targetPos = (Vector2)player.position + new Vector2(offset.x * -dir, offset.y);

        // 遅れて追従（慣性あり）
        Vector2 newPos = Vector2.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // ふわふわ浮遊
        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        newPos.y += floatOffset;

        transform.position = newPos;
    }

}
