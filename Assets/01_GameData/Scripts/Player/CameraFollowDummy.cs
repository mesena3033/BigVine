using UnityEngine;

public class CameraFollowDummy : MonoBehaviour
{
    public Transform player;
    public float fixedY = 0f;
    public float startOffset = 5f;

    // ★追加：Z座標も強制的に固定する変数
    public float fixedZ = -10f;

    private float minCameraX;

    void Start()
    {
        if (player == null) return;
        minCameraX = player.position.x + startOffset;

        // 強制的に fixedZ の位置へ
        transform.position = new Vector3(minCameraX, fixedY, fixedZ);
    }

    void LateUpdate()
    {
        if (player == null) return;

        //Debug.Log($"Player: {player.position.x} / CameraLimit: {minCameraX}");

        float targetX = Mathf.Max(player.position.x, minCameraX);

        // ★修正：transform.position.z ではなく fixedZ を使う
        transform.position = new Vector3(targetX, fixedY, fixedZ);
    }
}