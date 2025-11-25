using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 2f;

    private bool hasTriggered = false;
    private Rigidbody2D rb;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;
            Fall();
        }
    }

    private void Fall()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        StartCoroutine(SmoothFall());
    }



    private void DestroyPlatform()
    {
        Destroy(gameObject);
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

        DestroyPlatform();
    }

}
