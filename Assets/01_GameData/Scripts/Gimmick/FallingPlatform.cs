using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private float shakeDuration = 0.5f; // óhÇÍÇÈéûä‘
    [SerializeField] private float shakeAmount = 0.1f;   // óhÇÍïù
    [SerializeField] private float shakeSpeed = 20f;     // óhÇÍÇÃë¨Ç≥

    private bool hasTriggered = false;
    private Rigidbody2D rb;
    private Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered) return;

        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;
            Fall();
        }
    }

    private void Fall()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        StartCoroutine(ShakeThenFall());
    }

    private System.Collections.IEnumerator ShakeThenFall()
    {
        float timer = 0f;
        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.localPosition = originalPos + new Vector3(shake, 0, 0);

            yield return null;
        }
        transform.localPosition = originalPos;

        yield return StartCoroutine(SmoothFall());
    }

    private System.Collections.IEnumerator SmoothFall()
    {
        float speed = 5f;
        float time = 0f;

        while (time < destroyDelay)
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}