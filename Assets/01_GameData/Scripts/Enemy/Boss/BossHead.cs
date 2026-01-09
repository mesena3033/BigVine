using UnityEngine;

public class BossHead : MonoBehaviour
{
    [Header("追いかける対象")]
    [SerializeField] private Transform targetBody; // 体のTransform

    [Header("追従の滑らかさ")]
    [SerializeField] private float followSpeed = 8f;   // この値が大きいほど、機敏に追いかける

    private Vector3 initialOffset; // 体と頭の初期位置の差分

    private bool isFlippedX = false; // 現在、左右反転しているか
    private bool isFlippedY = false; // 現在、上下反転しているか


    void Awake()
    {
        if (targetBody != null)
        {
            // 親オブジェクト(Boss)からの相対位置(localPosition)で初期オフセットを計算
            // これにより、Bossオブジェクト自体の位置に影響されずに、体と頭の相対関係を保てる
            initialOffset = transform.localPosition - targetBody.localPosition;
        }
        else
        {
            Debug.LogError("BossHead: targetBodyがインスペクターで設定されていません！", this.gameObject);
        }
    }

    // BossControllerから上下の向きの指示を受け取るための公開メソッド
    public void SetVerticalFlip(bool flippedY)
    {
        isFlippedY = flippedY;
    }

    // BossControllerから向きの指示を受け取るための公開メソッド
    public void SetFlip(bool flipped)
    {
        isFlippedX = flipped;
    }

    // ワールド座標(position)ではなく、ローカル座標(localPosition)で計算するように変更します
    void LateUpdate()
    {
        if (targetBody != null)
        {
            // 現在の向きに応じたオフセットを計算 (ローカル座標系)
            Vector3 currentOffset = initialOffset;

            // 左右反転の処理
            if (isFlippedX)
            {
                currentOffset.x = -initialOffset.x;
            }

            // 上下反転の処理
            if (isFlippedY)
            {
                currentOffset.y = -initialOffset.y;
            }

            // 目標地点 = 体の現在のローカル位置 + 計算した差分
            Vector3 targetLocalPosition = targetBody.localPosition + currentOffset;

            // 現在地から目標地点へ、ローカル座標基準で滑らかに移動します
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * followSpeed);
        }
    }
}
