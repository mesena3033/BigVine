using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ツタの種類を定義
public enum VineType
{
    Horizontal, // 横ツタ
    Vertical    // 縦ツタ
}

public class BossVine : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private SpriteRenderer warningSprite;
    [SerializeField] private Transform vineBody;
    [SerializeField] private Transform vineTip;
    [SerializeField] private BoxCollider2D bodyCollider;
    [SerializeField] private BoxCollider2D tipCollider;

    [Header("設定")]
    [SerializeField] private float extendSpeed = 20f;
    [SerializeField] private float waitTime = 3.0f;

    [Header("葉っぱ（足場）の設定")]
    [Tooltip("足場として生成する葉っぱのプレハブ")]
    [SerializeField] private GameObject leafPlatformPrefab;
    [Tooltip("葉っぱを生成する間隔（ツタの伸長方向）")]
    [SerializeField] private float leafSpawnInterval = 2.5f;
    [Tooltip("ツタの中心から垂直方向にどれだけ離して葉っぱを配置するか")]
    [SerializeField] private float leafSpawnOffsetY = 1.2f;

    public BossController ownerBoss;
    private List<GameObject> spawnedLeaves = new List<GameObject>();
    private bool spawnLeafOnLeft = true;　//葉っぱを左右交互に生成するためのフラグ

    public void StartAttack(float length, float warningDuration, VineType type)
    {
        // 受け取ったタイプをAttackSequenceに引き継ぐ
        StartCoroutine(AttackSequence(length, warningDuration, type));
    }

    IEnumerator AttackSequence(float targetLength, float warningDuration, VineType vineType)
    {
        // --- 1. 初期化 ---
        vineBody.localScale = new Vector3(0, vineBody.localScale.y, 1);
        vineBody.localPosition = Vector3.zero;
        vineTip.localPosition = Vector3.zero;
        bodyCollider.enabled = false;
        tipCollider.enabled = false;
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

        // --- 3. 攻撃開始 ---
        tipCollider.enabled = true; // 先端の攻撃判定は常にON

        // タイプに応じて胴体の当たり判定を切り替える
        if (vineType == VineType.Horizontal)
        {
            // 横ツタなら、胴体の当たり判定を有効にする 
            bodyCollider.enabled = true;
        }
        // Verticalの場合は、無効

        // 最初の葉っぱを左右どちらから出すかランダム
        spawnLeafOnLeft = (Random.value < 0.5f);

        float currentLen = 0f;
        float nextLeafSpawnPoint = leafSpawnInterval;

        while (currentLen < targetLength)
        {
            currentLen += extendSpeed * Time.deltaTime;
            if (currentLen > targetLength) currentLen = targetLength;

            vineBody.localScale = new Vector3(currentLen, vineBody.localScale.y, 1);
            vineBody.localPosition = new Vector3(currentLen / 2, 0, 0);
            vineTip.localPosition = new Vector3(currentLen, 0, 0);

            // 縦ツタの場合のみ、葉っぱを生成
            if (vineType == VineType.Vertical)
            {
                if (leafPlatformPrefab != null && currentLen >= nextLeafSpawnPoint)
                {
                    // フラグに基づいて左右どちらに生成するか決定
                    float direction = spawnLeafOnLeft ? -1f : 1f;
                    SpawnLeaf(nextLeafSpawnPoint, direction);

                    // 次回のためにフラグを反転させる
                    spawnLeafOnLeft = !spawnLeafOnLeft;

                    nextLeafSpawnPoint += leafSpawnInterval;
                }
            }
            yield return null;
        }

        // --- 4. 待機 ---
        yield return new WaitForSeconds(waitTime);

        // --- 5. 戻る ---

        // 縦ツタの場合のみ、葉っぱを破棄する
        if (vineType == VineType.Vertical)
        {
            foreach (var leaf in spawnedLeaves)
            {
                if (leaf != null) Destroy(leaf);
            }
            spawnedLeaves.Clear();
        }

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

    private void SpawnLeaf(float spawnX, float direction)
    {
        // 1. 葉っぱのローカル座標を計算
        Vector3 localSpawnPosition = new Vector3(spawnX, leafSpawnOffsetY * direction, 0);

        // 2. ローカル座標をワールド座標に変換
        Vector3 worldSpawnPosition = transform.TransformPoint(localSpawnPosition);

        // 3. ワールド座標基準で、回転させずに(Quaternion.identity)生成
        GameObject leaf = Instantiate(leafPlatformPrefab, worldSpawnPosition, Quaternion.identity);

        // 4. 生成した葉っぱを管理リストに追加
        spawnedLeaves.Add(leaf);

        // 5. 向きの調整
        if (direction < 0)
        {
            // 左右どちらに反転させるかは、ツタの向きによるのでプレハブの見た目に合わせて調整
            SpriteRenderer sr = leaf.GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipY = true;
        }
    }
}