using UnityEngine;

public class CameraFollowDummy : MonoBehaviour
{
    public Transform player;
    public float fixedY = 0f;      // 基本となる高さ
    public float startOffset = 5f; // 開始時のXオフセット
    public float fixedZ = -10f;

    // カメラが移動できる右の限界座標
    [Header("Camera Bounds")]
    public float maxCameraX = 100f; 

    [Header("Smooth Settings")]
    public float smoothSpeed = 5f; // Y軸移動の滑らかさ

    private float minCameraX;
    private float currentOffsetY = 0f; // 現在適用中のYオフセット量

    private Camera cam; // カメラコンポーネントを保持する変数
    private float initialOrthographicSize; // 元のカメラサイズを保存する変数
    private float targetOrthographicSize; // 目標のカメラサイズ

    void Start()
    {
        if (player == null) return;
        minCameraX = player.position.x + startOffset;

        // 初期位置設定
        transform.position = new Vector3(minCameraX, fixedY, fixedZ);

        cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            // カメラの初期サイズを保存しておく
            initialOrthographicSize = cam.orthographicSize;
            targetOrthographicSize = initialOrthographicSize;
        }
        else
        {
            Debug.LogError("Orthographic設定のCameraコンポーネントが見つかりません。", this);
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 1. まずプレイヤーを追いかける目標座標を計算し、それが左の限界(minCameraX)より小さくならないようにする
        float desiredX = Mathf.Max(player.position.x, minCameraX);

        // 2. 上記で計算した目標座標が、さらに右の限界(maxCameraX)を超えないように制限する
        float targetX = Mathf.Min(desiredX, maxCameraX);

        // --- Y軸の計算 (新規追加: 滑らかに移動) ---
        // 目標となるY座標 = 基本の高さ(fixedY) + 追加オフセット(currentOffsetY)
        float targetY = fixedY + currentOffsetY;

        // 現在のY座標から目標のY座標へ少しずつ近づける (Lerp)
        float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * smoothSpeed);

        // 座標更新
        transform.position = new Vector3(targetX, newY, fixedZ);

        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, Time.deltaTime * smoothSpeed);
        }
    }

    // 外部（トリガーなど）から呼ばれる関数
    // offset: 上にずらす量（0なら元の高さに戻る）
    public void SetVerticalOffset(float offset)
    {
        currentOffsetY = offset;
    }

    public void SetOrthographicSize(float newSize)
    {
        // 目標サイズをセットする（実際の変更はLateUpdateで滑らかに行われる）
        targetOrthographicSize = newSize;
    }

    public void ResetCameraSettings()
    {
        SetVerticalOffset(0f);
        SetOrthographicSize(initialOrthographicSize);
    }
}