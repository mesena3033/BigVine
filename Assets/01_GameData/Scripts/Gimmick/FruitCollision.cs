using UnityEngine;

public class FruitCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BreakableWall"))
        {
            Destroy(collision.gameObject);
        }

    }
}
