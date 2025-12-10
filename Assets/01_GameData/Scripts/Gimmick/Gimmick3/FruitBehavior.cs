using UnityEngine;

public class FruitBehavior : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask enemyLayer;

    private void Start()
    {
        Destroy(gameObject, lifetime); // ŠÔ‚ÅÁ‚¦‚é
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // enemyLayer‚Æˆê’v‚·‚éƒŒƒCƒ„[‚É“–‚½‚Á‚½?
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Destroy(other.gameObject); // š“G‚ğíœ
            Destroy(gameObject);       // š‚±‚Ì’e‚àÁ‚¦‚é
        }
    }
}
