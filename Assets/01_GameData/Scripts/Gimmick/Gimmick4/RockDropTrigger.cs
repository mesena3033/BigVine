using UnityEngine;

public class RockDropTrigger : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rockRb;
    [SerializeField] private float gravityScale = 2f;

    private bool hasTriggered = false;
    private GameObject lastBullet = null;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("MagicBullet") && other.gameObject != lastBullet)
        {
            lastBullet = other.gameObject;
            hasTriggered = true;

            if (rockRb != null)
            {
                rockRb.bodyType = RigidbodyType2D.Dynamic;
                rockRb.gravityScale = gravityScale;
            }
        }
    }
}
