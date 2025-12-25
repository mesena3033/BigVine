using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5.0f;
    [SerializeField] private int reflectDamage = 2; // 跳ね返した時のダメージ

    private bool isReflected = false; // 跳ね返された状態フラグ
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // --- 1. ツタシールドに当たった場合 (反射) ---
        if (!isReflected && other.CompareTag("CounterShield"))
        {
            ReflectToBoss();
            return; // ここで処理を終えて、下の破壊処理に行かないようにする
        }

        // --- 2. ボスに当たった場合 (反射中のみ) ---
        if (isReflected)
        {
            // 親オブジェクトにBossControllerがあるか探す
            BossController boss = other.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(reflectDamage);
                Debug.Log("溶解液カウンターヒット！");
                Destroy(gameObject); // 役目を終えて消滅
                return;
            }
        }

        // --- 3. プレイヤーや地面に当たった場合 (通常時) ---
        if (isReflected && (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player")))
        {
            return; // スルー
        }

        // 通常時はプレイヤーにダメージ (PlayerHP側で処理されるが念のため消す)
        if (!isReflected && (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player")))
        {
            Destroy(gameObject);
        }
        // 地面(コメントアウトしている場合は貫通)
        //else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    Destroy(gameObject);
        //}
    }

    // --- ボスに向かって跳ね返す計算 ---
    void ReflectToBoss()
    {
        // ボスを探す
        BossController boss = FindFirstObjectByType<BossController>(); // Unity6対応記述
        if (boss == null) return;

        isReflected = true;

        // 色を変えて「跳ね返った感」を出す
        GetComponent<SpriteRenderer>().color = Color.yellow;

        // 速度リセット
        rb.linearVelocity = Vector2.zero;

        // --- 放物線計算 ---
        Vector3 startPos = transform.position;
        Vector3 targetPos = boss.transform.position;

        // 到達時間を指定（距離に応じて調整してもいいが、固定でもそれっぽくなる）
        float flightTime = 1.0f;

        // 重力加速度 (Physics2D.gravity * gravityScale)
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        // X軸の速度: 距離 / 時間
        float vx = (targetPos.x - startPos.x) / flightTime;

        // Y軸の速度: (高さ差 + 0.5 * 重力 * 時間^2) / 時間
        // 公式: y = vyt - 0.5gt^2  =>  vy = (y + 0.5gt^2) / t
        float vy = (targetPos.y - startPos.y + 0.5f * gravity * flightTime * flightTime) / flightTime;

        // 速度適用
        rb.linearVelocity = new Vector2(vx, vy);

        // 向き調整 (進行方向に向ける)
        float angle = Mathf.Atan2(vy, vx) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Debug.Log("反射！");
    }
}