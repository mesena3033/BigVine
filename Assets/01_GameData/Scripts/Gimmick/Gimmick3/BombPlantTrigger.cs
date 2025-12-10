using UnityEngine;

public class FlowerBombTrigger : MonoBehaviour
{
    [SerializeField] private GameObject budObject;
    [SerializeField] private GameObject flowerObject;
    [SerializeField] private GameObject fruitPrefab; // ← プレハブを入れる

    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float fruitLaunchForce = 300f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int maxShots = 3; // ← 撃てる回数

    private bool isFlowerActive = false;
    private int currentShots = 0;

    private bool isCooling = false;
    [SerializeField] private float fireInterval = 0.5f; // 発射間隔

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
    }

    private void Update()
    {
        if (!isFlowerActive) return;
        if (currentShots >= maxShots) return;
        if (isCooling) return; // ← クールダウン中は撃たせない

        Transform target = FindNearestEnemy();
        if (target != null)
        {
            StartCoroutine(FireRoutine(target));
        }
    }


    private Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
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

        // ★プレハブから生成
        GameObject fruit = Instantiate(fruitPrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = fruit.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            Vector2 direction = (target.position - transform.position).normalized;
            rb.AddForce(direction * fruitLaunchForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private System.Collections.IEnumerator FireRoutine(Transform target)
    {
        isCooling = true;            // クールダウン開始
        LaunchFruitAt(target);       // 発射
        currentShots++;              // 発射数カウント

        yield return new WaitForSeconds(fireInterval); // ← ここで待つ！

        isCooling = false;           // クールダウン終了
    }
}
