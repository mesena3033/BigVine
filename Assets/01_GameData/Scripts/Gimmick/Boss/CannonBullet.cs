using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    [SerializeField] private int damage = 1; // ボスへのダメージ量（少し強めに）
    [SerializeField] private float lifeTime = 3.0f;

    void Start()
    {
        Destroy(gameObject, lifeTime); // 外しても時間で消える
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        BossController boss = other.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
            Debug.Log("種爆弾ヒット！");

            // 爆発エフェクトなどを出すならここでInstantiate
            Destroy(gameObject);
        }
        // 地面に当たったら消える
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}