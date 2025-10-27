using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private Transform startPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = startPoint.position;
        }
    }
}
