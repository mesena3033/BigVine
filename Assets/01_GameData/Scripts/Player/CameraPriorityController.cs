using UnityEngine;
using Unity.Cinemachine;

public class CameraPriorityController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CinemachineCamera normalCamera; // VCamMain を設定
    [SerializeField] private CinemachineCamera highAltitudeCamera; // VCamHigh を設定

    [Header("設定")]
    [SerializeField] private float yThreshold = 12.0f;

    private bool isForcedHighAltitude = false;

    // 外部（FallingRock）からこのコントローラーの動作を一時停止させるためのフラグ
    public bool IsPaused { get; set; } = false;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player").transform;
        }
    }

    void Update()
    {
        // ポーズフラグが立っていたら、プライオリティの自動更新をスキップする
        if (IsPaused)
        {
            return;
        }

        // 強制フラグがON、またはプレイヤーが閾値より高い位置にいるか
        if (isForcedHighAltitude || playerTransform.position.y > yThreshold)
        {
            // 高所カメラの優先度を上げる
            normalCamera.Priority.Value = 10;
            highAltitudeCamera.Priority.Value = 11;
        }
        else
        {
            // 通常カメラの優先度を上げる (元に戻す)
            normalCamera.Priority.Value = 11;
            highAltitudeCamera.Priority.Value = 10;
        }
    }

    // BossControllerから呼び出されるための公開メソッド
    public void ForceHighAltitudeView(bool force)
    {
        isForcedHighAltitude = force;
    }
}
