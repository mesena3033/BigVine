using UnityEngine;



public class BombPlantTrigger : MonoBehaviour
{
    [SerializeField] private Transform sproutVisual;
    [SerializeField] private GameObject fruitObject;
    [SerializeField] private Vector3 grownScale = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] private float fruitLaunchForce = 300f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    private bool hasTriggered = false;

    private void Start()
    {
        if (fruitObject != null)
        {
            fruitObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("MagicBullet"))
        {
            hasTriggered = true;

            Transform target = FindNearestEnemy();
            Vector2 launchDirection = target != null
                ? (target.position - fruitObject.transform.position).normalized
                : Vector2.down;

            if (fruitObject != null)
            {
                fruitObject.SetActive(true);
                Rigidbody2D rb = fruitObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.AddForce(launchDirection * fruitLaunchForce);
                }
            }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}