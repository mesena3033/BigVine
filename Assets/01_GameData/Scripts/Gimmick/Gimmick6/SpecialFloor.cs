using UnityEngine;

public class SpecialFloor : MonoBehaviour
{
    [SerializeField] GameObject blockCollider; // 白い四角
    [SerializeField] Collider2D floorCollider; // 床本体 (最初は isTrigger = true)
    [SerializeField] LayerMask enemyLayer;     // 敵レイヤー
    bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // レイヤー判定
        if (!isActivated && ((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Destroy(other.gameObject); // 敵を消す
            Activate();                // 足場を完成
        }
    }

    void Activate()
    {
        isActivated = true;
        blockCollider.SetActive(false);
        floorCollider.isTrigger = false; // ← 乗れる床に変化！
    }
}
