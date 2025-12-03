using UnityEngine;

public class GrowthPointGimmick : MonoBehaviour
{
    // ギミックの種類
    public enum GimmickType { Turret, IvyShield, FallingRock }
    public GimmickType type;

    public int damageAmount = 1;

    // 魔法が当たって成長した時に呼ばれる想定
    public void OnMagicHit()
    {
        // 成長アニメーションや有効化処理
        Debug.Log("成長点が活性化しました");

        if (type == GimmickType.Turret)
        {
            // 砲台なら即座に弾を発射してボスを狙う処理などを呼ぶ
            // ここでは簡易的に「ボスを探してダメージ」とします
            AttackBoss();
        }
    }

    // 物理的な衝突（ツタシールドにボスが突っ込んできた時など）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ボスとぶつかったら
        if (collision.gameObject.TryGetComponent<BossController>(out BossController boss))
        {
            // ツタシールドの場合のみ、接触でダメージ
            if (type == GimmickType.IvyShield)
            {
                boss.TakeDamage(damageAmount);
                Debug.Log("ツタシールドでカウンター成功！");
                // 必要ならここでシールドを壊す
                Destroy(gameObject);
            }
        }
    }

    // 砲台などが遠隔で攻撃する場合の処理
    void AttackBoss()
    {
        // シーン上のボスを探してダメージを与える（簡易実装）
        BossController boss = FindFirstObjectByType<BossController>();
        if (boss != null)
        {
            boss.TakeDamage(damageAmount);
        }
    }
}