using UnityEngine;

public class TriggerSwitch : MonoBehaviour
{
    [SerializeField] private GameObject floorObject;
    [SerializeField] private GameObject appearObject; 

    [SerializeField] private string targetTag = "Player"; 

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag(targetTag)) return;

        triggered = true;

        if (floorObject != null)
            floorObject.SetActive(false);

        if (appearObject != null)
            appearObject.SetActive(true);
    }
}