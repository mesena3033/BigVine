using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerAction : MonoBehaviour
{
    private enum State
    {
        Enable,
        Disable
    }

    // ---------------------------- SerializeField
    // PlayerInputコンポーネントの参照
    [SerializeField] private PlayerInput _input;

    // 移動速度,加速度
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float acceleration = 10f;

    //ジャンプ力
    [SerializeField] private float _jumpPower = 90f;

    //ジャンプ可能なLayer
    [SerializeField] private LayerMask _groundLayer;

    // 通常時に使用する物理マテリアル
    [SerializeField] PhysicsMaterial2D normalMat;

    // 壁に接触したときに使用する摩擦なしの物理マテリアル
    [SerializeField] PhysicsMaterial2D noFrictionMat;

    // 弾発射関連 
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f; 
    [SerializeField] private Transform firePoint; 
    [SerializeField] private Transform aimCursor;
    [SerializeField] private float aimRadius = 6f;

    //アニメーター
    [SerializeField] private Animator animator; 

    // ---------------------------- Field

    // 入力を受け取るための変数
    private DemoAction _act;
    private DemoAction.PlayerActions _demoAct;

    // 入力を保存するための変数
    private Vector2 _dir = Vector2.zero;

    // その他、処理に必要な変数
    private Transform _tr = null;
    private Rigidbody2D _rb = null;
    private Collider2D _col = null;


    private bool _goJump = false;

    private bool _lastFacingRight = true;

    // 照準・弾管理用
    private Vector2 _lookInput = Vector2.zero;
    private List<GameObject> _bullets = new List<GameObject>();

    // ---------------------------- UnityMessage
    private void Awake()
    {
        // 入力アクションの初期化
        _act = new DemoAction();
        _demoAct = _act.Player;
        _col = new Collider2D();
    }

    private void Start()
    {
        // コンポーネントの取得
        _tr = transform;
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

    }
    private void Update()
    {
        // --- 右スティックで照準カーソルを制御 ---
        if (_lookInput.sqrMagnitude > 0.01f)
        {
            // 入力がある時だけカーソルを表示
            if (!aimCursor.gameObject.activeSelf)
                aimCursor.gameObject.SetActive(true);

            Vector3 aimDir = new Vector3(_lookInput.x, _lookInput.y, 0).normalized;
            aimCursor.position = transform.position + aimDir * aimRadius;
            // キャラの回転は行わず、スプライト切り替えだけで対応
        }
        else
        {
            // 入力がない時はカーソルを非表示
            if (aimCursor.gameObject.activeSelf)
                aimCursor.gameObject.SetActive(false);
        }

    }

    private void FixedUpdate()
    {
        // 移動処理の呼び出し
        Move();
        Jump();
    }

    private void OnEnable()
    {
        // 入力アクションの有効化とイベント登録
        _act?.Enable();
        ChangeAct(State.Enable);
    }

    private void OnDisable()
    {
        // 入力アクションの無効化とイベント解除
        ChangeAct(State.Disable);
        _act?.Disable();
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
                _col.sharedMaterial = normalMat;
                return;
            }
            else if (Mathf.Abs(Vector2.Angle(contact.normal, Vector2.left)) < 30f ||
                     Mathf.Abs(Vector2.Angle(contact.normal, Vector2.right)) < 30f)
            {
                _col.sharedMaterial = noFrictionMat;
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
        _col.sharedMaterial = normalMat;
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// アクションの変更・登録・解除
    /// </summary>
    /// <param name="state">変更方法</param>
    private void ChangeAct(State state)
    {
        // 入力機器の変更
        switch (state)
        {
            case State.Enable:
                _input.onControlsChanged += input => OnControlsChanged();
                break;

            case State.Disable:
                _input.onControlsChanged -= input => OnControlsChanged();
                break;
        }

        // 入力アクションとイベントハンドラの紐付け・解除
        Set(_demoAct.Move, OnMove);
        Set(_demoAct.Fire, OnFire);
        Set(_demoAct.Jump, OnJump);
        Set(_demoAct.Look, OnLook);


        // 引数によって処理を分岐
        void Set(InputAction input, Action<InputAction.CallbackContext> act)
        {
            switch (state)
            {
                case State.Enable:
                    input.started += act;
                    input.performed += act;
                    input.canceled += act;
                    break;

                case State.Disable:
                    input.started -= act;
                    input.performed -= act;
                    input.canceled -= act;
                    break;
            }
        }
    }

    /// <summary>
    /// 入力機器が変更された時の処理
    /// </summary>
    private void OnControlsChanged()
    {
        // 現在のコントロールスキームをログに表示
        // スキームから string に変更して入力機器を判別することが可能
        Debug.Log($"Control scheme changed:{_input.currentControlScheme}");
    }

    /// <summary>
    /// 移動の入力を受け取る
    /// </summary>
    /// <param name="context">コンテキスト</param>
    private void OnMove(InputAction.CallbackContext context)
    {
        // 入力値を保存
        _dir = context.ReadValue<Vector2>();
        // 入力方向があるときに最後の向きを保存
        if (_dir.x > 0) _lastFacingRight = true;
        else if (_dir.x < 0) _lastFacingRight = false;

        var which = _dir.x > 0;
        switch (context.phase)
        {
            case InputActionPhase.Started:
                if(which)
                {
                    animator.SetTrigger("RunRight");
                }
                else
                {
                    animator.SetTrigger("RunLeft");
                }
                    break;

            case InputActionPhase.Canceled:
                // Idle時は最後の向きに応じて待機モーション
                if (_lastFacingRight)
                {
                    animator.SetTrigger("IdleRight");
                }
                else
                {
                    animator.SetTrigger("IdleLeft");
                }

                break;
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        // 保存された入力値に基づいて移動処理を実装
        float targetX = _dir.x * _moveSpeed;

        // 現在速度を徐々に目標速度へ近づける
        float newX = Mathf.Lerp(_rb.linearVelocity.x, targetX, Time.fixedDeltaTime * acceleration);

        _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);

    }

    /// <summary>
    /// 着火の入力を受け取る
    /// </summary>
    /// <param name="context">コンテキスト</param>
    private void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Fire();
        }
    }

    /// <summary>
    /// 着火
    /// </summary>
    private void Fire()
    {// 3発未満なら撃てる
        if (_bullets.Count < 3)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            Vector2 direction = (aimCursor.position - firePoint.position).normalized;
            rb.linearVelocity = direction * bulletSpeed;

            _bullets.Add(bullet); // リストに追加

            Destroy(bullet, 1.5f);
            StartCoroutine(RemoveBulletAfterDelay(bullet, 1.5f));
        }

    }
    // 弾リストから削除するコルーチン
    private System.Collections.IEnumerator RemoveBulletAfterDelay(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        _bullets.Remove(bullet);
    }


    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _goJump = true;
        }
    }
    private void Jump()
    {
        if (_goJump && IsGrounded())
        {
            _rb.AddForce(Vector2.up * _jumpPower, ForceMode2D.Impulse);
            _goJump = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.CircleCast(
            _tr.position,
            0.6f,
            Vector2.down,
            0.5f,
            _groundLayer
        );
    }
    private void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

}
