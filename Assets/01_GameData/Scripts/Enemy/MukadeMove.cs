using TMPro;
using UnityEngine;

public class MukadeMove : MonoBehaviour
{
    [SerializeField] private LayerMask m_Layer;

    //  移動スピード
    [SerializeField] private float MoveHoriSpeed;

    [Header("消えるライン")]

    //  これ以下／以上で消す
    [SerializeField] private float DestroyYMin;
    [SerializeField] private float DestroyYMax;

    private Rigidbody2D Rb;
    private SpriteRenderer Sr;

    float yPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        Sr = GetComponent<SpriteRenderer>();

        yPos = transform.position.y;
    }

    private void FixedUpdate()
    {
        yPos = transform.position.y;

        if (yPos < DestroyYMax)
        {
            Vector2 delta = Vector2.up * MoveHoriSpeed * Time.fixedDeltaTime;
            Rb.MovePosition(Rb.position + delta);
        }
        else
        {
        }

    }

    // Update is called once per frame
    void Update()
    {
        //  範囲外に出たら消す
        float py = transform.position.y;
        if (py < DestroyYMin)
        {
            Destroy(gameObject);
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
    //    {
    //        //  透明度を上げる（color.Alphaを下げる）
    //        Sr.color = new Color32(200, 200, 200, 125);
    //    }
    //}
}
