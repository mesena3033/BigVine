using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ---------------------------- Field

    // プレイヤーの物理挙動を制御する Rigidbody2D コンポーネント
    Rigidbody2D rbody;

    // プレイヤーの当たり判定を持つ Collider2D コンポーネント
    Collider2D col;

    // 横方向の入力値（-1:左, 0:停止, 1:右）
    float axisH = 0.0f;

    // プレイヤーの移動速度
    public float Speed = 3.0f;

    // ジャンプの力（インパルス）
    public float JumpPw = 9.0f;

    // 地面判定に使用するレイヤーマスク
    public LayerMask groundLayer;

    // ジャンプ入力があったかどうかのフラグ
    bool goJump = false;

    // 通常時に使用する物理マテリアル
    [SerializeField] PhysicsMaterial2D normalMat;

    // 壁に接触したときに使用する摩擦なしの物理マテリアル
    [SerializeField] PhysicsMaterial2D noFrictionMat;


    // ---------------------------- UnityMessage

    /// <summary>
    /// 初期化処理。必要なコンポーネントを取得する
    /// </summary>
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    /// <summary>
    /// 毎フレーム呼ばれる処理。入力の取得や向きの変更を行う
    /// </summary>
    void Update()
    {
        axisH = Input.GetAxisRaw("Horizontal");

        if (axisH > 0.0f)
        {
            transform.localScale = new Vector2(1, 1);
        }
        else if (axisH < 0.0f)
        {
            transform.localScale = new Vector2(-1, 1);
        }

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    /// <summary>
    /// 固定フレームごとに呼ばれる物理演算処理。
    /// 地面判定と移動・ジャンプ処理を行う
    /// </summary>
    private void FixedUpdate()
    {
        bool onGround = Physics2D.CircleCast(
            transform.position,
            0.6f,
            Vector2.down,
            0.5f,
            groundLayer
        );

        rbody.linearVelocity = new Vector2(Speed * axisH, rbody.linearVelocity.y);

        if (onGround && goJump)
        {
            rbody.AddForce(Vector2.up * JumpPw, ForceMode2D.Impulse);
            goJump = false;
        }
    }

    /// <summary>
    /// 他のオブジェクトと接触している間の処理。
    /// 接触面の角度に応じて物理マテリアルを切り替える
    /// </summary>
    /// <param name="collision">接触しているオブジェクトの情報</param>
    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector2.Angle(contact.normal, Vector2.up) < 30f)
            {
                col.sharedMaterial = normalMat;
                return;
            }
            else if (Mathf.Abs(Vector2.Angle(contact.normal, Vector2.left)) < 30f ||
                     Mathf.Abs(Vector2.Angle(contact.normal, Vector2.right)) < 30f)
            {
                col.sharedMaterial = noFrictionMat;
                return;
            }
        }
    }

    /// <summary>
    /// 接触が終了したときの処理。
    /// 物理マテリアルを通常状態に戻す
    /// </summary>
    /// <param name="collision">接触が終了したオブジェクトの情報</param>
    void OnCollisionExit2D(Collision2D collision)
    {
        col.sharedMaterial = normalMat;
    }


    // ---------------------------- PublicMethod

    /// <summary>
    /// ジャンプ入力を受け付ける
    /// </summary>
    public void Jump()
    {
        goJump = true;
    }
}