using UnityEngine;
using Unity.Cinemachine; // Unity 6 / Cinemachine 3.x系

public class CameraDampingControl : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("プレイヤーのRigidbody2Dをセットしてください")]
    public Rigidbody2D playerRb;

    [Tooltip("上昇中・停止中のY追従遅延（今の設定値くらい：1～2）")]
    public float dampingUp = 1.0f;

    [Tooltip("落下中のY追従遅延（小さくするほど速い：0.1～0.3）")]
    public float dampingDown = 0.2f;

    // 内部変数
    private CinemachineCamera cmCamera;
    private CinemachinePositionComposer posComposer;

    void Start()
    {
        // 同じオブジェクトについているCinemachineコンポーネントを取得
        cmCamera = GetComponent<CinemachineCamera>();
        if (cmCamera != null)
        {
            posComposer = cmCamera.GetComponent<CinemachinePositionComposer>();
        }
    }

    void Update()
    {
        // 必要なものが揃っていなければ何もしない
        if (playerRb == null || posComposer == null) return;

        // プレイヤーのY速度をチェック
        // Unity 6 では velocity ではなく linearVelocity プロパティ推奨の場合があります
        // エラーが出る場合は .linearVelocity を .velocity に書き換えてください
        float yVelocity = playerRb.linearVelocity.y;

        // 落下中（速度がマイナス）なら、Dampingを小さくして素早く追従
        if (yVelocity < -0.1f)
        {
            posComposer.Damping.y = dampingDown;
        }
        else
        {
            // 上昇中や停止中は、Dampingを大きくして画面酔いを防ぐ
            posComposer.Damping.y = dampingUp;
        }
    }
}