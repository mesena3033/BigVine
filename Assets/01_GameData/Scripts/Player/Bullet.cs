using UnityEngine;

public class Bullet : MonoBehaviour
{
    // 弾が画面外に出たら消える処理
    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        if (viewportPos.x < 0f || viewportPos.x > 1f ||
            viewportPos.y < 0f || viewportPos.y > 1f)
        {
            Destroy(gameObject);
        }
    }

    // 弾が何かに当たった時の処理
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 成長点（ギミック）に当たったかどうかチェック
        GrowthPointGimmick gimmick = other.GetComponent<GrowthPointGimmick>();

        if (gimmick != null)
        {
            // ギミック発動
            gimmick.OnMagicHit();

            // 弾は消える
            Destroy(gameObject);
            return;
        }

        // 2. 地面や壁に当たったら消える
        //if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    Destroy(gameObject);
        //}
    }
}