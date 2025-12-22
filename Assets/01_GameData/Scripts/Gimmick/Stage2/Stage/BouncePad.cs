using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private float bounceForce = 12f;
    [SerializeField] private Vector2 bounceDirection = new Vector2(-1f, 1f);

    private Collider2D padCollider;
    private bool used = false;

    private void Awake()
    {
        padCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        used = true;                // ����x�Ǝg�킹�Ȃ�
        padCollider.enabled = false; // �������蔻��I�t

        Vector2 dir =
            (transform.right * bounceDirection.x +
             transform.up * bounceDirection.y).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * bounceForce, ForceMode2D.Impulse);
    }
}
