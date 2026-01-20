using UnityEngine;
using System.Collections;

public class FallingPlatforms : MonoBehaviour
{
    [SerializeField] private float fallStartDelay = 0.5f;
    [SerializeField] private float fallDuration = 2f;
    [SerializeField] private float returnDelay = 1f;
    [SerializeField] private float fallSpeed = 5f;

    [Header("揺れ設定")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeSpeed = 20f;

    private bool hasTriggered = false;
    private Rigidbody2D rb;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        // ★ まずは何もせず待つ（落ちる前の間）
        yield return new WaitForSeconds(fallStartDelay);

        // ★ 落ちる直前に揺れる
        float shakeTimer = 0f;
        Vector3 basePos = originalPos;

        while (shakeTimer < shakeDuration)
        {
            shakeTimer += Time.deltaTime;

            float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.position = basePos + new Vector3(shake, 0, 0);

            yield return null;
        }

        // 揺れ終わったら位置を戻す
        transform.position = basePos;

        // ★ 落下開始
        float time = 0f;
        while (time < fallDuration)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }

        // 落下後の待機
        yield return new WaitForSeconds(returnDelay);

        // ★ 元の位置に戻す
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = originalPos;
        transform.rotation = originalRot;

        hasTriggered = false;
    }
}