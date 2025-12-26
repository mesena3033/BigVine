using UnityEngine;

public class BossHead : MonoBehaviour
{
    [Header("追いかける対象")]
    [SerializeField] private Transform targetBody; // 体のTransform

    [Header("追従の滑らかさ")]
    [SerializeField] private float followSpeed = 8f;   // この値が大きいほど、機敏に追いかける

    private bool isFlipped = false; // 現在反転しているか(右を向いているか)

    private Vector3 initialOffset; // 体と頭の初期位置の差分

    private bool isFlippedX = false; // 現在、左右反転しているか
    private bool isFlippedY = false; // 現在、上下反転しているか


    void Start()
    {
        if (targetBody != null)
        {
            // 最初に、体と頭がどれだけ離れているかを計算して保存しておく
            initialOffset = transform.position - targetBody.position;
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
        isFlipped = flipped;
    }

    void LateUpdate()
    {
        if (targetBody != null)
        {
            // 現在の向きに応じたオフセットを計算
            Vector3 currentOffset = initialOffset;

            // 左右反転の処理
            if (isFlippedX)
            {
                currentOffset.x = -initialOffset.x;
            }
            // 上下反転の処理
            if (isFlippedY)
            {
                currentOffset.y = -initialOffset.y; // Yのオフセットを反転させる
            }

            // 目標地点 = 体の現在の位置 + 計算した差分
            Vector3 targetPosition = targetBody.position + currentOffset;

            // 現在地から目標地点へ、滑らかに移動する
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
    }
}
