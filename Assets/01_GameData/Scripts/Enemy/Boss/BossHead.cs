using UnityEngine;

public class BossHead : MonoBehaviour
{
    [Header("追いかける対象")]
    [SerializeField] private Transform targetBody; // 体のTransform

    [Header("追従の滑らかさ")]
    [SerializeField] private float followSpeed = 8f;   // この値が大きいほど、機敏に追いかける

    private Vector3 offset; // 体と頭の初期位置の差分

    void Start()
    {
        if (targetBody != null)
        {
            // 最初に、体と頭がどれだけ離れているかを計算して保存しておく
            offset = transform.position - targetBody.position;
        }
    }

    void LateUpdate()
    {
        if (targetBody != null)
        {
            // 目標地点 = 体の現在の位置 + 保存しておいた差分
            Vector3 targetPosition = targetBody.position + offset;

            // 現在地から目標地点へ、滑らかに移動する
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
    }
}
