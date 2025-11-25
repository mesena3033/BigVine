using UnityEngine;

public class LeafMagicTrigger : MonoBehaviour
{
    [SerializeField] private GameObject budObject;
    [SerializeField] private Transform leaf1;
    [SerializeField] private Transform leaf2;

    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

    private bool hasTriggered = false;
    private bool hasClosed = false;   // ★葉っぱを閉じたかどうか

    private void Start()
    {
        if (leaf1 != null) leaf1.gameObject.SetActive(false);
        if (leaf2 != null) leaf2.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("MagicBullet")) return;

        hasTriggered = true;

        if (budObject != null) Destroy(budObject);

        if (leaf1 != null) leaf1.gameObject.SetActive(true);
        if (leaf2 != null) leaf2.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!hasTriggered || hasClosed) return;

        // 敵を探す
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        if (hits.Length > 0)
        {
            CloseLeaves();  // ★敵を見つけたら閉じる
            DestroyEnemies(hits);
            hasClosed = true;
        }
    }

    private void CloseLeaves()
    {
        // ★葉っぱを縦にする（rotation）
        leaf1.rotation = Quaternion.Euler(0, 0, 0);
        leaf2.rotation = Quaternion.Euler(0, 0, 0);

        // ★葉っぱをオブジェクト中央へ寄せる（position）
        leaf1.position = transform.position;
        leaf2.position = transform.position;
    }

    private void DestroyEnemies(Collider2D[] hits)
    {
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
