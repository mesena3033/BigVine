using UnityEngine;

public class MagicLift : MonoBehaviour
{
    [SerializeField] private Rigidbody2D platformRB;
    [SerializeField] private float liftAmount = 3f;
    [SerializeField] private float stayTime = 2f;
    [SerializeField] private float upSpeed = 20f;     // 上昇スピード
    [SerializeField] private float downSpeed = 14f;   // 下降スピード

    [SerializeField] private Transform player;

    [SerializeField] private GameObject liftImage;    // ★追加画像（普段表示、上昇中は非表示）

    public bool playerOnPlatform = false;

    private Vector2 originalPos;
    private Vector2 liftedPos;

    private bool isMoving = false;

    private void Start()
    {
        originalPos = platformRB.position;
        liftedPos = originalPos + Vector2.up * liftAmount;

        // 最初は画像を表示しておく
        if (liftImage != null) liftImage.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isMoving) return;
        if (!other.CompareTag("MagicBullet")) return;

        StopAllCoroutines();
        StartCoroutine(LiftThenReturn());
    }

    private System.Collections.IEnumerator LiftThenReturn()
    {
        isMoving = true;

        // 上昇開始 → 画像を消す
        if (liftImage != null) liftImage.SetActive(false);

        // 上昇処理
        while (Vector2.Distance(platformRB.position, liftedPos) > 0.01f)
        {
            Vector2 target = Vector2.MoveTowards(platformRB.position, liftedPos, upSpeed * Time.deltaTime);
            platformRB.MovePosition(target);
            yield return null;
        }
        platformRB.MovePosition(liftedPos);

        yield return new WaitForSeconds(stayTime);

        // 下降処理
        while (Vector2.Distance(platformRB.position, originalPos) > 0.01f)
        {
            Vector2 target = Vector2.MoveTowards(platformRB.position, originalPos, downSpeed * Time.deltaTime);
            platformRB.MovePosition(target);
            yield return null;
        }
        platformRB.MovePosition(originalPos);

        // 下降完了 → 画像を再表示
        if (liftImage != null) liftImage.SetActive(true);

        isMoving = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(platformRB.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
