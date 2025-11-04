using UnityEngine;
using Unity.Cinemachine;

public class BombPlantTrigger : MonoBehaviour
{
    [SerializeField] private Transform sproutVisual;
    [SerializeField] private GameObject fruitObject;
    [SerializeField] private Vector3 grownScale = new Vector3(1.5f, 1.5f, 1f);
    [SerializeField] private float fruitLaunchForce = 300f;

    private bool hasTriggered = false;

    private void Start()
    {
        if (fruitObject != null)
        {
            fruitObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.gameObject.CompareTag("MagicBullet"))
        {
            hasTriggered = true;

            if (fruitObject != null)
            {
                fruitObject.SetActive(true);
                Rigidbody2D rb = fruitObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 launchDirection = new Vector2(1f, 1f).normalized;
                    rb.AddForce(launchDirection * fruitLaunchForce);
                    rb.gravityScale = 0f;
                }
            }
        }
    }
}
