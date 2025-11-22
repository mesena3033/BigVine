using UnityEngine;

public class CameraHeightTrigger : MonoBehaviour
{
    [Header("Settings")]
    public float offsetAmount = 3.0f;   // カメラを上げる量
    public string playerTag = "Player"; // プレイヤーのタグ

    // 判定の基準となるY座標（このオブジェクトの中心より少し下にするなどの調整用）
    // 0ならこのオブジェクトの中心と同じ高さ
    public float resetThresholdOffset = 0f;

    private CameraFollowDummy cameraScript;
    private Transform playerTransform;
    private bool isMonitoring = false; // 監視中かどうかのフラグ

    void Start()
    {
        if (Camera.main != null)
        {
            cameraScript = Camera.main.GetComponent<CameraFollowDummy>();
        }
    }

    void Update()
    {
        // 監視モードでなければ何もしない
        if (!isMonitoring || playerTransform == null || cameraScript == null) return;

        // このオブジェクトのY座標 (threshold) を計算
        float thresholdY = transform.position.y + resetThresholdOffset;

        // プレイヤーのY座標が、基準(thresholdY)よりも「下」になったら
        if (playerTransform.position.y < thresholdY)
        {
            // カメラを元に戻す
            cameraScript.SetVerticalOffset(0f);
            // 監視終了
            isMonitoring = false;
            playerTransform = null;
        }
    }

    // プレイヤーが入ってきた時だけ反応する
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag) && cameraScript != null)
        {
            // プレイヤーの参照を保持
            playerTransform = collision.transform;

            // カメラを上にずらす
            cameraScript.SetVerticalOffset(offsetAmount);

            // 高さ監視をスタートする
            isMonitoring = true;
        }
    }
}