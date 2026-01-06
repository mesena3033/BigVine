using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.InputSystem.Controls;

public class GroundEnemy : MonoBehaviour
{
    [Header("移動関係")]

    //  移動の可否
    [SerializeField] private bool MoveBool;

    //  左右方向
    [SerializeField] private string MoveHori;

    //  移動スピード
    [SerializeField] private float MoveHoriSpeed;

    [Header("消えるライン")]

    //  これ以下／以上で消す
    [SerializeField] private float DestroyYMin;
    [SerializeField] private float DestroyYMax;

    [SerializeField] private bool Mukade;
    int eSize = 1;

    private Rigidbody2D Rb2d;
    private float DefaultGravityScale;
    
    //  方向
    private int dir;

    //  接地判定
    //  タグ"Ground"との接触
    private HashSet<Collider2D> GroundContact = new HashSet<Collider2D>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rb2d = GetComponent<Rigidbody2D>();
        if(Mukade == true)
        {
            eSize = 2;
        }

        //  方向代入
        if (MoveHori == "左")
        {
            //  Groundで反転するため逆にする
            dir = eSize;
        }
        else if (MoveHori == "右")
        {
            dir = -eSize;

            //  右に向く
            transform.localScale = new Vector2(-dir, 1);
        }
        else
            //  デフォは左
            { dir = -eSize; }

        DefaultGravityScale = Rb2d.gravityScale;
    }

    //  一定間隔で呼び出されるUpdate
    //  RigidBodyはこちらのほうがよい
    private void FixedUpdate()
    {
        //  接地判定より、地面についているとき重力無効化
        Rb2d.gravityScale = (GroundContact.Count == 0) ? DefaultGravityScale : 0f;

        if (MoveBool != true)
        {
            return;
        }

        //  接地時のみ横移動の制御
        if(GroundContact.Count > 0)
        {
            Vector2 delta = Vector2.right * dir * MoveHoriSpeed * Time.deltaTime;
            Rb2d.MovePosition(Rb2d.position + delta);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //  範囲外に出たら消す
        float py = transform.position.y;
        if (py < DestroyYMin || py > DestroyYMax)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //接地判定の肯定
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            GroundContact.Add(other.collider);
        }

        //  Groundに衝突時移動方向を反転する
        if ((other.gameObject.layer == LayerMask.NameToLayer("Ground")) ||
            (other.gameObject.layer == LayerMask.NameToLayer("Enemy")))
        {
            dir *= -1;  //  方向を反転

            //  画像反転
            transform.localScale = new Vector2(-dir, eSize);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        //  接地判定の否定
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            GroundContact.Remove(other.collider);
        }
    }

}
