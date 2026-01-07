using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerHP : MonoBehaviour
{
    public int MaxHP = 3;
    public int NowHP;
    public float InvincibleTime = 2f;
    private bool isInvincible = false;

    [Header("UI Settings")]
    public GameObject HeartPrefab;
    public Transform HeartParent;
    public SpriteRenderer sr;

    private List<GameObject> hearts = new List<GameObject>();

    public Transform RespawnPoint;

    //  レイヤーID(Player, Enemy)
    private int PlayerLayer;
    private int EnemyLayer;

    // 物理演算用
    [Header("Physics Settings")]
    public float KnockbackForce = 10f; // ノックバックの強さ
    private Rigidbody2D rb;

    [Header("Knockback Settings")]
    public float KnockbackTime = 0.2f; // ノックバックで操作不能になる時間
    private bool isKnockingBack = false; // ノックバック中かどうかのフラグ

    void Start()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

        rb = GetComponent<Rigidbody2D>();

        //  レイヤーID取得
        PlayerLayer = gameObject.layer;
        EnemyLayer = LayerMask.NameToLayer("Enemy");

        sr = GetComponentInChildren<SpriteRenderer>();
        NowHP = MaxHP;

        for (int i = 0; i < MaxHP; i++)
        {
            GameObject h = Instantiate(HeartPrefab, HeartParent);
            hearts.Add(h);
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        NowHP -= damage;
        if (NowHP <= 0)
        {
            NowHP = 0;
            Die();
        }

        UpdateUI();
        StartCoroutine(InvincibleCoroutine());
    }

    // PlayerActionスクリプトからノックバック状態を参照するためのメソッド
    public bool IsKnockingBack()
    {
        return isKnockingBack;
    }

    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        // ダメージ後に一定時間、Enemy / EnemyBullet との衝突を無効化して「すり抜け」仕様にする
        if (PlayerLayer >= 0)
        {
            if (EnemyLayer >= 0) Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, true);
        }

        float elapsed = 0f;

        while (elapsed < InvincibleTime)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.2f);

            sr.enabled = true;
            yield return new WaitForSeconds(0.2f);

            elapsed += 0.4f;
        }

        sr.enabled = true;
        isInvincible = false;

        // 一定時間経過後、衝突を元に戻す
        if (PlayerLayer >= 0)
        {
            if (EnemyLayer >= 0) Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, false);
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
        NowHP = MaxHP;
        UpdateUI();

        if (RespawnPoint != null)
        {
            transform.position = RespawnPoint.position;
        }
        else
        {
            Debug.LogWarning("ReSpawnPoint が設定されていません！");
        }
        SceneManager.LoadScene("GameOver");
    }

    private void UpdateUI()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].SetActive(i < NowHP);
        }
    }
    public void HealFull()
    {
        NowHP = MaxHP;
        UpdateUI();
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        // 無敵時間中、または既にノックバック中なら処理しない
        if (isInvincible || isKnockingBack) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // より正確な衝突検知用のコルーチンを呼び出す
            StartCoroutine(KnockbackFromCollisionCoroutine(collision));
            TakeDamage(1);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // 無敵時間中、または既にノックバック中なら処理しない
        if (isInvincible || isKnockingBack) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Trigger用のコルーチンを呼び出す
            StartCoroutine(KnockbackFromTriggerCoroutine(other.transform));
            TakeDamage(1);
        }
    }

    // 物理的な衝突(OnCollision)からノックバックを処理するコルーチン
    private IEnumerator KnockbackFromCollisionCoroutine(Collision2D collision)
    {
        isKnockingBack = true; // ノックバック状態をON

        // 最初の接触点の法線ベクトルを取得（衝突面から垂直に外側へ向かうベクトル）
        Vector2 contactNormal = collision.GetContact(0).normal;

        // 法線ベクトルのX成分の符号で左右を判断
        // 左から押されたらnormal.xは正(右向き)、右から押されたらnormal.xは負(左向き)になる
        float directionX = Mathf.Sign(contactNormal.x);

        // 真上/真下からの接触でdirectionXが0になる稀なケースに対応
        if (directionX == 0)
        {
            directionX = (transform.position.x > collision.transform.position.x) ? 1f : -1f;
        }

        // ノックバックの方向ベクトルを作成
        Vector2 knockbackDir = new Vector2(directionX, 0.5f).normalized;
        rb.linearVelocity = knockbackDir * KnockbackForce;

        // 指定した時間、操作不能にする
        yield return new WaitForSeconds(KnockbackTime);

        isKnockingBack = false; // ノックバック状態を解除
    }

    // トリガー(OnTrigger)からノックバックを処理するコルーチン
    private IEnumerator KnockbackFromTriggerCoroutine(Transform enemyTransform)
    {
        isKnockingBack = true; // ノックバック状態をON

        // プレイヤーと敵の水平方向の位置関係から、ノックバックの左右の向きを決定
        float directionX = (transform.position.x > enemyTransform.position.x) ? 1f : -1f;

        // ノックバックの方向ベクトルを作成
        Vector2 knockbackDir = new Vector2(directionX, 0.5f).normalized;
        rb.linearVelocity = knockbackDir * KnockbackForce;

        // 指定した時間、操作不能にする
        yield return new WaitForSeconds(KnockbackTime);

        isKnockingBack = false; // ノックバック状態を解除
    }
}
