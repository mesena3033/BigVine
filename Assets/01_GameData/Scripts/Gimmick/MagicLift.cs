using UnityEngine;

public class MagicLiftTrigger : MonoBehaviour
{
    [SerializeField] private Transform[] targetObjects; // 動かす対象（床・壁・見た目）
    [SerializeField] private Transform player;          // プレイヤーを指定
    [SerializeField] private float liftAmount = 3f;     // 上に動かす距離
    [SerializeField] private float duration = 5f;       // 戻るまでの時間

    private Vector3[] originalPositions;
    private bool isLifted = false;
    public bool playerOnPlatform = false;

    private void Start()
    {
        originalPositions = new Vector3[targetObjects.Length];
        for (int i = 0; i < targetObjects.Length; i++)
        {
            originalPositions[i] = targetObjects[i].position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 魔法弾が「床以外のこのトリガー」に当たったら床を動かす
        if (!isLifted && other.CompareTag("MagicBullet"))
        {
            isLifted = true;

            // 床・壁・見た目を持ち上げる
            for (int i = 0; i < targetObjects.Length; i++)
            {
                targetObjects[i].position = originalPositions[i] + Vector3.up * liftAmount;
            }

            // プレイヤーが床に乗っているときだけ持ち上げる
            if (playerOnPlatform && player != null)
            {
                player.position += Vector3.up * liftAmount;
            }

            Invoke(nameof(ReturnToOriginal), duration);
        }
    }

    private void ReturnToOriginal()
    {
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i].position = originalPositions[i];
        }

        if (playerOnPlatform && player != null)
        {
            player.position -= Vector3.up * liftAmount;
        }

        isLifted = false;
    }

    // プレイヤーが床に乗ったとき
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = true;
        }
    }

    // プレイヤーが床から離れたとき
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;
        }
    }
}