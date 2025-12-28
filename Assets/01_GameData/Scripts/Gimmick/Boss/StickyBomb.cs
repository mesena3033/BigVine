using UnityEngine;
using System.Collections;

public class StickyBomb : MonoBehaviour
{
    [Header("爆発設定")]
    [SerializeField] private float explosionRadius = 3.0f;
    [SerializeField] private int bossDamage = 3;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("時限設定")]
    [SerializeField] private float delayBeforeBlink = 0.5f;
    [SerializeField] private float firstBlinkDuration = 1.0f;
    [SerializeField] private float secondBlinkDuration = 1.0f;

    [Header("見た目")]
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private float blinkInterval = 0.2f;

    [Header("効果音")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip timerTickSound; // タイマーのループ音
    [SerializeField] private AudioClip explosionSound; // 爆発音

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // audioSourceがアタッチされていなければ追加する（念のため）
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 爆弾が生成(Instantiate)された時に自動で呼び出されるStartメソッド
    void Start()
    {
        // 地面への接触を待たずに、すぐに爆発シーケンスを開始する
        StartCoroutine(ExplosionSequence());

        // タイマー音の再生を開始
        if (audioSource != null && timerTickSound != null)
        {
            audioSource.clip = timerTickSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }


    private IEnumerator ExplosionSequence()
    {
        // 1. 地面に張り付いてから少し待つ
        yield return new WaitForSeconds(delayBeforeBlink);

        // 2. 通常速度で点滅 (1秒間)
        float elapsedTime = 0f;
        while (elapsedTime < firstBlinkDuration)
        {
            spriteRenderer.color = (spriteRenderer.color == originalColor) ? blinkColor : originalColor;
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        // 3. 2倍速で点滅 (1秒間)
        float fastBlinkInterval = blinkInterval / 2f;
        elapsedTime = 0f;
        while (elapsedTime < secondBlinkDuration)
        {
            spriteRenderer.color = (spriteRenderer.color == originalColor) ? blinkColor : originalColor;
            yield return new WaitForSeconds(fastBlinkInterval);
            elapsedTime += fastBlinkInterval;
        }

        // 4. 爆発
        Explode();
    }

    private void Explode()
    {
        // タイマー音を停止
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 爆発音を再生
        // 自身がすぐDestroyされるため、音が途切れないようにPlayClipAtPointで再生
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // 爆発エフェクトが設定されていれば生成する
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 爆発範囲内のすべてのコライダーを取得する
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        // ボスに複数回ダメージを与えないようにするためのフラグ
        bool bossAlreadyDamaged = false;

        // 検知したすべてのコライダーをループでチェックする
        foreach (var hitCollider in colliders)
        {
            // まず、当たったオブジェクト自身、またはその親オブジェクトにBossControllerがあるか探す
            BossController boss = hitCollider.GetComponentInParent<BossController>();

            // もしボスが直接見つかり、まだこの爆発でダメージを与えていないなら
            if (boss != null && !bossAlreadyDamaged)
            {
                // ボスにダメージを与え、フラグを立てる
                boss.TakeDamage(bossDamage);
                bossAlreadyDamaged = true;
                Debug.Log("爆発がボス本体にヒット！");
            }
            // もしボスが直接見つからず、まだダメージも与えていない場合
            else if (!bossAlreadyDamaged)
            {
                // 次に、当たったオブジェクト自身、またはその親にBossVineがあるか探す
                BossVine vine = hitCollider.GetComponentInParent<BossVine>();

                // もしツタが見つかり、そのツタに主人（ボス）が設定されているなら
                if (vine != null && vine.ownerBoss != null)
                {
                    // ツタの主人であるボスにダメージを与え、フラグを立てる
                    vine.ownerBoss.TakeDamage(bossDamage);
                    bossAlreadyDamaged = true;
                    Debug.Log("爆発がツタにヒット！ ボスにダメージ！");
                }
            }
        }

        // 自身のゲームオブジェクトを破棄する
        Destroy(gameObject);
    }
}