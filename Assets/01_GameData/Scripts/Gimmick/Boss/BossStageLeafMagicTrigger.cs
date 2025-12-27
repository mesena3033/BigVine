using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class BossStageLeafMagicTrigger : MonoBehaviour
{
    [Header("オブジェクト参照")]
    [SerializeField] private GameObject budObject;        // 芽
    [SerializeField] private GameObject leafOpenImage;    // 開く
    [SerializeField] private GameObject leafClosedImage;  // 閉じる

    [Header("索敵設定")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask enemyLayer; // 索敵対象のレイヤー（"Enemy"などを設定）

    [Header("ボス戦用設定")]
    [Tooltip("このハエトリグサが食べることができる敵のプレハブを指定します")]
    [SerializeField] private GameObject[] edibleEnemyPrefabs; 

    [Header("アニメーション設定")]
    [SerializeField] private float closeDuration = 0.5f;
    [SerializeField] private float openDuration = 0.5f;

    private bool hasTriggered = false;
    private bool isProcessing = false;

    private void Start()
    {
        SetActiveState(bud: true, open: false, closed: false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("MagicBullet")) return;

        hasTriggered = true;

        if (budObject != null) budObject.SetActive(false);

        SetActiveState(bud: false, open: true, closed: false);
    }

    private void Update()
    {
        if (!hasTriggered || isProcessing) return;

        // 1. まず指定レイヤーのコライダーを全て取得
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        // 2. その中から「食べられる敵」だけをリストアップする
        List<Collider2D> edibleEnemies = new List<Collider2D>();
        foreach (var hitCollider in hits)
        {
            // 食べられるプレハブのリストと比較するループ
            foreach (var prefab in edibleEnemyPrefabs)
            {
                // ヒットしたオブジェクトの名前から "(Clone)" を除いたものが、プレハブの名前と一致するかチェック
                // これにより、どのプレハブから生成されたインスタンスかを判定する
                if (hitCollider.gameObject.name.Replace("(Clone)", "") == prefab.name)
                {
                    edibleEnemies.Add(hitCollider);
                    break; // 一致するプレハブが見つかったら、次のヒットオブジェクトのチェックに移る
                }
            }
        }

        // 3. 食べられる敵が1体でもいたら、食べる動作を開始
        if (edibleEnemies.Count > 0)
        {
            // フィルタリングした敵のリストを渡す
            EatEnemies(edibleEnemies.ToArray());
            StartCoroutine(LeafCycle());
        }
    }

    private IEnumerator LeafCycle()
    {
        isProcessing = true;

        SetActiveState(bud: false, open: false, closed: true);
        yield return new WaitForSeconds(closeDuration);

        SetActiveState(bud: false, open: true, closed: false);
        yield return new WaitForSeconds(openDuration);

        SetActiveState(bud: true, open: false, closed: false);

        hasTriggered = false;

        isProcessing = false;
    }

    // メソッド名をより分かりやすく変更
    private void EatEnemies(Collider2D[] enemiesToEat)
    {
        foreach (var enemy in enemiesToEat)
        {
            // 念のため、オブジェクトがまだ存在するか確認してから破壊
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    private void SetActiveState(bool bud, bool open, bool closed)
    {
        if (budObject != null) budObject.SetActive(bud);
        if (leafOpenImage != null) leafOpenImage.SetActive(open);
        if (leafClosedImage != null) leafClosedImage.SetActive(closed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
