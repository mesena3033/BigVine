using UnityEngine;

public class FlowerBombTrigger : MonoBehaviour
{
    [SerializeField] private GameObject budObject;
    [SerializeField] private GameObject flowerObject;
    [SerializeField] private GameObject fruitPrefab;

    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float fruitLaunchForce = 300f;
    [SerializeField] private LayerMask enemyLayer;

    private bool isFlowerActive = false;
    private bool hasShot = false; // ★1回撃ったか

    private void Start()
    {
        if (flowerObject != null) flowerObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isFlowerActive) return;
        if (!other.CompareTag("MagicBullet")) return;

        isFlowerActive = true;

        if (budObject != null) budObject.SetActive(false);
        if (flowerObject != null) flowerObject.SetActive(true);

        Destroy(other.gameObject); // 魔法弾消す
    }

    private void Update()
    {
        if (!isFlowerActive) return;
        if (hasShot) return;

        Transform target = FindNearestEnemy();
        if (target != null)
        {
            LaunchFruitAt(target);
            hasShot = true; // ★ここで終了
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    private void LaunchFruitAt(Transform target)
    {
        if (fruitPrefab == null || target == null) return;

        GameObject fruit =
            Instantiate(fruitPrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = fruit.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            Vector2 dir = (target.position - transform.position).normalized;
            rb.AddForce(dir * fruitLaunchForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
