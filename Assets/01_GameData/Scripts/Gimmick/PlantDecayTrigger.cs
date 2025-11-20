using UnityEngine;

public class RockReleaseTrigger : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rockBody; // Šâ‚Ì Rigidbody2D
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("MagicBullet")) return;

        hasTriggered = true;

        if (rockBody != null)
        {
            rockBody.bodyType = RigidbodyType2D.Dynamic; // Šâ‚ð“®‚©‚·
        }
    }
}

