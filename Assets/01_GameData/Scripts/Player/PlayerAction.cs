using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
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

    [SerializeField] private float _shotDelay = 0.3f;   // 発射までの遅延
    [SerializeField] private float _shotCooldown = 0.5f;// 次の弾までのクールタイム

    //アニメーター
    [SerializeField] private Animator _animator;

    [SerializeField] private AudioSource _audioSource;

    //SE用
    public AudioSource SetGround;

    private SpriteRenderer SpriteRenderer;
    public Sprite SetWalkA;
    public Sprite SetWalkB;

    public AudioSource SetWalkSE;


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

    private PlayerHP _playerHP = null;

    private bool _goJump = false;

    private bool _wasGrounded = false;

    private bool _lastFacingRight = true;

    // 照準・弾管理用
    private Vector2 _lookInput = Vector2.zero;

    private bool _isShootingProcess = false; // 発射シーケンス中（遅延～クールタイム中）かどうか
    private bool _isCasting = false;//魔法詠唱中

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
        _playerHP = GetComponent<PlayerHP>();

        SetGround = GetComponent<AudioSource>();
        SetWalkSE = GetComponent<AudioSource>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        // ノックバック中は照準ロジックも停止させると、より安定します
        if (_playerHP != null && _playerHP.IsKnockingBack())
        {
            // ノックバック中はカーソルを強制的に非表示にする
            if (_aimCursor.gameObject.activeSelf)
            {
                _aimCursor.gameObject.SetActive(false);
            }
            return;
        }

        // プレイヤーが静止している(_dirの入力がない)時のみ表示

        bool isMoving = _dir.sqrMagnitude > 0.01f; // 移動入力があるかどうか
        bool hasLookInput = _lookInput.sqrMagnitude > 0.01f; // 照準入力があるかどうか

        // 「移動していない」かつ「照準入力がある」場合のみカーソル有効
        if (!isMoving && hasLookInput)
        {
            if (!_aimCursor.gameObject.activeSelf)
                _aimCursor.gameObject.SetActive(true);

            Vector3 aimDir = new Vector3(_lookInput.x, _lookInput.y, 0).normalized;
            _aimCursor.position = transform.position + aimDir * _aimRadius;
        }
        else
        {
            // 移動中、または入力なしの場合は非表示
            if (_aimCursor.gameObject.activeSelf)
                _aimCursor.gameObject.SetActive(false);
        }

        //  移動中、特定のスプライトになったらSEを鳴らす
        if (SpriteRenderer.sprite == SetWalkA || SpriteRenderer.sprite == SetWalkB)
        {
            SetWalkSE.Play();
        }
    }

    private void FixedUpdate()
    {
        // ノックバック中はプレイヤーの物理的な操作（移動・ジャンプ）をすべて無効にする
        if (_playerHP != null && _playerHP.IsKnockingBack())
        {
            return;
        }

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

    // 他のオブジェクトと接触している間の処理
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

    // 接触が終了したときの処理
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

        if (!_isCasting)
        {
            // 入力方向があるときに最後の向きを保存
            if (_dir.x > 0) _lastFacingRight = true;
            else if (_dir.x < 0) _lastFacingRight = false;

            // SpriteRendererで反転
            _sr.flipX = !_lastFacingRight;
        }

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
        if (_isCasting)
        {
            // 空中なら落下はしたいのでY軸の速度は維持し、X軸だけ0にする
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            return;
        }
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
            // 条件：静止中かつ照準が出ている(_aimCursorがアクティブ) かつ 発射プロセス中でない
            if (_aimCursor.gameObject.activeSelf && !_isShootingProcess)
            {
                _animator.SetTrigger("Magic");

                FaceToCursor();

                // コルーチンを開始して遅延射撃を行う
                StartCoroutine(FireRoutine());
            }
        }
    }
    private IEnumerator FireRoutine()
    {
        // 処理中フラグを立てる（これで連射を防ぐ）
        _isShootingProcess = true;

        _isCasting = true;

        // 1. 発射前の待機（0.5秒）
        yield return new WaitForSeconds(_shotDelay);

        // 待機中に移動してしまった場合などはキャンセルするならここでチェックを入れる
        // 今回は「発射したら」という仕様なのでそのまま発射処理へ

        Fire(); // 実際の弾生成処理

        _isCasting = false;

        // 2. 次の弾発射までのクールタイム（1.0秒）
        yield return new WaitForSeconds(_shotCooldown);

        // クールタイム終了、再発射可能に
        _isShootingProcess = false;
    }

    private void FaceToCursor()
    {
        // プレイヤーからカーソルへのベクトル
        float xDiff = _aimCursor.position.x - transform.position.x;

        // Xがプラスなら右(0~90, -90~0)、マイナスなら左(90~180, -180~-90)にある
        if (xDiff > 0)
        {
            _lastFacingRight = true;
            _sr.flipX = false; // 右向き（通常）
        }
        else if (xDiff < 0)
        {
            _lastFacingRight = false;
            _sr.flipX = true; // 左向き（反転）
        }
    }

    /// <summary>
    /// 着火
    /// </summary>
    private void Fire()
    {
        // プレイヤー自身の位置から発射
        Vector3 spawnPos = _tr.position + Vector3.up * 1f;

        GameObject bulletObj = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();

        // 照準カーソル方向へ発射
        // ※待機時間の0.5秒の間にカーソル位置が変わっていた場合、現在のカーソル位置に向かって飛びます
        Vector2 direction = (_aimCursor.position - spawnPos).normalized;
        rb.linearVelocity = direction * _bulletSpeed;
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
            SetGround.Play();

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
