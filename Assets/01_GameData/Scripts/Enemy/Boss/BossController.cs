using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] private int maxHp = 10;
    private int currentHp;

    [Header("攻撃設定: 突進")]
    public float chargeSpeed = 10f;      // 秒速
    public float chargeDuration = 1.5f;  // 持続時間

    [Header("ステージ範囲制限")]
    public float minX = -8.0f; // 画面左端のX座標
    public float maxX = 8.0f;  // 画面右端のX座標

    [Header("参照")]
    [SerializeField] private Rigidbody2D rb; // 当たり判定用
    [SerializeField] private SpriteRenderer[] bodyPartsRenderers; // 色を変えるパーツ一覧

    private Transform playerTransform;
    private bool isDead = false;

    void Start()
    {
        currentHp = maxHp;

        // プレイヤー取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // 行動ループ開始
        StartCoroutine(BossBehaviorLoop());
    }

    // --- AI思考ルーチン ---
    IEnumerator BossBehaviorLoop()
    {
        while (!isDead)
        {
            // 1. 待機
            yield return new WaitForSeconds(2.0f);

            if (isDead) break;

            // プレイヤーがいれば攻撃開始
            if (playerTransform != null)
            {
                yield return StartCoroutine(ChargeAttack());
            }
            else
            {
                // プレイヤー再検索（死亡後のリトライなどを想定）
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player) playerTransform = player.transform;
            }
        }
    }

    // --- 攻撃アクション: 突進 ---
    IEnumerator ChargeAttack()
    {
        // 1. 方向決定
        float directionSign = Mathf.Sign(playerTransform.position.x - transform.position.x); // 右:1, 左:-1

        // 2. 見た目の向きを合わせる
        // ※右向きの画像がデフォルトなら (directionSign < 0) で反転
        // ※左向きの画像がデフォルトなら (directionSign > 0) で反転
        bool shouldFlip = (directionSign < 0);
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null) sr.flipX = shouldFlip;
        }

        // 3. 予備動作（0.5秒溜め）
        Debug.Log("ボス：突進構え");
        yield return new WaitForSeconds(0.5f);

        // 4. 移動処理 (Transform移動)
        Debug.Log("ボス：突進開始");
        float timer = 0f;

        while (timer < chargeDuration)
        {
            if (isDead) break;

            // 移動量計算
            float moveAmount = directionSign * chargeSpeed * Time.deltaTime;
            float newX = transform.position.x + moveAmount;

            // 画面外に出ないよう制限 (Clamp)
            newX = Mathf.Clamp(newX, minX, maxX);

            // 座標更新
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            timer += Time.deltaTime;
            yield return null; // 1フレーム待つ
        }

        // 5. 攻撃後の硬直
        Debug.Log("ボス：突進終了");
        yield return new WaitForSeconds(1.0f);
    }

    // --- ダメージ・死亡処理 ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHp -= damage;
        Debug.Log($"ボスHP: {currentHp}");

        // 被弾エフェクト
        StartCoroutine(FlashDamageEffect());

        if (currentHp <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashDamageEffect()
    {
        Color originalColor = Color.white;
        Color damageColor = new Color(0.5f, 0f, 0.5f, 1f); // 紫色

        foreach (var sr in bodyPartsRenderers) if (sr != null) sr.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        foreach (var sr in bodyPartsRenderers) if (sr != null) sr.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("ボス撃破！");
        // ここに爆発エフェクト生成などを追加
        Destroy(gameObject, 0.5f);
    }
}