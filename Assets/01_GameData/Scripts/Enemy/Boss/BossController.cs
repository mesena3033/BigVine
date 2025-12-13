using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] private int maxHp = 10;
    private int currentHp;

    [Header("攻撃設定: 共通")]
    [SerializeField] private Transform firePoint; // 弾の発射位置

    [Header("攻撃設定: 突進 (25%)")]
    [SerializeField] private GameObject warningAreaPrefab; // 突進の警告エフェクト（赤い四角）
    [SerializeField] private float chargeWarningTime = 1.2f; // 突進の警告表示時間
    [SerializeField] private float blinkInterval = 0.2f;     // 警告の点滅間隔（秒）
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

    [Header("溶解液攻撃: 警告")]
    [SerializeField] private SpriteRenderer headRenderer;           // 点滅させる頭のSpriteRenderer
    [SerializeField] private Color acidWarningColor = Color.red;   // 警告色
    [SerializeField] private float acidWarningTime = 1.0f;         // 警告の表示時間
    [SerializeField] private float acidBlinkInterval = 0.2f;       // 点滅間隔

    [Header("攻撃設定: 大技分岐")]
    [SerializeField] private float diveBombChance = 0.55f; // 押しつぶし攻撃を行う確率 (55%)

    [Header("攻撃設定: 急降下爆撃 (別枠30%)")]
    [SerializeField] private float diveFollowTime = 2.0f;    // 警告がプレイヤーを追従する時間
    [SerializeField] private float diveFastBlinkTime = 1.0f; // 警告が高速点滅する時間
    [SerializeField] private float diveFallSpeed = 40f;      // ボスが上から落ちてくる速度
    [SerializeField] private float diveStunTime = 2.0f;      // 着地後の行動不能時間
    [SerializeField] private float submergeAnimTime = 1.0f; // 地面に潜る/戻るアニメーションの時間

    [Header("攻撃設定: 画面奥からの範囲攻撃")]
    [SerializeField] private GameObject rangeAttackHitboxPrefab;      // 攻撃判定のプレハブ
    [SerializeField] private float zoomInDuration = 1.5f;      // 顔が奥から迫ってくる時間
    [SerializeField] private float rangeWarningTime = 2.0f;    // 警告線の表示時間
    [SerializeField] private float attackTravelTime = 0.2f;    // 攻撃判定が中心まで到達する時間
    [SerializeField] private int numberOfAttacks = 6;          // 攻撃の数（警告線の数）
    [SerializeField] private float attackRadius = 15f;         // 攻撃が開始される半径（警告線の長さ）

    [Header("ステージ範囲制限")]
    public float minX = -16.0f;
    public float maxX = 16.0f;
    public float maxY = 12.0f;
    public float minY = -4.0f;

    [Header("参照")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer[] bodyPartsRenderers;

    [Header("効果音")]
    [SerializeField] private AudioSource audioSource; // 音を再生するコンポーネント
    [SerializeField] private AudioClip damageSound;   // ダメージを受けた時のSE
    [SerializeField] private AudioClip deathSound;  // 倒された時のSE

    private Transform playerTransform;
    private bool isDead = false;
    private Vector3 initialPosition;
    private Vector3 initialHeadScale; // 顔の初期スケールを保存する変数
    private Vector3 initialHeadLocalPosition;

    void Start()
    {
        currentHp = maxHp;
        initialPosition = transform.position;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // firePointが設定されていなければ自分の位置にする
        if (firePoint == null) firePoint = transform;

        // 顔の初期スケールを保存
        if (headRenderer != null)
        {
            initialHeadScale = headRenderer.transform.localScale;
            initialHeadLocalPosition = headRenderer.transform.localPosition;
        }

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
                // プレイヤーの方を向く
                FacePlayer();

                float specialAttackRoll = Random.Range(0f, 100f);

                // 30%の確率で新しい大技「急降下爆撃」を行う
                if (specialAttackRoll < 30f)
                {
                    yield return StartCoroutine(SpecialAttackBranch());
                }
                else // 残り70%の場合は、これまで通りの通常攻撃を行う
                {
                    float normalAttackRoll = Random.Range(0f, 100f);

                    if (normalAttackRoll < 25f) // 0 ～ 25 (25%) -> 突進
                    {
                        yield return StartCoroutine(ChargeAttack());
                    }
                    else if (normalAttackRoll < 65f) // 25 ～ 65 (40%) -> ツタ
                    {
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

        float moveDir;

        GameObject warningInstance = null; // 生成したオブジェクトを保持する変数
        if (warningAreaPrefab != null && playerTransform != null)
        {
            // --- ① プレハブから警告オブジェクトを生成 ---
            warningInstance = Instantiate(warningAreaPrefab);

            // --- ② 突進範囲を計算 ---
            moveDir = Mathf.Sign(playerTransform.position.x - transform.position.x);
            float chargeDistance = chargeSpeed * chargeDuration;
            float startX = transform.position.x;
            float endX = Mathf.Clamp(startX + moveDir * chargeDistance, minX, maxX);

            // --- ③ 警告のサイズと位置を設定 ---
            float warningWidth = Mathf.Abs(endX - startX);
            float warningCenterX = startX + (endX - startX) / 2;

            Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
            foreach (var sr in bodyPartsRenderers)
            {
                if (sr != null) totalBounds.Encapsulate(sr.bounds);
            }
            float warningHeight = totalBounds.size.y;
            float warningCenterY = totalBounds.center.y;

            // 生成したインスタンスの位置とサイズを変更
            warningInstance.transform.position = new Vector3(warningCenterX, warningCenterY, 0);
            warningInstance.transform.localScale = new Vector3(warningWidth, warningHeight, 1);

            // --- ④ 点滅処理 ---
            float elapsedTime = 0f;
            warningInstance.SetActive(true);

            while (elapsedTime < chargeWarningTime)
            {
                warningInstance.SetActive(!warningInstance.activeSelf);
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }

            // --- ⑤ 使い終わったオブジェクトを破棄 ---
            Destroy(warningInstance);
        }
        else
        {
            yield return new WaitForSeconds(chargeWarningTime);
        }

        Debug.Log("ボス：突進開始");
        float directionSign = transform.localScale.x > 0 ? -1 : 1; 
        // 簡易的にプレイヤー方向へ突進するために再計算
        moveDir = Mathf.Sign(playerTransform.position.x - transform.position.x);

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
        Debug.Log("ボス：溶解液攻撃準備");

        if (headRenderer != null)
        {
            // 1. 元の色を保存しておく
            Color originalColor = headRenderer.color;
            float elapsedTime = 0f;

            // 2. 警告時間中、点滅を繰り返すループ
            while (elapsedTime < acidWarningTime)
            {
                // 色を警告色と元の色で交互に切り替える
                headRenderer.color = (headRenderer.color == originalColor) ? acidWarningColor : originalColor;

                // 設定した間隔で待機
                yield return new WaitForSeconds(acidBlinkInterval);
                elapsedTime += acidBlinkInterval;
            }

            // 3. 警告終了後、必ず元の色に戻す
            headRenderer.color = originalColor;
        }
        else
        {
            // headRendererが設定されていない場合は、単純に待つだけ
            yield return new WaitForSeconds(acidWarningTime);
        }

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

    // --- 攻撃分岐: 大技 ---
    IEnumerator SpecialAttackBranch()
    {
        Debug.Log("ボス：[大技] 準備");

        // --- 1. 地中に潜って消える (アニメーション) ---
        yield return StartCoroutine(AnimateSubmerge(true, submergeAnimTime));

        // --- 2. 確率で攻撃を分岐 ---
        if (Random.value < diveBombChance) // 55%で押しつぶし
        {
            yield return StartCoroutine(DiveBombAttack());
        }
        else // 残り45%で新攻撃
        {
            yield return StartCoroutine(BackgroundRangeAttack());
        }

        // --- 3. どちらの攻撃後でも、元の場所に戻るアニメーションを行う ---
        yield return StartCoroutine(AnimateSubmerge(false, submergeAnimTime));

        Debug.Log("ボス：通常パターンに復帰");
    }

    // --- 攻撃: 急降下爆撃 ---
    IEnumerator DiveBombAttack()
    {
        Debug.Log("ボス：[大技] 急降下爆撃開始");

        // --- 2. 警告エリアを生成し、プレイヤーを追従 ---
        GameObject warning = Instantiate(warningAreaPrefab);
        Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
        foreach (var sr in bodyPartsRenderers) if (sr != null) totalBounds.Encapsulate(sr.bounds);

        float warningWidth = totalBounds.size.x;
        float warningHeight = maxY - minY;
        float warningCenterY = (maxY + minY) / 2f;
        warning.transform.localScale = new Vector3(warningWidth, warningHeight, 1);

        float elapsedTime = 0f;
        float blinkInterval = 0.2f;
        while (elapsedTime < diveFollowTime)
        {
            warning.transform.position = new Vector3(playerTransform.position.x, warningCenterY, 0);
            warning.SetActive(!warning.activeSelf);
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        // --- 3. 追従終了、警告の点滅を高速化 ---
        elapsedTime = 0f;
        float fastBlinkInterval = blinkInterval / 2;
        while (elapsedTime < diveFastBlinkTime)
        {
            warning.SetActive(!warning.activeSelf);
            yield return new WaitForSeconds(fastBlinkInterval);
            elapsedTime += fastBlinkInterval;
        }

        float fallXPosition = warning.transform.position.x;
        Destroy(warning);

        // --- 4. 上から高速で落ちてくる ---
        transform.position = new Vector3(fallXPosition, maxY + 5f, 0);
        foreach (var sr in bodyPartsRenderers) sr.enabled = true;

        Vector3 targetFallPosition = new Vector3(fallXPosition, minY + totalBounds.size.y / 2, 0);
        while (transform.position.y > targetFallPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetFallPosition, diveFallSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetFallPosition;

        // --- 5. 着地後、一定時間行動不能 ---
        Debug.Log("ボス：着地、行動不能");
        yield return new WaitForSeconds(diveStunTime);

        // --- 6. 再び潜る ---
        // 攻撃後の復帰は SpecialAttackBranch で行うので、ここでは潜るだけ
        float endY = transform.position.y - totalBounds.size.y;
        float submergeDuration = submergeAnimTime * 0.5f; // 短めに
        elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, endY, startPos.z);
        while (elapsedTime < submergeDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / submergeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        foreach (var sr in bodyPartsRenderers) sr.enabled = false;
    }

    // --- 攻撃: 画面奥からの範囲攻撃 ---
    IEnumerator BackgroundRangeAttack()
    {
        Debug.Log("ボス：[大技] 画面奥からの範囲攻撃");

        // --- 1. 顔以外のパーツを非表示にする ---
        foreach (var sr in bodyPartsRenderers)
        {
            if (headRenderer != null && sr != headRenderer)
            {
                sr.enabled = false;
            }
        }

        // --- 2. ボス本体を画面中央へ移動させる ---
        Vector3 screenCenter = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        transform.position = screenCenter;

        // --- 3. 顔が画面中央奥から迫ってくる演出 ---
        if (headRenderer != null)
        {
            headRenderer.enabled = true;
            headRenderer.transform.localPosition = Vector3.zero; // 顔の相対位置を一時的にゼロに

            Vector3 startScale = Vector3.zero;
            Vector3 targetScale = initialHeadScale * 2f;

            float elapsedTime = 0f;
            while (elapsedTime < zoomInDuration)
            {
                float t = elapsedTime / zoomInDuration;
                headRenderer.transform.localScale = Vector3.Lerp(startScale, targetScale, t * t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            headRenderer.transform.localScale = targetScale;
        }

        // --- 4. 共通の警告プレハブを使って放射状の警告線を生成＆点滅 ---
        List<GameObject> warningInstances = new List<GameObject>();
        for (int i = 0; i < numberOfAttacks; i++)
        {
            if (warningAreaPrefab == null) break;

            float angle = 360f / numberOfAttacks * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * Vector3.right;

            Vector3 warningScale = new Vector3(attackRadius, 0.3f, 1f);
            Vector3 warningPosition = screenCenter + direction * (attackRadius / 2f);

            GameObject warning = Instantiate(warningAreaPrefab, warningPosition, rotation);
            warning.transform.localScale = warningScale;
            warningInstances.Add(warning);
        }

        float warningElapsedTime = 0f;
        while (warningElapsedTime < rangeWarningTime)
        {
            foreach (var warning in warningInstances)
            {
                warning.SetActive(!warning.activeSelf);
            }
            yield return new WaitForSeconds(blinkInterval);
            warningElapsedTime += blinkInterval;
        }

        foreach (var warning in warningInstances)
        {
            if (warning != null) warning.SetActive(true);
        }

        // --- 5. 警告線の先端から中心へ攻撃判定を走らせる ---
        List<GameObject> hitboxes = new List<GameObject>();
        foreach (var warning in warningInstances)
        {
            if (rangeAttackHitboxPrefab == null || warning == null) break;

            Vector3 direction = warning.transform.right;
            Vector3 startPos = screenCenter + direction * attackRadius;

            GameObject hitbox = Instantiate(rangeAttackHitboxPrefab, startPos, warning.transform.rotation);
            hitboxes.Add(hitbox);

            StartCoroutine(MoveHitbox(hitbox.transform, screenCenter, attackTravelTime));
        }

        yield return new WaitForSeconds(attackTravelTime + 0.5f);

        // --- 6. クリーンアップ ---
        foreach (var warning in warningInstances) Destroy(warning);
        foreach (var hitbox in hitboxes) Destroy(hitbox);

        if (headRenderer != null)
        {
            headRenderer.enabled = false;
            headRenderer.transform.localScale = initialHeadScale;
            headRenderer.transform.localPosition = initialHeadLocalPosition; // 顔の相対位置を元に戻す
        }
    }

    IEnumerator MoveHitbox(Transform hitbox, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = hitbox.position;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (hitbox == null) yield break; // 移動中に破壊された場合
            hitbox.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (hitbox != null) Destroy(hitbox.gameObject);
    }


    // --- 共通処理 ---
    void SpawnVine(Vector3 pos, float angle, float length)
    {
        GameObject vineObj = Instantiate(vinePrefab, pos, Quaternion.Euler(0, 0, angle));
        BossVine vineScript = vineObj.GetComponent<BossVine>();
        if (vineScript != null)
        {
            vineScript.ownerBoss = this; // 生成したツタに、自分自身(ボス)の情報を渡す

            vineScript.StartAttack(length, vineWarningTime);
        }
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

        // SEが設定されていれば再生する
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

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

        // オーディオソースと撃破SEが設定されていれば再生する
        if (audioSource != null && deathSound != null)
        {
            // 他の音を止めずに再生する
            audioSource.PlayOneShot(deathSound);
        }

        Destroy(gameObject, 0.5f);
    }

    // --- 地面に潜る/戻るアニメーション ---
    private IEnumerator AnimateSubmerge(bool isSubmerging, float duration)
    {
        // ボスの高さを計算
        Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
        foreach (var sr in bodyPartsRenderers) if (sr != null) totalBounds.Encapsulate(sr.bounds);
        float bossHeight = totalBounds.size.y;

        Vector3 startPos, endPos;

        if (isSubmerging) // 潜る場合
        {
            startPos = transform.position;
            endPos = new Vector3(startPos.x, startPos.y - bossHeight, startPos.z);
        }
        else // 戻ってくる（浮上する）場合
        {
            startPos = new Vector3(initialPosition.x, initialPosition.y - bossHeight, initialPosition.z);
            endPos = initialPosition;
            transform.position = startPos; // 見えないうちに地下へ移動
            foreach (var sr in bodyPartsRenderers) sr.enabled = true; // 表示を戻す
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos; // ぴったり位置を合わせる

        if (isSubmerging) // 潜り終わったら非表示にする
        {
            foreach (var sr in bodyPartsRenderers) sr.enabled = false;
        }
    }
}