using UnityEngine;
using Unity.Cinemachine;

public class LeafGrow : MonoBehaviour
{
    [SerializeField] private GameObject leaf1Normal;
    [SerializeField] private GameObject leaf1Grown;
    [SerializeField] private GameObject leaf2Normal;
    [SerializeField] private GameObject leaf2Grown;
    [SerializeField] private float detectionRadius = 1.5f;

    private int growthStage = 0;
    private GameObject lastBullet = null;

    private void Start()
    {
        leaf1Normal.SetActive(true);
        leaf1Grown.SetActive(false);
        leaf2Normal.SetActive(true);
        leaf2Grown.SetActive(false);
    }

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
            leaf1Normal.SetActive(false);
            leaf1Grown.SetActive(true);
        }
        else if (growthStage == 2)
        {
            leaf2Normal.SetActive(false);
            leaf2Grown.SetActive(true);
        }
    }
}

