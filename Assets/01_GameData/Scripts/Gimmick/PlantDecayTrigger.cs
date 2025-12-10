using UnityEngine;

public class RockReleaseTrigger : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rockBody; // 岩の Rigidbody2D
    [SerializeField] private GameObject destroyTarget1;
    [SerializeField] private GameObject destroyTarget2;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("MagicBullet")) return;

        hasTriggered = true;

        if (rockBody != null)
        {
            rockBody.bodyType = RigidbodyType2D.Dynamic; // 岩を動かす
        }

        // オブジェクトを消す
        if (destroyTarget1 != null) Destroy(destroyTarget1);
        if (destroyTarget2 != null) Destroy(destroyTarget2);

    }
}

