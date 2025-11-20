using UnityEngine;

public class LeafMagicTrigger : MonoBehaviour
{
    [SerializeField] private GameObject budObject;       // 芽（最初に見えてる）
    [SerializeField] private Transform leaf1;            // 葉1
    [SerializeField] private Transform leaf2;            // 葉2
    [SerializeField] private float detectionRadius = 3f; // 敵を探す範囲
    [SerializeField] private LayerMask enemyLayer;       // 敵レイヤー

    private bool hasTriggered = false;

    private void Start()
    {
        // 最初は葉っぱを非表示にしておく
        if (leaf1 != null) leaf1.gameObject.SetActive(false);
        if (leaf2 != null) leaf2.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("MagicBullet")) return;

        hasTriggered = true;

        // 芽を消す
        if (budObject != null) Destroy(budObject);

        // 葉っぱを表示して回転リセット
        if (leaf1 != null)
        {
            leaf1.gameObject.SetActive(true);
            leaf1.rotation = Quaternion.identity;
        }
        if (leaf2 != null)
        {
            leaf2.gameObject.SetActive(true);
            leaf2.rotation = Quaternion.identity;
        }

        // 敵を消す
        DestroyEnemiesInRange();
    }

    private void DestroyEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        foreach (var hit in hits)
        {
            Destroy(hit.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}