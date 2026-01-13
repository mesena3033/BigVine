using UnityEngine;

public class RafflesiaGrow : MonoBehaviour
{
    [Header("オブジェクト設定")]
    [SerializeField] private GameObject seedObject;      // 種の状態
    [SerializeField] private GameObject bloomObject;     // 開花画像
    [SerializeField] private ParticleSystem bloomEffect; // 開花エフェクト

    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private LayerMask platformLayer2;

    [Header("成長設定")]
    [SerializeField] private float growDuration = 1.5f;  // 大きくなる時間
    [SerializeField] private float shakeAmount = 0.1f;   // 左右揺れの幅
    [SerializeField] private float shakeSpeed = 10f;     // 揺れる速さ

    private bool isGrowing = false;
    private Vector3 originalPos;

    private void Start()
    {
        seedObject.SetActive(true);
        bloomObject.SetActive(false);

        if (bloomEffect != null)
        {
            bloomEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        originalPos = seedObject.transform.localPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGrowing) return;
        if (!other.CompareTag("MagicBullet")) return;

        StartCoroutine(GrowAndBloom());
    }

    private System.Collections.IEnumerator GrowAndBloom()
    {
        isGrowing = true;

        float timer = 0f;

        while (timer < growDuration)
        {
            timer += Time.deltaTime;
            float t = timer / growDuration;

            float scale = Mathf.Lerp(0.8f, 1f, t);
            seedObject.transform.localScale = Vector3.one * scale;

            float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            seedObject.transform.localPosition = originalPos + new Vector3(shake, 0, 0);

            yield return null;
        }

        seedObject.transform.localScale = Vector3.one;
        seedObject.transform.localPosition = originalPos;

        seedObject.SetActive(false);
        bloomObject.SetActive(true);

        if (bloomEffect != null)
        {
            bloomEffect.Play();
            yield return new WaitForSeconds(bloomEffect.main.duration);
        }

        DisableAllPlatforms();

        isGrowing = false;


    }

    private void DisableAllPlatforms()
    {
        Collider2D[] platforms = Physics2D.OverlapCircleAll(
            transform.position,
            100,                
            platformLayer, platformLayer2
        );

        foreach (var p in platforms)
        {
            p.gameObject.SetActive(false);
        }
    }
}