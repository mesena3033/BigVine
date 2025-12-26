using Unity.Cinemachine;
using UnityEngine;

public class SkyEnemy : MonoBehaviour
{
    [Header("移動関係")]

    //  移動の可否
    [SerializeField] private bool MoveBool;

    //  左右方向
    [SerializeField] private string Move;
    private string MoveXY;

    //  移動スピード
    [SerializeField] private float MoveSpeed;

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
    private float startPos;

    //  左右の移動量
    private float LeftBound;
    private float RightBound;

    private float UpBound;
    private float DownBound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float RoundTripHeight = RoundTripWidth;

        Rb2d = GetComponent<Rigidbody2D>();

        //  一応こちらでも重力の無視設定
        Rb2d.gravityScale = 0f;

        if (Move == "左")
        {
            MoveXY = "x";
            dir = -1;
        }
        else if (Move == "右")
        {
            MoveXY = "x";
            dir = 1;

            //  右に向く
            transform.localScale = new Vector2(-dir, 1);
        }
        else if (Move == "上")
        {
            MoveXY = "y";
            dir = 1;
        }
        else if (Move == "下")
        {
            MoveXY = "y";
            dir = -1;
        }
        else
        //  デフォは左
        { dir = -1; }

        //  往復範囲設定
        if (MoveXY == "x")
        {
            startPos = transform.position.x;

            //  半分の幅
            float half = Mathf.Abs(RoundTripWidth) * 0.5f;

            //  左右
            LeftBound = startPos - half;
            RightBound = startPos + half;
        }
        else if (MoveXY == "y")
        {
            startPos = transform.position.y;

            //  半分の高さ
            float half = Mathf.Abs(RoundTripHeight) * 0.5f;

            //  上下
            UpBound = startPos + half;
            DownBound = startPos - half;
        }
    }

    //  GroundEnemyに記入済み
    //  rigid bodyはこっちのほうがいい
    private void FixedUpdate()
    {
        if(MoveBool != true)
        {
            return;
        }

        if (MoveXY == "x")
        {
            //  横移動
            Vector2 deltaX = Vector2.right * dir * MoveSpeed * Time.deltaTime;
            Rb2d.MovePosition(Rb2d.position + deltaX);

            //  往復判定
            if (RoundTrip == true)
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
        }
        else if (MoveXY == "y")
        {
            //  縦移動
            Vector2 deltaY = Vector2.up * dir * MoveSpeed * Time.deltaTime;
            Rb2d.MovePosition(Rb2d.position +  deltaY);

            //  往復判定
            if (RoundTrip == true)
            {
                float y = Rb2d.position.y;

                if(y >= UpBound && dir == 1)
                {
                    ReverseDirection();
                }
                else if(y <= DownBound && dir == -1)
                {
                    ReverseDirection();
                }
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

    //  外部から自律移動を開始させるためのメソッド
    public void ActivateAIMovement(string initialMoveDirection)
    {
        //  移動方向を設定
        Move = initialMoveDirection;

        //  移動フラグを立てる
        MoveBool = true;
    }

    //  方向の反転
    private void ReverseDirection()
    {
        dir = -dir;

        if (MoveXY == "x")
        {
            //  画像挿入したら見た目も反転
            transform.localScale = new Vector2(-dir, 1);
        }
    }
}
