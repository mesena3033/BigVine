using UnityEngine;
using Unity.Cinemachine;

public class LeafGrow : MonoBehaviour
{
    [SerializeField] private Transform leafVisual;
    [SerializeField] private Vector3 midScale = new Vector3(4f, 0.8f, 1f); 
    [SerializeField] private Vector3 grownScale = new Vector3(5f, 1f, 1f);
    [SerializeField] private float detectionRadius = 1.5f;

    private int growthStage = 0;

    private GameObject lastBullet = null;

    private void Update()
    {
        if (growthStage >= 2) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("MagicBullet") && hit.gameObject != lastBullet)
            {
                lastBullet = hit.gameObject;
                Grow();
                break;
            }
        }
    }

    private void Grow()
    {
        growthStage++;

        if (growthStage == 1)
        {
            leafVisual.localScale = midScale;
        }
        else if (growthStage == 2)
        {
            leafVisual.localScale = grownScale;
        }
    }
}

