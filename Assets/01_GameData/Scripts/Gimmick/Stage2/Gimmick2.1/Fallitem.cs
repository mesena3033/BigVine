using UnityEngine;

public class Fallitem : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool used = false; // 一回だけ落ちる

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // 最初は動かない
    }

    private void Update()
    {
        if (used) return;

        if (Input.GetMouseButtonDown(0))
        {
            // マウス位置をワールド座標に変換
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // クリック位置にあるコライダーを取得
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            // 自分自身がクリックされた？
            if (hit != null && hit.gameObject == this.gameObject)
            {
                Fall();
            }


        }
    }
    private void Fall()
    {
        used = true;
        rb.bodyType = RigidbodyType2D.Dynamic; // ← これで落ちる！
    }


}


