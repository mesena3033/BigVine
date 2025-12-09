using UnityEngine;

public class BossBody : MonoBehaviour
{
    [Header("揺れの大きさ")]
    [SerializeField] private float amplitude = 0.2f; // 上下に動く幅

    [Header("揺れの速さ")]
    [SerializeField] private float frequency = 1f;   // 1秒間に何往復するか

    private Vector3 initialLocalPosition;

    void Start()
    {
        // 動き始める前の、親オブジェクトからの相対位置を保存しておく
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // sin波を使って、Y座標だけを周期的に動かす
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;

        // 保存しておいた初期位置に、計算したYの動きを加える
        transform.localPosition = new Vector3(
            initialLocalPosition.x,
            initialLocalPosition.y + yOffset,
            initialLocalPosition.z
        );
    }
}
