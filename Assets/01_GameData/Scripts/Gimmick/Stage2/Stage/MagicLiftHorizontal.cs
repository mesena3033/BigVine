using UnityEngine;

public class MagicLiftHorizontal : MonoBehaviour
{
    [SerializeField] private Rigidbody2D platformRB;
    [SerializeField] private float moveAmount = 3f;     // 右に出る距離
    [SerializeField] private float stayTime = 2f;
    [SerializeField] private float outSpeed = 20f;      // 出てくるスピード
    [SerializeField] private float backSpeed = 14f;     // 戻るスピード

    [SerializeField] private GameObject liftImage;      // 普段表示、移動中は非表示

    [Header("効果音")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound; // 起動する時のSE

    private Vector2 originalPos;
    private Vector2 movedPos;

    private bool isMoving = false;

    private void Start()
    {
        originalPos = platformRB.position;
        movedPos = originalPos + Vector2.right * moveAmount;

        if (liftImage != null) liftImage.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isMoving) return;
        if (!other.CompareTag("MagicBullet")) return;

        StopAllCoroutines();
        StartCoroutine(MoveThenReturn());
    }

    private System.Collections.IEnumerator MoveThenReturn()
    {
        isMoving = true;

        // 起動音を再生
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // 出現開始 → 画像を消す
        if (liftImage != null) liftImage.SetActive(false);

        // 右へ移動
        while (Vector2.Distance(platformRB.position, movedPos) > 0.01f)
        {
            Vector2 target = Vector2.MoveTowards(
                platformRB.position,
                movedPos,
                outSpeed * Time.deltaTime
            );
            platformRB.MovePosition(target);
            yield return null;
        }
        platformRB.MovePosition(movedPos);

        yield return new WaitForSeconds(stayTime);

        // 元の位置へ戻る
        while (Vector2.Distance(platformRB.position, originalPos) > 0.01f)
        {
            Vector2 target = Vector2.MoveTowards(
                platformRB.position,
                originalPos,
                backSpeed * Time.deltaTime
            );
            platformRB.MovePosition(target);
            yield return null;
        }
        platformRB.MovePosition(originalPos);

        // 完了 → 画像再表示
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
