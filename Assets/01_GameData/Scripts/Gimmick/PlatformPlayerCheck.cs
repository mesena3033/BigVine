using UnityEngine;

public class PlatformPlayerCheck : MonoBehaviour
{
    [SerializeField] private MagicLift liftTrigger;
    private int insideCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            insideCount++;
            liftTrigger.playerOnPlatform = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            insideCount--;
            if (insideCount <= 0)
            {
                insideCount = 0;
                liftTrigger.playerOnPlatform = false;
            }
        }
    }
}
