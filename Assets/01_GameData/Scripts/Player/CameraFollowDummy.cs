using UnityEngine;

public class CameraFollowDummy : MonoBehaviour
{
    public Transform player;
    public float fixedY = 0f;      // 基本となる高さ
    public float startOffset = 5f; // 開始時のXオフセット
    public float fixedZ = -10f;

    [Header("Smooth Settings")]
    public float smoothSpeed = 5f; // Y軸移動の滑らかさ

    private float minCameraX;
    private float currentOffsetY = 0f; // 現在適用中のYオフセット量

    void Start()
    {
        if (player == null) return;
        minCameraX = player.position.x + startOffset;

        // 初期位置設定
        transform.position = new Vector3(minCameraX, fixedY, fixedZ);
    }

    void LateUpdate()
    {
        if (player == null) return;

        // --- X軸の計算 (既存のロジック: 戻らないカメラ) ---
        float targetX = Mathf.Max(player.position.x, minCameraX);

        // --- Y軸の計算 (新規追加: 滑らかに移動) ---
        // 目標となるY座標 = 基本の高さ(fixedY) + 追加オフセット(currentOffsetY)
        float targetY = fixedY + currentOffsetY;

        // 現在のY座標から目標のY座標へ少しずつ近づける (Lerp)
        float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * smoothSpeed);

        // 座標更新
        transform.position = new Vector3(targetX, newY, fixedZ);
    }

    // 外部（トリガーなど）から呼ばれる関数
    // offset: 上にずらす量（0なら元の高さに戻る）
    public void SetVerticalOffset(float offset)
    {
        currentOffsetY = offset;
    }
}