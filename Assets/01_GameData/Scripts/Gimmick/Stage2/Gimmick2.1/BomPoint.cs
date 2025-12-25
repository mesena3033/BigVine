using UnityEngine;

public class BomPoint : MonoBehaviour
{
    [SerializeField] private GameObject targetPlant;     // ★落ちる＆爆発する植物
    [SerializeField] private GameObject explosionPrefab; // 爆発エフェクト
    [SerializeField] private float fallTime = 1.5f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float explosionRadius = 2f;

    private bool isFalling = false;
    private float timer = 0f;

    private Vector3 plantOriginalPos; // ★元の位置を保存
    private bool canTrigger = true;   // ★魔法を受け付けるかどうか

    private void Start()
    {
        if (targetPlant != null)
        {
            plantOriginalPos = targetPlant.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTrigger) return;
        if (!other.CompareTag("MagicBullet")) return;

        // ★魔法が当たったら落下開始
        isFalling = true;
        timer = 0f;
        canTrigger = false;
    }

    private void Update()
    {
        if (isFalling && targetPlant != null)
        {
            timer += Time.deltaTime;

            // ★植物を落とす
            targetPlant.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            if (timer >= fallTime)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        if (targetPlant == null) return;

        Vector3 pos = targetPlant.transform.position;

        // ★爆発エフェクト
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, pos, Quaternion.identity);
        }

        // ★爆発範囲内の足場を動かす
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, explosionRadius);

        foreach (var h in hits)
        {
            ExplosionMover mover = h.GetComponent<ExplosionMover>();
            if (mover != null)
            {
                mover.StartMoving();
            }
        }

        // ★植物を元の位置に戻す
        targetPlant.transform.position = plantOriginalPos;

        // ★次の魔法を受け付ける準備
        isFalling = false;
        canTrigger = true;
    }
}