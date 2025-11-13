using Unity.Cinemachine;
using UnityEngine;

public class MagicClickTrigger : MonoBehaviour
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
            //stage3Object.SetActive(true);
            StartCoroutine(FadeOut(stage2Object));
            StartCoroutine(FadeIn(stage3Object));
            StartCoroutine(FadeOut(stage3Object));
            StartCoroutine(FadeIn(stage4Object));
            //StartCoroutine(ShowAndFade(stage4Object, stage3Object, 0.5f));
        }
        else if (triggerCount == 2)
        {
            StartCoroutine(FadeIn(stage5Object));
            StartCoroutine(FadeOut(stage4Object));
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

        obj.SetActive(true); // Ç‹Ç∏ï\é¶ONÇ…Ç∑ÇÈ

        float duration = 0.5f;
        float time = 0f;
        Color original = sr.color;
        sr.color = new Color(original.r, original.g, original.b, 0f); // ç≈èâÇÕìßñæ

        while (time < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, time / duration); // èôÅXÇ…ïsìßñæÇ÷
            sr.color = new Color(original.r, original.g, original.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        sr.color = new Color(original.r, original.g, original.b, 1f);
    }

}