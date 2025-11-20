using UnityEngine;

public class FlowerBombTrigger : MonoBehaviour
{
    [SerializeField] private GameObject budObject;
    [SerializeField] private GameObject flowerObject;
    [SerializeField] private GameObject fruitObject;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float fruitLaunchForce = 300f;
    [SerializeField] private LayerMask enemyLayer;

    private bool isFlowerActive = false;
    private bool hasLaunchedFruit = false;

    private void Start()
    {
        if (flowerObject != null) flowerObject.SetActive(false);
        if (fruitObject != null) fruitObject.SetActive(false);
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
        if (!isFlowerActive || hasLaunchedFruit) return;

        Transform target = FindNearestEnemy();
        if (target != null)
        {
            LaunchFruitAt(target);
            hasLaunchedFruit = true;
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
        if (fruitObject == null || target == null)
        {
            Debug.Log("Fruit or target is null");
            return;
        }

        fruitObject.SetActive(true);
        fruitObject.transform.position = transform.position;

        Rigidbody2D rb = fruitObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            Vector2 direction = (target.position - transform.position).normalized;
            Debug.Log("Launching fruit toward: " + target.name + " with direction: " + direction);
            rb.AddForce(direction * fruitLaunchForce);
        }
        else
        {
            Debug.Log("No Rigidbody2D found on fruitObject");
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}