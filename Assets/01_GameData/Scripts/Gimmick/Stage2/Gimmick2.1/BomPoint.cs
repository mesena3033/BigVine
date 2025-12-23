using UnityEngine;

public class BomPoint : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float fallTime = 1.5f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float explosionRadius = 2f; // ★ 爆発範囲

    private bool isFalling = false;
    private float timer = 0f;

    private void Update()
    {
        if (!isFalling && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.gameObject == this.gameObject)
            {
                isFalling = true;
                timer = 0f;
            }
        }

        if (isFalling)
        {
            timer += Time.deltaTime;
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            if (timer >= fallTime)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        // 爆発エフェクト
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // ★ 爆発範囲内の足場を探す
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var h in hits)
        {
            ExplosionMover mover = h.GetComponent<ExplosionMover>();
            if (mover != null)
            {
                mover.StartMoving();
            }
        }

        Destroy(gameObject);
    }


}


