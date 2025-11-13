using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 0.5f;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private Rigidbody2D rb;

    private bool hasTriggered = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;
            Invoke(nameof(Fall), fallDelay);
        }
    }

    private void Fall()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;
        Invoke(nameof(DestroyPlatform), destroyDelay);
    }

    private void DestroyPlatform()
    {
        Destroy(gameObject);
    }


}
