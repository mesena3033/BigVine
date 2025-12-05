using UnityEngine;
using System.Collections;

public class BossVine : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private SpriteRenderer warningSprite;
    [SerializeField] private Transform vineBody;
    [SerializeField] private Transform vineTip; // 先端オブジェクト
    [SerializeField] private BoxCollider2D bodyCollider; // 胴体のコライダー
    [SerializeField] private BoxCollider2D tipCollider;  // 【追加】先端のコライダー

    [Header("設定")]
    [SerializeField] private float extendSpeed = 20f;
    [SerializeField] private float waitTime = 3.0f;

    public void StartAttack(float length, float warningDuration)
    {
        StartCoroutine(AttackSequence(length, warningDuration));
    }

    IEnumerator AttackSequence(float targetLength, float warningDuration)
    {
        // --- 1. 初期化 ---
        // 胴体を縮めておく
        vineBody.localScale = new Vector3(0, vineBody.localScale.y, 1);
        vineBody.localPosition = Vector3.zero; // 根本は親の中心(0,0)

        // 先端も根本に置いておく
        vineTip.localPosition = Vector3.zero;

        // コライダー無効化
        bodyCollider.enabled = false;
        tipCollider.enabled = false;

        // 予兆表示位置の調整
        warningSprite.transform.localScale = new Vector3(targetLength, 1, 1);
        warningSprite.transform.localPosition = new Vector3(targetLength / 2, 0, 0);
        warningSprite.gameObject.SetActive(true);

        // --- 2. 予兆点滅 ---
        float flashInterval = warningDuration / 4;
        for (int i = 0; i < 2; i++)
        {
            warningSprite.color = new Color(1f, 0f, 0f, 0.5f);
            yield return new WaitForSeconds(flashInterval);
            warningSprite.color = new Color(1f, 0f, 0f, 0.1f);
            yield return new WaitForSeconds(flashInterval);
        }
        warningSprite.gameObject.SetActive(false);

        // --- 3. 攻撃開始  ---
        bodyCollider.enabled = true;
        tipCollider.enabled = true; // 先端の攻撃判定ON

        float currentLen = 0f;

        while (currentLen < targetLength)
        {
            currentLen += extendSpeed * Time.deltaTime;
            if (currentLen > targetLength) currentLen = targetLength;

            // 胴体の更新
            vineBody.localScale = new Vector3(currentLen, vineBody.localScale.y, 1);
            // PivotがCenterのスプライトなら、長さの半分だけ位置をずらすと「左端」が固定されているように見える
            vineBody.localPosition = new Vector3(currentLen / 2, 0, 0);

            // 先端の更新
            vineTip.localPosition = new Vector3(currentLen, 0, 0);

            yield return null;
        }

        // --- 4. 待機 ---
        yield return new WaitForSeconds(waitTime);

        // --- 5. 戻る ---

        while (currentLen > 0f)
        {
            currentLen -= extendSpeed * Time.deltaTime;
            if (currentLen < 0) currentLen = 0;

            vineBody.localScale = new Vector3(currentLen, vineBody.localScale.y, 1);
            vineBody.localPosition = new Vector3(currentLen / 2, 0, 0);
            vineTip.localPosition = new Vector3(currentLen, 0, 0);

            yield return null;
        }

        Destroy(gameObject);
    }
}