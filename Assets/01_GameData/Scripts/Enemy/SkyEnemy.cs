using UnityEngine;

public class SkyEnemy : MonoBehaviour
{
    [Header("移動関係")]

    //  移動の可否
    [SerializeField] private bool MoveBool;

    //  左右方向
    [SerializeField] private string MoveHori;

    //  移動スピード
    [SerializeField] private float MoveHoriSpeed;

    //  往復
    [Header("往復の可否")]
    [SerializeField] private bool RoundTrip;

    //  往復幅(全幅)
    //  Widthは幅
    [SerializeField] private float RoundTripWidth;

    [Header("消えるライン")]

    //  これ以下／以上で消す
    [SerializeField] private float DestroyXMin;
    [SerializeField] private float DestroyXMax;

    private Rigidbody2D Rb2d;
    private int dir;

    //  最初の地点
    private float startX;

    //  左右の移動量
    private float LeftBound;
    private float RightBound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rb2d = GetComponent<Rigidbody2D>();

        //  一応こちらでも重力の無視設定
        Rb2d.gravityScale = 0f;

        if (MoveHori == "左")
        {
            dir = -1;
        }
        else if (MoveHori == "右")
        {
            dir = 1;
        }
        else
        //  デフォは左
        { dir = -1; }

        //  往復範囲設定
        startX = transform.position.x;

        //  半分の幅
        float half = Mathf.Abs(RoundTripWidth) * 0.5f;

        //  左右
        LeftBound = startX - half;
        RightBound = startX + half;
    }

    //  GroundEnemyに記入済み
    //  rigid bodyはこっちのほうがいい
    private void FixedUpdate()
    {
        if(MoveBool != true)
        {
            return;
        }

        //  横移動
        Vector2 delta = Vector2.right * dir * MoveHoriSpeed * Time.deltaTime;
        Rb2d.MovePosition(Rb2d.position + delta);

        //  往復判定
        if(RoundTrip == true)
        {
            float x = Rb2d.position.x;

            if (x <= LeftBound && dir == -1)
            {
                ReverseDirection();
            }
            else if (x >= RightBound && dir == 1)
            {
                ReverseDirection();
            }
        }

        //  範囲外に出たら消す
        float py = transform.position.y;
        if (py < DestroyXMin || py > DestroyXMax)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //  方向の反転
    private void ReverseDirection()
    {
        dir = -dir;

        //  画像挿入したら見た目も反転
    }
}
