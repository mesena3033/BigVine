using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    // シングルトンパターン
    public static CameraShaker Instance { get; private set; }

    private Coroutine currentShakeCoroutine;

    void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// カメラを揺らす処理を開始
    /// </summary>
    /// <param name="duration">揺らす時間（秒）</param>
    /// <param name="magnitude">揺れの強さ</param>
    public void Shake(float duration, float magnitude)
    {
        // 既に揺れている場合は、一度止めてから新しい揺れを開始する
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }
        currentShakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // 記憶した「基準点」に対して揺れを加える 
            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentShakeCoroutine = null;
    }
}