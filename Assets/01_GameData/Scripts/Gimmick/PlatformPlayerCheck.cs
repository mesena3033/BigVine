using UnityEngine;

public class PlatformPlayerCheck : MonoBehaviour
{
    [SerializeField] private MagicLiftTrigger liftTrigger;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            liftTrigger.playerOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            liftTrigger.playerOnPlatform = false;
        }
    }
}
