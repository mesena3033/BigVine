using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] private int maxHp = 10;
    private int currentHp;

    [Header("攻撃設定: 共通")]
    [SerializeField] private Transform firePoint; // 弾の発射位置

    [Header("攻撃設定: 突進 (25%)")]
    public float chargeSpeed = 10f;
    public float chargeDuration = 1.5f;
    public float returnSpeed = 5f;

    [Header("攻撃設定: ツタ (40%)")]
    [SerializeField] private GameObject vinePrefab;
    [SerializeField] private float vineWarningTime = 1.0f;

    [Header("攻撃設定: 溶解液 (35%)")]
    [SerializeField] private GameObject acidBulletPrefab; // 弾のプレハブ
    [SerializeField] private float throwForce = 10f;      // 投げる強さ
    [SerializeField] private float throwInterval = 0.5f;  // 連射間隔

    [Header("ステージ範囲制限")]
    public float minX = -16.0f;
    public float maxX = 16.0f;
    public float maxY = 12.0f;
    public float minY = -4.0f;

    [Header("参照")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer[] bodyPartsRenderers;

    private Transform playerTransform;
    private bool isDead = false;
    private Vector3 initialPosition;

    void Start()
    {
        currentHp = maxHp;
        initialPosition = transform.position;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // firePointが設定されていなければ自分の位置にする
        if (firePoint == null) firePoint = transform;

        StartCoroutine(BossBehaviorLoop());
    }

    IEnumerator BossBehaviorLoop()
    {
        while (!isDead)
        {
            // 待機
            yield return new WaitForSeconds(2.0f);
            if (isDead) break;

            if (playerTransform != null)
            {
                float roll = Random.Range(0f, 100f);

                // プレイヤーの方を向く
                FacePlayer();

                if (roll < 25f) // 0 ～ 25 (25%) -> 突進
                {
                    yield return StartCoroutine(ChargeAttack());
                }
                else if (roll < 65f) // 25 ～ 65 (40%) -> ツタ
                {
                    // ツタの中でさらに縦横をランダム分岐 (50:50)
                    if (Random.value > 0.5f)
                        yield return StartCoroutine(VineAttackHorizontal());
                    else
                        yield return StartCoroutine(VineAttackVertical());
                }
                else // 65 ～ 100 (35%) -> 溶解液
                {
                    yield return StartCoroutine(AcidSpitAttack());
                }
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player) playerTransform = player.transform;
            }
        }
    }

    // --- 攻撃: 突進 ---
    IEnumerator ChargeAttack()
    {
        Debug.Log("ボス：突進構え");
        yield return new WaitForSeconds(0.5f);

        Debug.Log("ボス：突進開始");
        float directionSign = transform.localScale.x > 0 ? -1 : 1; 
        // 簡易的にプレイヤー方向へ突進するために再計算
        float moveDir = Mathf.Sign(playerTransform.position.x - transform.position.x);

        float timer = 0f;
        while (timer < chargeDuration)
        {
            if (isDead) break;
            float moveAmount = moveDir * chargeSpeed * Time.deltaTime;
            float newX = Mathf.Clamp(transform.position.x + moveAmount, minX, maxX);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        // 戻る処理
        Debug.Log("ボス：定位置へ帰還");
        // 戻る方向を向く
        FlipBody(Mathf.Sign(initialPosition.x - transform.position.x) < 0);

        while (Vector3.Distance(transform.position, initialPosition) > 0.1f)
        {
            if (isDead) break;
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = initialPosition;
        FacePlayer();
    }

    // --- 攻撃: 横ツタ ---
    IEnumerator VineAttackHorizontal()
    {
        Debug.Log("ボス：横ツタ攻撃");
        bool isLeftStart = (Random.value > 0.5f);
        float spawnX = isLeftStart ? minX : maxX;
        float spawnY = Mathf.Clamp(playerTransform.position.y, minY + 1.0f, maxY - 1.0f);

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        float zAngle = isLeftStart ? 0f : 180f;

        SpawnVine(spawnPos, zAngle, Mathf.Abs(maxX - minX));
        yield return new WaitForSeconds(vineWarningTime + 0.5f);
    }

    // --- 攻撃: 縦ツタ ---
    IEnumerator VineAttackVertical()
    {
        Debug.Log("ボス：縦ツタ攻撃");
        float spawnX = Mathf.Clamp(playerTransform.position.x, minX + 1.0f, maxX - 1.0f);
        Vector3 spawnPos = new Vector3(spawnX, maxY, 0);
        float zAngle = -90f;
        float length = Mathf.Abs(maxY - minY);

        SpawnVine(spawnPos, zAngle, length);
        yield return new WaitForSeconds(vineWarningTime + 0.5f);
    }

    // --- 溶解液攻撃 (投擲) ---
    IEnumerator AcidSpitAttack()
    {
        Debug.Log("ボス：溶解液攻撃");

        // 3発撃つ
        for (int i = 0; i < 3; i++)
        {
            if (isDead) break;

            // 弾生成
            GameObject bullet = Instantiate(acidBulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null && playerTransform != null)
            {
                // プレイヤーへの方向計算
                Vector2 direction = (playerTransform.position - firePoint.position);

                Vector2 throwDir = direction.normalized + Vector2.up * 0.5f; // 斜め45度くらい上

                bulletRb.linearVelocity = throwDir.normalized * throwForce;
            }

            // 次の弾までの待機
            yield return new WaitForSeconds(throwInterval);
        }

        yield return new WaitForSeconds(1.0f); // 撃ち終わりの硬直
    }


    // --- 共通処理 ---
    void SpawnVine(Vector3 pos, float angle, float length)
    {
        GameObject vineObj = Instantiate(vinePrefab, pos, Quaternion.Euler(0, 0, angle));
        BossVine vineScript = vineObj.GetComponent<BossVine>();
        if (vineScript != null) vineScript.StartAttack(length, vineWarningTime);
    }

    void FacePlayer()
    {
        if (playerTransform == null) return;
        float direction = playerTransform.position.x - transform.position.x;
        FlipBody(direction < 0);
    }

    void FlipBody(bool flipX)
    {
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null) sr.flipX = flipX;
        }
    }

    // --- ダメージ・死亡 ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp -= damage;
        StartCoroutine(FlashDamageEffect());
        if (currentHp <= 0) Die();
    }

    IEnumerator FlashDamageEffect()
    {
        Color originalColor = Color.white;
        Color damageColor = new Color(0.5f, 0f, 0.5f, 1f);
        foreach (var sr in bodyPartsRenderers) if (sr != null) sr.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        foreach (var sr in bodyPartsRenderers) if (sr != null) sr.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("ボス撃破！");
        Destroy(gameObject, 0.5f);
    }
}