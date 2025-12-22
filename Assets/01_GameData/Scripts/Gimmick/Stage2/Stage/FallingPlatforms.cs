using UnityEngine;
using System.Collections;

public class FallingPlatforms : MonoBehaviour
{
    [SerializeField] private float fallStartDelay = 0.5f;
    [SerializeField] private float fallDuration = 2f;   // 落ち続ける時間
    [SerializeField] private float returnDelay = 1f;    // 元の位置に戻るまでの待ち時間
    [SerializeField] private float fallSpeed = 5f;      // 落下スピード

    private bool hasTriggered = false;
    private Rigidbody2D rb;

    private Vector3 originalPos;       // 元の位置
    private Quaternion originalRot;    // 元の角度

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 最初の位置と角度を保存
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered) return;

        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;
            StartCoroutine(FallRoutine());
        }
    }

    private IEnumerator FallRoutine()
    {
        yield return new WaitForSeconds(fallStartDelay);

        // 落下開始
        float time = 0f;

        while (time < fallDuration)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }

        // 落ちた後、少し待つ
        yield return new WaitForSeconds(returnDelay);

        // 元の位置に戻す
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = originalPos;
        transform.rotation = originalRot;

        // 次にまた踏んだら落ちるようにする
        hasTriggered = false;
    }
}