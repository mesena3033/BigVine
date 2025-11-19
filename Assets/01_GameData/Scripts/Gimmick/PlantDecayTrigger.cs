using UnityEngine;

public class PlantDecayTrigger : MonoBehaviour
{
    [SerializeField] private GameObject witheredSupport; // ŒÍ‚ê‚éŽx‚¦
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("MagicBullet")) return;

        hasTriggered = true;

        if (witheredSupport != null)
        {
            Destroy(witheredSupport); // Žx‚¦‚ð‰ó‚·
        }
    }

}
