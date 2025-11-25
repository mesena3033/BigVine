using System.Collections.Generic;
using UnityEngine;

public class BulletAbsorber : MonoBehaviour
{
    [Header("設定")]
    // 速度をどれくらいにするか（0.3なら元の30%の速度になる）
    [SerializeField, Range(0f, 1f)] private float _slowDownFactor = 0.3f;

    // エリアに入ってから消滅するまでの時間（秒）
    [SerializeField] private float _destroyDelay = 1.0f;

    /// <summary>
    /// Trigger（当たり判定）に何かが入った時に呼ばれる
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ぶつかった相手が「Bullet」コンポーネントを持っているか確認
        if (collision.TryGetComponent<Bullet>(out Bullet bullet))
        {
            // 1. 弾のRigidbodyを取得して速度を操作
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 速度をガクッと落とす
                rb.linearVelocity *= _slowDownFactor;
            }

            bullet.enabled = false;

            // 2. 指定時間後にGameObjectごと破壊する
            Destroy(collision.gameObject, _destroyDelay);
        }
    }
}