using UnityEngine;

public class BossStageCameraController : MonoBehaviour
{
    [Header("追従対象")]
    [Tooltip("カメラが追従するプレイヤーのTransform")]
    [SerializeField] private Transform playerTransform;

    [Header("カメラ挙動設定")]
    [Tooltip("カメラの挙動が切り替わるプレイヤーのY座標の閾値")]
    [SerializeField] private float yThreshold = 12.0f;

    [Tooltip("プレイヤーが高所にいる時のカメラの目標Y座標")]
    [SerializeField] private float highAltitudeCameraY = 12.0f;

    [Tooltip("プレイヤーが高所にいる時に、見える範囲を何倍にするか")]
    [SerializeField] private float sizeMultiplier = 2.0f;
    
    [Tooltip("カメラの位置やサイズが変化する際の滑らかさ（値が小さいほど速く追従）")]
    [SerializeField] private float smoothTime = 0.5f;


    // --- 内部変数 ---
    private Camera mainCamera;
    private Vector3 initialCameraPosition;
    private float initialOrthographicSize;

    // SmoothDampで使う速度変数（毎回0で初期化されるのを防ぐ）
    private Vector3 positionVelocity = Vector3.zero;
    private float sizeVelocity = 0f;

    void Start()
    {
        mainCamera = GetComponent<Camera>();

        // カメラがOrthographic（2D用）でない場合は警告を出す
        if (!mainCamera.orthographic)
        {
            Debug.LogWarning("このカメラスクリプトは、Orthographic（平行投影）カメラでの使用を想定しています。");
        }
        
        // プレイヤーが設定されていなければ、自動で"Player"タグを探す
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player Transformが設定されておらず、'Player'タグのオブジェクトも見つかりませんでした。");
                this.enabled = false; // スクリプトを無効化
                return;
            }
        }
        
        // ゲーム開始時のカメラの初期位置とサイズを記憶しておく
        initialCameraPosition = transform.position;
        initialOrthographicSize = mainCamera.orthographicSize;
    }

    // 全てのUpdate処理が終わった後にカメラを動かすため、LateUpdateを使用
    void LateUpdate()
    {
        // プレイヤーがいない場合は処理を中断
        if (playerTransform == null) return;
        
        // 1. 目標となるカメラの位置とサイズを決定する
        Vector3 targetPosition;
        float targetSize;
        
        if (playerTransform.position.y > yThreshold)
        {
            // 【12より大きい場合】
            // 目標位置：XとZは初期値のまま、Yだけ指定の高さにする
            targetPosition = new Vector3(initialCameraPosition.x, highAltitudeCameraY, initialCameraPosition.z);
            // 目標サイズ：初期サイズの2倍（sizeMultiplier倍）にする
            targetSize = initialOrthographicSize * sizeMultiplier;
        }
        else
        {
            // 【12以下の場合】
            // 目標位置：ゲーム開始時のカメラ位置に戻す
            targetPosition = initialCameraPosition;
            // 目標サイズ：ゲーム開始時のサイズに戻す
            targetSize = initialOrthographicSize;
        }

        // 2. 現在の位置/サイズを、目標値に向かって"滑らかに"変化させる
        // Vector3.SmoothDamp: 急に動かず、ぬるっと目標に近づく動きを実装できる
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, smoothTime);
        
        // Mathf.SmoothDamp: 数値（float）を滑らかに変化させる
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, targetSize, ref sizeVelocity, smoothTime);
    }
}
