using UnityEngine;

public class LeafMagicTrigger : MonoBehaviour
{
    [SerializeField] private GameObject budObject;
    [SerializeField] private GameObject leafOpenImage;   // ★閉じる前の画像
    [SerializeField] private GameObject leafClosedImage; // ★閉じた後の画像

    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

    private bool hasTriggered = false;
    private bool hasClosed = false;

    private void Start()
    {
        if (leafOpenImage != null) leafOpenImage.SetActive(false);
        if (leafClosedImage != null) leafClosedImage.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("MagicBullet")) return;

        hasTriggered = true;

        if (budObject != null) Destroy(budObject);

        // 魔法が当たったら「閉じる前の画像」を表示
        if (leafOpenImage != null) leafOpenImage.SetActive(true);
    }

    private void Update()
    {
        if (!hasTriggered || hasClosed) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        if (hits.Length > 0)
        {
            CloseLeaves();       // ★閉じた後の画像に切り替え
            DestroyEnemies(hits);
            hasClosed = true;
        }
    }

    private void CloseLeaves()
    {
        if (leafOpenImage != null) leafOpenImage.SetActive(false);
        if (leafClosedImage != null) leafClosedImage.SetActive(true);
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