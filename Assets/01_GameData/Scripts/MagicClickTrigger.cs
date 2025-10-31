using UnityEngine;

public class MagicClickTrigger : MonoBehaviour
{
    [SerializeField] private GameObject vineObject;
    private bool hasTriggered = false;

    private void Start()
    {
        if (vineObject != null)
        {
            vineObject.SetActive(false); // ç≈èâÇÕîÒï\é¶
        }
    }

    private void OnMouseDown()
    {
        if (hasTriggered) return;

        if (vineObject != null)
        {
            vineObject.SetActive(true);
            hasTriggered = true;
        }
    }
}
