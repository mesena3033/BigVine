using UnityEngine;

public class SpecialFloor : MonoBehaviour
{
    [SerializeField] GameObject beforeImage;   // 足場完成前の画像
    [SerializeField] GameObject afterImage;    // 足場完成後の画像
    [SerializeField] Collider2D floorCollider; // 床本体 (最初は isTrigger = true)
    [SerializeField] LayerMask enemyLayer;     // 敵レイヤー

    bool isActivated = false;

    private void Start()
    {
        // 最初は「完成前」だけ表示
        if (beforeImage != null) beforeImage.SetActive(true);
        if (afterImage != null) afterImage.SetActive(false);
    }

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

        // 画像切り替え
        if (beforeImage != null) beforeImage.SetActive(false);
        if (afterImage != null) afterImage.SetActive(true);

        // 床を乗れるようにする
        floorCollider.isTrigger = false;
    }
}