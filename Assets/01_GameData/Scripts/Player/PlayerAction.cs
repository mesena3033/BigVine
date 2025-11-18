using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Audio;

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

    //ジャンプ力
    [SerializeField] private float _jumpPower = 90f;

    //ジャンプ可能なLayer
    [SerializeField] private LayerMask _groundLayer;

    // 通常時に使用する物理マテリアル
    [SerializeField] PhysicsMaterial2D normalMat;

    // 壁に接触したときに使用する摩擦なしの物理マテリアル
    [SerializeField] PhysicsMaterial2D noFrictionMat;

    // 弾発射関連 
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 10f; 
    [SerializeField] private Transform _aimCursor;
    [SerializeField] private float _aimRadius = 6f;

    //アニメーター
    [SerializeField] private Animator _animator;

    [SerializeField] private AudioSource _audioSource;


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
    private SpriteRenderer _sr = null;

    private bool _goJump = false;

    private bool _wasGrounded = false;

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
        _sr = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        // --- 右スティックで照準カーソルを制御 ---
        if (_lookInput.sqrMagnitude > 0.01f)
        {
            // 入力がある時だけカーソルを表示
            if (!_aimCursor.gameObject.activeSelf)
                _aimCursor.gameObject.SetActive(true);

            Vector3 aimDir = new Vector3(_lookInput.x, _lookInput.y, 0).normalized;
            _aimCursor.position = transform.position + aimDir * _aimRadius;
            // キャラの回転は行わず、スプライト切り替えだけで対応
        }
        else
        {
            // 入力がない時はカーソルを非表示
            if (_aimCursor.gameObject.activeSelf)
                _aimCursor.gameObject.SetActive(false);
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
        Set(_demoAct.ToggleBGM, OnToggleBGM);
        Set(_demoAct.ExitGame, OnExitGame);

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
        _dir = context.ReadValue<Vector2>();

        // 入力方向があるときに最後の向きを保存
        if (_dir.x > 0) _lastFacingRight = true;
        else if (_dir.x < 0) _lastFacingRight = false;

        // SpriteRendererで反転
        _sr.flipX = !_lastFacingRight;

        switch (context.phase)
        {
            case InputActionPhase.Started:
                _animator.SetTrigger("Run");
                break;

            case InputActionPhase.Canceled:
                _animator.SetTrigger("Idle");
                break;
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    private void Move()
    {
        _rb.linearVelocity = new Vector2(_dir.x * _moveSpeed, _rb.linearVelocity.y);
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
    {
        if (_bullets.Count < 3)
        {
            // プレイヤー自身の位置から発射
            Vector3 spawnPos = _tr.position + Vector3.up * 1f;

            GameObject bulletObj = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
            Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();

            // 照準カーソル方向へ発射
            Vector2 direction = (_aimCursor.position - spawnPos).normalized;
            rb.linearVelocity = direction * _bulletSpeed;

            _bullets.Add(bulletObj);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.OnDestroyed += b => _bullets.Remove(bulletObj);
            }
        }
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _goJump = true;

            _animator.SetTrigger("Jump");
        }
    }
    private void Jump()
    {
        if (_goJump && IsGrounded())
        {
            _rb.AddForce(Vector2.up * _jumpPower, ForceMode2D.Impulse);
            _goJump = false;
        }

        // 接地状態の変化を検知
        bool groundedNow = IsGrounded();
        if (groundedNow && !_wasGrounded)
        {
            // 今回新しく着地した瞬間だけ Idle/Run を発火
            if (_dir.x != 0)
            {
                _animator.SetTrigger("Run");
            }
            else
            {
                _animator.SetTrigger("Idle");
            }
        }

        _wasGrounded = groundedNow;
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
    private void OnToggleBGM(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
            else
            {
                _audioSource.Play();
            }
        }
    }
    private void OnExitGame(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Exit Game triggered");

            // エディタ上では停止、ビルド後はアプリ終了
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }

}
