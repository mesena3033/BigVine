using UnityEngine;

public class EnemyBulletManager : MonoBehaviour
{
    //  エネミーの弾
    [SerializeField] private GameObject Bullet;

    //  弾を撃つ条件
    //  むずい

    //  弾を撃つインターバル
    [SerializeField] private float BulletInterval;

    //  追尾精度
    [Header("追尾精度(中高未実装)")]
    [Tooltip("追尾精度を設定（低/中/高）")]
    [SerializeField] private string ShootingControll;   //  低中高

    //  弾の移動方向
    [Header("移動方向")]
    //  追尾精度が低のときのみ使用
    [SerializeField] private string VerMove;    //  上下
    [SerializeField] private string HoriMove;   //  左右

    //  弾の情報
    private int BulletXMove;
    private int BulletYMove;

    private Vector2 BulletVer;
    private Quaternion BulletRotation;

    //  射撃間隔計算
    float TimeCount = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ShootingControll == "低")
        {
            //  移動方向決定
            if (VerMove == "上")
            {
                BulletYMove = 1;
            }
            else if (VerMove == "下")
            {
                BulletYMove = -1;
            }
            else
            {
                BulletYMove = 0;
            }

            if (HoriMove == "左")
            {
                BulletXMove = -1;
            }
            else if (HoriMove == "右")
            {
                BulletXMove = 1;
            }
            else
            {
                BulletXMove = 0;
            }

            //  上下左右を Vector2 に変換
            BulletVer = new Vector2(BulletXMove, BulletYMove);

            //  Vector2 から角度を計算
            float BulletAngle = Mathf.Atan2(BulletVer.y, BulletVer.x) * Mathf.Rad2Deg;

            //  z軸を回転させるため Quaternion を生成
            BulletRotation = Quaternion.Euler(0, 0, BulletAngle);
        }
        else if (ShootingControll == "中")
        {
            //  未定
        }
        else if (ShootingControll == "高")
        {
            //  未定
        }
    }

    // Update is called once per frame
    void Update()
    {
        //  カウントが BulletInterval に達したら撃つ！
        if (TimeCount >= BulletInterval)
        {
            Instantiate(Bullet, transform.position, BulletRotation);

            TimeCount -= BulletInterval;
        }
        else
        {
            TimeCount += Time.deltaTime;
        }
    }
}
