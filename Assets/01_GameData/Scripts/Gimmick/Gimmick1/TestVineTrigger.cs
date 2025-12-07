using UnityEngine;

public class TestVineTrigger : MonoBehaviour
{
    [SerializeField] private GameObject stage2Object;
    [SerializeField] private GameObject stage3Object;
    [SerializeField] private GameObject stage4Object;
    [SerializeField] private GameObject stage5Object;

    private int triggerCount = 0;

    private void Start()
    {
        stage2Object.SetActive(true);
        stage3Object.SetActive(false);
        stage4Object.SetActive(false);
        stage5Object.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("MagicBullet")) return;

        triggerCount++;

        if (triggerCount == 1)
        {
            // 2 → 3 → 4 とフェードインして残す
            StartCoroutine(FadeIn(stage3Object));
            StartCoroutine(FadeIn(stage4Object));
        }
        else if (triggerCount == 2)
        {
            // 5 をフェードインして残す
            StartCoroutine(FadeIn(stage5Object));

            // このタイミングで 2 を消す
            StartCoroutine(FadeOut(stage2Object));
        }
    }

    private System.Collections.IEnumerator FadeOut(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float duration = 0.5f;
        float time = 0f;
        Color original = sr.color;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            sr.color = new Color(original.r, original.g, original.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        sr.color = new Color(original.r, original.g, original.b, 0f);
        obj.SetActive(false);
    }

    private System.Collections.IEnumerator FadeIn(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        obj.SetActive(true); // 表示ON

        float duration = 0.5f;
        float time = 0f;
        Color original = sr.color;
        sr.color = new Color(original.r, original.g, original.b, 0f); // 最初透明

        while (time < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, time / duration);
            sr.color = new Color(original.r, original.g, original.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        sr.color = new Color(original.r, original.g, original.b, 1f);
    }
}