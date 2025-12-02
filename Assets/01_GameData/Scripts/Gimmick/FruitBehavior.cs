using UnityEngine;
public class FruitBehavior : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f; [SerializeField] private LayerMask enemyLayer;
    private void Start()
    {
        Destroy(gameObject, lifetime); // éûä‘Ç≈è¡Ç¶ÇÈ
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Destroy(gameObject); // ìGÇ…ìñÇΩÇ¡ÇΩÇÁè¡Ç¶ÇÈ
        }
    }
}