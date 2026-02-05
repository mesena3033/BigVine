using TMPro;
using Unity.VisualScripting;
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

    //  ムカデのY座標
    float yPos;

    //  Playerとの距離測定用
    float distance;
    float PlayerPosition, EnemyPosition;

    //  ムカデの速度戻すよう
    bool ReMove = false;
    float ReSpeed;

    bool Count;
    float CountTime;
    float LimitTime = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        Sr = GetComponent<SpriteRenderer>();

        //  現在の座標を取得
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
    }

    // Update is called once per frame
    void Update()
    {
        //  範囲外に出たら消す
        yPos = transform.position.y;
        EnemyPosition = yPos;

        if (yPos < DestroyYMin) //  下
        {
            Destroy(gameObject);
        }
        else if(yPos > DestroyYMax) //  上
        {
            //Destroy(gameObject);
        }

        if (ReMove == true)
        {
            //  一定時間下降し続ける
            if (Count == true)
            {
                MoveHoriSpeed = ReSpeed;

                ReMove = false;
                Count = false;
                CountTime = 0;
            }
            else
                CountTime += Time.deltaTime;

            if (CountTime >= LimitTime) Count = true;
        }
    }

    //  Playerとの当たり判定
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //  Playerとの距離測定
            EnemyPosition = this.transform.position.y;
            PlayerPosition = collision.transform.position.y;

            distance = EnemyPosition - PlayerPosition;

            if(distance < 0) distance = 0;

            ReSpeed = MoveHoriSpeed;
            MoveHoriSpeed = -(distance + 5.0f);

            //  戻る、と時間の測定開始
            ReMove = true;
            Count = false;
        }
    }
}
