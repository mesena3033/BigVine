using UnityEngine;
using System.Collections;

public class LeafMagicTrigger : MonoBehaviour
{
    [SerializeField] private GameObject budObject;        // 芽
    [SerializeField] private GameObject leafOpenImage;    // 開く
    [SerializeField] private GameObject leafClosedImage;  // 閉じる

    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] private float closeDuration = 0.5f;
    [SerializeField] private float openDuration = 0.5f;

    private bool hasTriggered = false;
    private bool isProcessing = false;

    private void Start()
    {
        // ★最初は芽だけ表示
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        if (hits.Length > 0)
        {
            DestroyEnemies(hits);
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

    private void DestroyEnemies(Collider2D[] hits)
    {
        foreach (var hit in hits)
        {
            Destroy(hit.gameObject);
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