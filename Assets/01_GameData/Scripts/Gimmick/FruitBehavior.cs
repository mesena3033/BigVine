using UnityEngine;

public class FruitBehavior : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask enemyLayer;

    private void Start()
    {
        Destroy(gameObject, lifetime); // 時間で消える
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Destroy(gameObject); // 敵に当たったら消える
            Destroy(other.gameObject); // 敵も消す（必要なら）
        }
    }
}

