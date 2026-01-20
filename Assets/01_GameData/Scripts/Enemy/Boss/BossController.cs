using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    // ボスの形態を定義するenum
    private enum BossForm
    {
        Form1, // 第一形態
        Form2, // 第二形態
        Form3  // 最終形態
    }
    private BossForm currentForm;

    // 形態ごとのパラメータ倍率
    private float cooldownMultiplier = 1.0f; // クールタイムに乗算（値が大きいほど長くなり、簡単になる）
    private float speedMultiplier = 1.0f;    // スピードに乗算（値が小さいほど遅くなり、簡単になる）

    // 攻撃の種類を定義
    private enum BossAttackType
    {
        AcidSpit,       // 毒液(地上)
        CeilingAcid,    // 毒液(天井)
        VineHorizontal, // ツタ(横)
        VineVertical,   // ツタ(縦)
        Charge,         // 突進
        DiveBomb,       // 急降下
        FlySummon,      // ハエ呼び寄せ
        Bite            // 噛みつき(画面奥からの範囲攻撃)
    }

    [Header("ギミック制御 (シーン上のオブジェクトを登録)")]
    [SerializeField] private GameObject[] bombFlowers; // 爆弾花 (StickyBombLauncher)
    [SerializeField] private GameObject[] flyTraps;    // ハエトリ (PlantTrap)
    [SerializeField] private GameObject[] rockFalls;   // 落石 (RockFall)

    [Header("ステータス")]
    [SerializeField] private int maxHp = 10;
    private int currentHp;

    [Header("UI連携用イベント")]
    // HPが変更されたときに呼び出すイベント
    public UnityEvent<int, int> OnHPChanged;

    [Header("イベント設定")]
    [Tooltip("イベント中に無効化する足場のコライダー")]
    [SerializeField] private Collider2D[] temporaryPlatforms;
    [Tooltip("プレイヤーの入力スクリプト")]
    [SerializeField] private PlayerAction playerAction;
    [Tooltip("吹き出しUIのプレハブ")]
    [SerializeField] private GameObject speechBubblePrefab;
    [Tooltip("変身前の初期スプライト")]
    [SerializeField] private Sprite initialFormSprite;
    [Tooltip("プレイヤーが最初に落下してくる位置")]
    [SerializeField] private Transform playerFallStartPoint;

    private bool isEventRunning = true; // イベント実行中フラグ
    private SpeechBubbleController speechBubbleInstance;

    private Sprite originalBodySprite; // 元のスプライトを保存するための変数

    [Header("攻撃設定: 共通")]
    [SerializeField] private Transform firePoint; // 弾の発射位置

    [Header("攻撃設定: 突進 (25%)")]
    [Header("攻撃設定: 突進 (25%)")]
    [SerializeField] private GameObject warningAreaPrefab; // 突進の警告エフェクト（赤い四角）
    [SerializeField] private float chargeWarningTime = 3.0f; // 突進の警告表示時間
    [SerializeField] private float blinkInterval = 0.2f;     // 警告の点滅間隔（秒）
    public float chargeSpeed = 10f;     // 廃止（chargeThroughSpeed に統合）
    public float chargeDuration = 1.5f; // 廃止（画面外に出るまで突進するため）
    public float returnSpeed = 5f;
    [Tooltip("画面を駆け抜ける際の突進速度")]
    [SerializeField] private float chargeThroughSpeed = 25f; // 新しい突進速度
    [Tooltip("突進時のカメラシェイクの強さ")]
    [SerializeField] private float shakeMagnitude = 0.5f;
    [Tooltip("突進時のカメラシェイクの時間")]
    [SerializeField] private float shakeDuration = 12.0f;

    // 独立ループ
    [Header("攻撃設定: ツタ (独立ループ)")]
    [SerializeField] private GameObject vinePrefab;
    [SerializeField] private float vineWarningTime = 1.0f;
    [Tooltip("ツタ攻撃を独立して実行する間隔（秒）")]
    [SerializeField] private float vineAttackInterval = 6.0f; // 6秒ごとなど

    [Header("攻撃設定: ハエ呼び寄せ (独立ループ)")]
    [SerializeField] private GameObject flyPrefab; // ハエのプレハブ
    [SerializeField] private float flySummonInterval = 20.0f; // 呼び寄せの間隔
    [SerializeField] private GameObject sonicWaveEffectPrefab; // 頭から出る音波エフェクトのプレハブ
    [SerializeField] private AudioClip flySummonSound; // 呼び寄せSE
    [SerializeField] private int numberOfFlies = 3; // 呼び出すハエの数
    [SerializeField] private float flyMoveSpeed = 5f; // ハエが目標地点まで移動する速度
    [SerializeField] private Vector2 flySpawnOffset = new Vector2(2.0f, 0); // 画面端からのオフセット
    [Tooltip("ハエを1匹食べた時のHP回復量")]
    [SerializeField] private int flyHealAmount = 1;

    [Header("攻撃設定: 溶解液 (35%)")]
    [SerializeField] private GameObject acidBulletPrefab; // 弾のプレハブ
    [SerializeField] private float throwForce = 10f;      // 投げる強さ
    [SerializeField] private float throwInterval = 0.5f;  // 連射間隔

    [Header("攻撃設定: 天井からの毒液")]
    [Tooltip("一度に降らせる毒液の弾の数")]
    [SerializeField] private int acidRainShots = 20;
    [Tooltip("毒液の弾を発射する間隔")]
    [SerializeField] private float acidRainInterval = 0.1f;
    [Tooltip("天井攻撃の滞在時間")]
    [SerializeField] private float ceilingStayDuration = 4.0f; // 新しい変数
    [Tooltip("毒液の弾が左右にばらける度合い")]
    [SerializeField] private float acidRainSpreadForce = 8f;
    [Tooltip("ボスが天井に出現/消失するアニメーションの時間")]
    [SerializeField] private float ceilingAnimDuration = 1.0f;
    [Header("対空行動設定")]
    [Tooltip("プレイヤーが高所にいると判断するY座標の閾値")]
    [SerializeField] private float highPlayerYThreshold = 12.0f;
    [Tooltip("対空攻撃（天井溶解液）を連続で行う際のクールダウン時間")]
    [SerializeField] private float antiAirAttackCooldown = 4.0f;

    [Header("溶解液攻撃: 警告")]
    [SerializeField] private SpriteRenderer headRenderer;           // 点滅させる頭のSpriteRenderer
    [SerializeField] private Color acidWarningColor = Color.red;   // 警告色
    [SerializeField] private float acidWarningTime = 1.0f;         // 警告の表示時間
    [SerializeField] private float acidBlinkInterval = 0.2f;       // 点滅間隔

    [Header("攻撃設定: 大技分岐")]
    [SerializeField] private float diveBombChance = 0.55f; // 押しつぶし攻撃を行う確率 (55%)

    [Header("攻撃設定: 急降下爆撃 (別枠30%)")]
    [SerializeField] private float diveFollowTime = 2.0f;    // 警告がプレイヤーを追従する時間
    [SerializeField] private float diveFastBlinkTime = 1.0f; // 警告が高速点滅する時間
    [SerializeField] private float diveFallSpeed = 40f;      // ボスが上から落ちてくる速度
    [SerializeField] private float diveStunTime = 2.0f;      // 着地後の行動不能時間
    [Tooltip("急降下攻撃の着地時のカメラシェイクの強さ")]
    [SerializeField] private float diveShakeMagnitude = 0.8f; 
    [Tooltip("急降下攻撃の着地時のカメラシェイクの時間")]
    [SerializeField] private float diveShakeDuration = 0.4f;
    [SerializeField] private float submergeAnimTime = 1.0f; // 地面に潜る/戻るアニメーションの時間

    [Header("攻撃設定: 画面奥からの範囲攻撃")]
    [SerializeField] private GameObject backgroundAttackFacePrefab;
    [SerializeField] private GameObject rangeAttackHitboxPrefab;      // 攻撃判定のプレハブ
    [SerializeField] private float zoomInDuration = 1.5f;      // 顔が奥から迫ってくる時間
    [SerializeField] private float rangeWarningTime = 2.0f;    // 警告線の表示時間
    [SerializeField] private float attackTravelTime = 0.2f;    // 攻撃判定が中心まで到達する時間
    [SerializeField] private int numberOfAttacks = 6;          // 攻撃の数（警告線の数）
    [SerializeField] private float attackRadius = 15f;         // 攻撃が開始される半径（警告線の長さ）

    [Header("ステージ範囲制限")]
    public float minX = -16.0f;
    public float maxX = 16.0f;
    public float maxY = 12.0f;
    public float minY = -4.0f;

    [Header("基準点設定")]
    [SerializeField] private GameObject returnParticlePrefab; // 戻る場所を示すパーティクル
    [SerializeField] private Vector2 standbyPositionOffset = new Vector2(2, 0); // 画面端からのオフセット

    [Header("参照: 形態ごとのパーツ")]
    [SerializeField] private GameObject bodyForm1;
    [Tooltip("第一形態の吹き出しを表示する位置")]
    [SerializeField] private Transform speechBubbleAnchorForm1;
    [SerializeField] private GameObject bodyForm2;
    [SerializeField] private GameObject headForm2;
    [SerializeField] private GameObject bodyForm3;
    [SerializeField] private GameObject headForm3;

    [Header("参照")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer[] bodyPartsRenderers;

    [Header("参照: カメラ制御")]
    // CameraShakerの代わりにImpulseを発生させるコンポーネント
    [SerializeField] private CinemachineImpulseSource impulseSource;
    // 新しいカメラ優先度コントローラー
    [SerializeField] private CameraPriorityController cameraPriorityController;

    [Header("参照: 頭の制御用")]
    [SerializeField] private BossHead bossHead; // BossHeadスクリプトをインスペクターから設定

    [Header("効果音")]
    [SerializeField] private AudioSource audioSource; // 音を再生するコンポーネント
    [SerializeField] private AudioClip damageSound;   // ダメージを受けた時のSE
    [SerializeField] private AudioClip deathSound;  // 倒された時のSE
    [SerializeField] private AudioClip acidSpitSound; // 溶解液を発射する時のSE
    [SerializeField] private AudioClip diveLandSound; // 急降下で着地した時のSE
    [SerializeField] private AudioClip chargeStartSound; // 突進を開始する時のSE
    [SerializeField] private AudioClip returnSound; // 地面から帰ってくる時のSE

    private Transform playerTransform;
    private bool isDead = false;
    private Vector3 initialPosition;
    private Vector3 currentTargetPosition; // 現在の目標待機位置
    private Vector3 initialHeadScale; // 顔の初期スケールを保存する変数
    private Vector3 initialHeadLocalPosition;

    private bool canFacePlayer = true; // プレイヤーの方向を向くかどうかを制御するフラグ

    void Start()
    {
        // プレイヤー参照の取得
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            // PlayerActionコンポーネントも取得
            if (playerAction == null) playerAction = playerObj.GetComponent<PlayerAction>();
        }

        // --- イベント用セットアップ ---
        // 吹き出しを最初に一度だけ生成し、非表示にしておく
        if (speechBubblePrefab != null)
        {
            GameObject bubbleObj = Instantiate(speechBubblePrefab, transform.position, Quaternion.identity);
            speechBubbleInstance = bubbleObj.GetComponent<SpeechBubbleController>();
            speechBubbleInstance.Hide();
        }

        // --- 形態の決定 ---
        if (GameManager.Instance != null)
        {
            int growthCount = GameManager.Instance.growthMagicCount;
            if (growthCount <= 10)
            {
                currentForm = BossForm.Form1;
                cooldownMultiplier = 1.6f; // クールタイムが1.6倍（長くなる）
                speedMultiplier = 0.65f;  // スピードが0.65倍（遅くなる）
                Debug.Log("ボスは第一形態で出現します。");
            }
            else if (growthCount <= 18)
            {
                currentForm = BossForm.Form2;
                cooldownMultiplier = 1.6f; // クールタイムが1.6倍
                speedMultiplier = 0.65f;   // スピードが0.65倍
                Debug.Log("ボスは第二形態で出現します。");
            }
            else
            {
                currentForm = BossForm.Form3;
                cooldownMultiplier = 1.0f; // 基準
                speedMultiplier = 1.0f;   // 基準
                Debug.Log("ボスは最終形態で出現します。");
            }
        }
        else
        {
            // GameManagerが見つからない場合は最終形態で動作
            currentForm = BossForm.Form3;
            Debug.LogWarning("GameManagerが見つからないため、ボスは最終形態で出現します。");
        }

        // --- 形態に応じてHPを設定 ---
        switch (currentForm)
        {
            case BossForm.Form1:
                maxHp = 15;
                break;
            case BossForm.Form2:
                maxHp = 20;
                break;
            case BossForm.Form3:
                maxHp = 30;
                break;
        }

        // --- 形態に応じて見た目と参照を切り替える ---
        InitializeBossForm();

        // 形態に応じてギミックの出現を制御する
        SetupGimmicks();

        // イベントシーケンスを開始する
        StartCoroutine(StartOpeningEvent());

        // --- 形態に応じてパラメータを調整 ---
        // 突進
        chargeWarningTime *= cooldownMultiplier;
        chargeThroughSpeed *= speedMultiplier;
        returnSpeed *= speedMultiplier;

        // ツタ (独立ループ)
        vineAttackInterval *= cooldownMultiplier;
        vineWarningTime *= cooldownMultiplier;

        // ハエ呼び寄せ (独立ループ)
        flySummonInterval *= cooldownMultiplier;
        flyMoveSpeed *= speedMultiplier;

        // 溶解液 (投擲)
        throwInterval *= cooldownMultiplier; // 連射間隔は長いほど簡単になる
        acidWarningTime *= cooldownMultiplier;

        // 天井からの毒液
        acidRainInterval *= cooldownMultiplier; // 弾の発射間隔は長いほど簡単になる
        ceilingStayDuration *= cooldownMultiplier;

        // 対空行動
        antiAirAttackCooldown *= cooldownMultiplier;

        // 急降下爆撃
        diveFollowTime *= cooldownMultiplier;
        diveFastBlinkTime *= cooldownMultiplier;
        diveFallSpeed *= speedMultiplier;
        diveStunTime *= cooldownMultiplier;

        // 画面奥からの範囲攻撃
        zoomInDuration /= speedMultiplier; // 時間は長いほど簡単になる（speedMultiplierは1未満なので割る）
        rangeWarningTime *= cooldownMultiplier;
        attackTravelTime /= speedMultiplier; // 時間は長いほど簡単になる

        currentHp = maxHp;
        initialPosition = transform.position;

        // 最初の目標待機位置は、初期位置と同じ場所にする
        currentTargetPosition = initialPosition;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // firePointが設定されていなければ自分の位置にする
        if (firePoint == null) firePoint = transform;

        // 顔の初期スケールを保存
        if (headRenderer != null)
        {
            initialHeadScale = headRenderer.transform.localScale;
            initialHeadLocalPosition = headRenderer.transform.localPosition;
        }

        StartCoroutine(BossBehaviorLoop());
        StartCoroutine(VineAttackLoop());
        StartCoroutine(FlySummonLoop());

        // ゲーム開始時に、自分にアタッチされているImpulseSourceを自動で取得
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        // カメラ優先度コントローラーがインスペクターで設定されていなければ探す
        if (cameraPriorityController == null && Camera.main != null)
        {
            cameraPriorityController = Camera.main.GetComponent<CameraPriorityController>();
        }
        if (cameraPriorityController == null)
        {
            Debug.LogError("シーン内にCameraPriorityControllerが見つかりません！");
        }

        // ゲーム開始時にUIに初期HPを通知するためにイベントを発行
        if (OnHPChanged != null)
        {
            OnHPChanged.Invoke(currentHp, maxHp);
        }
    }

    void LateUpdate()
    {
        // isDeadでなく、プレイヤーがいて、向き変更が許可されている場合のみプレイヤーの方向を向く
        if (!isDead && playerTransform != null && canFacePlayer)
        {
            FacePlayer();
        }
    }

    // 形態ごとのギミック有効/無効設定
    private void SetupGimmicks()
    {
        // 一旦すべて有効にする（または初期状態に任せる）

        // 第一形態: [爆弾花、ハエトリ、落石] を無効にする
        if (currentForm == BossForm.Form1)
        {
            SetObjectsActive(bombFlowers, false);
            SetObjectsActive(flyTraps, false);
            SetObjectsActive(rockFalls, false);
        }
        // 第二形態: [落石] を無効にする
        else if (currentForm == BossForm.Form2)
        {
            SetObjectsActive(rockFalls, false);
            // 爆弾花、ハエトリは有効のまま
        }
        // 第三形態: すべて有効 (何もしない)
    }

    // ヘルパー関数: 配列内のオブジェクトを一括でSetActive
    private void SetObjectsActive(GameObject[] objects, bool isActive)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            if (obj != null) obj.SetActive(isActive);
        }
    }

    // 攻撃の使用可否判定ロジック
    private bool CanUseAttack(BossAttackType type)
    {
        // 現在のHP率 (0.0 ～ 1.0)
        float hpRate = (float)currentHp / maxHp;

        switch (currentForm)
        {
            case BossForm.Form1:
                // 第一形態の仕様
                if (type == BossAttackType.AcidSpit) return true;
                if (type == BossAttackType.CeilingAcid) return true; // 100%開放
                if (type == BossAttackType.VineHorizontal) return true;
                if (type == BossAttackType.VineVertical) return true;
                // 突進、急降下、ハエ、噛みつきは不可
                return false;

            case BossForm.Form2:
                // 第二形態の仕様
                if (type == BossAttackType.AcidSpit) return true;
                if (type == BossAttackType.CeilingAcid) return true;
                if (type == BossAttackType.VineHorizontal) return true;
                if (type == BossAttackType.VineVertical) return true;
                if (type == BossAttackType.DiveBomb) return true; // 急降下
                if (type == BossAttackType.FlySummon) return hpRate <= 0.5f; // ハエ(50%以下)
                // 突進、噛みつきは不可
                return false;

            case BossForm.Form3:
                // 第三形態の仕様
                if (type == BossAttackType.AcidSpit) return true;
                if (type == BossAttackType.CeilingAcid) return true;
                if (type == BossAttackType.VineHorizontal) return true;
                if (type == BossAttackType.VineVertical) return true;
                if (type == BossAttackType.DiveBomb) return true;
                if (type == BossAttackType.Charge) return hpRate <= 0.25f; // 突進(25%以下)
                if (type == BossAttackType.FlySummon) return hpRate <= 0.5f; // ハエ(50%以下)
                if (type == BossAttackType.Bite) return hpRate <= 0.75f; // 噛みつき(75%以下)
                return false;
        }
        return false;
    }

    IEnumerator BossBehaviorLoop()
    {
        // イベントが終了するまで待機
        yield return new WaitUntil(() => !isEventRunning);

        while (!isDead)
        {
            // --- 行動選択の前に、まずプレイヤーの位置をチェック ---
            if (playerTransform != null && playerTransform.position.y > highPlayerYThreshold)
            {
                // プレイヤーが高所にいる場合、通常行動をキャンセルして対空攻撃を優先する
                Debug.Log("プレイヤーが高所にいるため、対空攻撃パターンに移行");

                // 1. 地面に潜る
                yield return StartCoroutine(AnimateSubmerge(true, submergeAnimTime));

                // 2. 天井からの溶解液攻撃を実行
                yield return StartCoroutine(CeilingAcidAttack());

                // 3. 地上に戻る
                UpdateNextStandbyPosition();
                yield return StartCoroutine(AnimateSubmerge(false, submergeAnimTime));

                // 4. 攻撃後のクールダウン
                Debug.Log($"対空攻撃クールダウン開始（{antiAirAttackCooldown}秒）");
                yield return new WaitForSeconds(antiAirAttackCooldown);

                // 5. ループの先頭に戻り、再度プレイヤーの位置をチェックする
                continue;
            }

            // 待機
            yield return new WaitForSeconds(3.0f); // 攻撃間隔を少し調整
            if (isDead) break;

            if (playerTransform != null)
            {
                // 行動の抽選ロジック

                // 1. まず「大技」を使うかどうか抽選 (30%)
                // ただし、現在使える大技が1つもない場合は、通常攻撃へ流す
                bool trySpecial = (Random.Range(0f, 100f) < 30f);
                bool executedAction = false;

                if (trySpecial)
                {
                    // 現在使用可能な大技リストを作成
                    List<BossAttackType> availableSpecials = new List<BossAttackType>();

                    if (CanUseAttack(BossAttackType.DiveBomb)) availableSpecials.Add(BossAttackType.DiveBomb);
                    if (CanUseAttack(BossAttackType.Bite)) availableSpecials.Add(BossAttackType.Bite);
                    if (CanUseAttack(BossAttackType.CeilingAcid)) availableSpecials.Add(BossAttackType.CeilingAcid); // 天井毒液も大技枠候補に含める

                    if (availableSpecials.Count > 0)
                    {
                        // ランダムに選択して実行
                        BossAttackType selected = availableSpecials[Random.Range(0, availableSpecials.Count)];
                        yield return StartCoroutine(ExecuteSpecialAttack(selected));
                        executedAction = true;
                    }
                }

                // 大技を実行しなかった場合 (抽選漏れ or 使用可能技なし)、通常攻撃を行う
                if (!executedAction)
                {
                    // 通常攻撃: 突進(Charge) か 溶解液(AcidSpit)
                    // 使用可能なものをリストアップ
                    List<BossAttackType> availableNormals = new List<BossAttackType>();

                    if (CanUseAttack(BossAttackType.AcidSpit)) availableNormals.Add(BossAttackType.AcidSpit);
                    if (CanUseAttack(BossAttackType.Charge)) availableNormals.Add(BossAttackType.Charge);

                    if (availableNormals.Count > 0)
                    {
                        // ランダム選択
                        BossAttackType selected = availableNormals[Random.Range(0, availableNormals.Count)];

                        if (selected == BossAttackType.Charge)
                        {
                            yield return StartCoroutine(ChargeAttack());
                        }
                        else
                        {
                            yield return StartCoroutine(AcidSpitAttack());
                        }
                    }
                    else
                    {
                        // 何もできない場合 (ありえないはずだが安全策)、威嚇して待機
                        Debug.LogWarning("使用可能な攻撃がありません");
                        yield return new WaitForSeconds(1.0f);
                    }
                }
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player) playerTransform = player.transform;
            }
        }
    }

    // ツタ攻撃を一定間隔で独立して実行するループ
    IEnumerator VineAttackLoop()
    {
        // イベントが終了するまで待機
        yield return new WaitUntil(() => !isEventRunning);

        // 戦闘開始直後にいきなりツタが来ないように、最初は少し待つ
        yield return new WaitForSeconds(vineAttackInterval / 2f);

        while (!isDead)
        {
            // ボスが行動不能な状態（潜っている最中など）は待機
            if (bodyPartsRenderers[0] != null && !bodyPartsRenderers[0].enabled)
            {
                yield return new WaitForSeconds(1.0f); // 1秒ごとに状態をチェック
                continue; // ループの先頭に戻る
            }

            // 攻撃可能なツタの種類をリストアップ
            List<BossAttackType> validVines = new List<BossAttackType>();
            if (CanUseAttack(BossAttackType.VineHorizontal)) validVines.Add(BossAttackType.VineHorizontal);
            if (CanUseAttack(BossAttackType.VineVertical)) validVines.Add(BossAttackType.VineVertical);

            if (playerTransform != null && validVines.Count > 0)
            {
                // 可能なものからランダム選択
                BossAttackType selected = validVines[Random.Range(0, validVines.Count)];

                if (selected == BossAttackType.VineHorizontal)
                {
                    yield return StartCoroutine(VineAttackHorizontal());
                }
                else
                {
                    yield return StartCoroutine(VineAttackVertical());
                }
            }

            // 次のツタ攻撃までの待機
            yield return new WaitForSeconds(vineAttackInterval);
        }
    }

    // ハエ呼び寄せ攻撃を一定間隔で独立して実行するループ
    IEnumerator FlySummonLoop()
    {
        // イベントが終了するまで待機
        yield return new WaitUntil(() => !isEventRunning);

        // 戦闘開始直後にいきなり呼ばないように、最初は少し待つ
        yield return new WaitForSeconds(flySummonInterval / 2f);

        while (!isDead)
        {
            // 次の呼び出しまで待機
            yield return new WaitForSeconds(flySummonInterval);

            // ボスが行動不能な状態（潜っている最中など）は攻撃しない
            if (bodyPartsRenderers[0] != null && !bodyPartsRenderers[0].enabled)
            {
                // 状態が回復するまで1秒ごとにチェック
                yield return new WaitUntil(() => bodyPartsRenderers[0] != null && bodyPartsRenderers[0].enabled && !isDead);
            }

            // 使用許可が出ている場合のみ実行
            if (playerTransform != null && CanUseAttack(BossAttackType.FlySummon))
            {
                yield return StartCoroutine(SummonFlies());
            }
        }
    }

    // ハエを召喚し、プレイヤーの近くへ移動させる処理
    IEnumerator SummonFlies()
    {
        Debug.Log("ボス：ハエの呼び寄せ");

        // --- 演出：音波エフェクトとSE ---
        if (sonicWaveEffectPrefab != null)
        {
            // ボスの頭の位置（firePoint）からエフェクトを出す
            Instantiate(sonicWaveEffectPrefab, firePoint.position, Quaternion.identity);
        }
        if (audioSource != null && flySummonSound != null)
        {
            audioSource.PlayOneShot(flySummonSound);
        }

        // 呼び寄せモーションのような短い待機時間
        yield return new WaitForSeconds(1.0f);

        // --- ハエの生成と移動開始 ---
        if (playerTransform == null || flyPrefab == null) yield break;

        for (int i = 0; i < numberOfFlies; i++)
        {
            // 1. 出現位置を画面外の左右ランダムに決める
            bool spawnOnLeft = (Random.value > 0.5f);
            float spawnX = spawnOnLeft ? minX - flySpawnOffset.x : maxX + flySpawnOffset.x;
            // Y座標はプレイヤーの少し上あたりでランダムにする
            float spawnY = playerTransform.position.y + Random.Range(2.0f, 5.0f);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

            // 2. 目標地点をプレイヤーの近くに設定
            float targetX = playerTransform.position.x + Random.Range(-3.0f, 3.0f);
            float targetY = playerTransform.position.y + Random.Range(2.0f, 4.0f);
            Vector3 targetPosition = new Vector3(targetX, targetY, 0);

            // 3. ハエを生成
            GameObject flyInstance = Instantiate(flyPrefab, spawnPosition, Quaternion.identity);

            // 4. ハエ自身に「あの場所へ飛んでいけ」と命令する
            SkyEnemyBoss skyEnemyScript = flyInstance.GetComponent<SkyEnemyBoss>();
            if (skyEnemyScript != null)
            {
                // ハエに、自分自身（ボス）がオーナーであることを教える
                skyEnemyScript.ownerBoss = this;
                skyEnemyScript.GoToTargetAndActivateAI(targetPosition, flyMoveSpeed);
            }

            // 全員同時に出現しないように少し待つ
            yield return new WaitForSeconds(0.3f);
        }
    }

    // ハエから呼び出され、HPを回復する公開メソッド
    public void HealByEatingFly()
    {
        if (isDead) return; // 既に死んでいたら何もしない

        Debug.Log("ボスがハエを捕食して回復！");

        currentHp += flyHealAmount;

        // 上限HPを超えないように調整
        if (currentHp > maxHp)
        {
            currentHp = maxHp;
        }

        // UIにHPの変更を通知
        if (OnHPChanged != null)
        {
            OnHPChanged.Invoke(currentHp, maxHp);
        }

        // ここに回復エフェクトやSE
    }

    // --- 攻撃: 突進 ---
    IEnumerator ChargeAttack()
    {
        Debug.Log("ボス：突進構え");

        float moveDir = 0;

        GameObject warningInstance = null;
        if (warningAreaPrefab != null && playerTransform != null)
        {
            warningInstance = Instantiate(warningAreaPrefab);

            moveDir = Mathf.Sign(playerTransform.position.x - transform.position.x);

            // 画面端までの距離を計算して警告範囲を設定
            float startX = transform.position.x;
            float endX = (moveDir > 0) ? maxX + 5.0f : minX - 5.0f; // 画面外まで突き抜けるように設定

            float warningWidth = Mathf.Abs(endX - startX);
            float warningCenterX = startX + (endX - startX) / 2;

            Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
            foreach (var sr in bodyPartsRenderers)
            {
                if (sr != null) totalBounds.Encapsulate(sr.bounds);
            }
            float warningHeight = totalBounds.size.y;
            float warningCenterY = totalBounds.center.y;

            warningInstance.transform.position = new Vector3(warningCenterX, warningCenterY, 0);
            warningInstance.transform.localScale = new Vector3(warningWidth, warningHeight, 1);

            float elapsedTime = 0f;
            warningInstance.SetActive(true);

            while (elapsedTime < chargeWarningTime)
            {
                warningInstance.SetActive(!warningInstance.activeSelf);
                yield return new WaitForSeconds(blinkInterval);
                elapsedTime += blinkInterval;
            }

            Destroy(warningInstance);
        }
        else
        {
            yield return new WaitForSeconds(chargeWarningTime);
        }

        Debug.Log("ボス：突進開始");

        // 突進開始SEを再生
        if (audioSource != null && chargeStartSound != null)
        {
            audioSource.PlayOneShot(chargeStartSound);
        }

        // 警告時に計算した突進方向を再度使用
        if (moveDir == 0) // 警告が表示されなかった場合の保険
        {
            moveDir = Mathf.Sign(playerTransform.position.x - transform.position.x);
        }

        // 突進方向に向きを合わせる
        FlipBody(moveDir > 0);

        // --- カメラシェイクを呼び出す ---
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }

        // --- 画面外に出るまで突進し続けるループ ---
        while (transform.position.x > minX - 5f && transform.position.x < maxX + 5f)
        {
            if (isDead) break;

            float moveAmount = moveDir * chargeThroughSpeed * Time.deltaTime;
            transform.Translate(new Vector3(moveAmount, 0, 0), Space.World);

            yield return null;
        }

        // --- 画面外に出た後、ボスを非表示にし、地面から再登場する ---
        Debug.Log("ボス：次の待機位置へ帰還準備");

        // 1. レンダラーを全て非表示にする
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null) sr.enabled = false;
        }

        // 2. 次の待機位置を決定
        UpdateNextStandbyPosition();

        // 3. 地面から再登場するアニメーションを呼び出す
        //    (AnimateSubmergeの第2引数に、潜るアニメーションの時間を渡す)
        yield return StartCoroutine(AnimateSubmerge(false, submergeAnimTime));
    }

    // --- 攻撃: 横ツタ ---
    IEnumerator VineAttackHorizontal()
    {
        Debug.Log("ボス：横ツタ攻撃");
        bool isLeftStart = (Random.value > 0.5f);
        float spawnX = isLeftStart ? minX : maxX;
        float spawnY = Mathf.Clamp(playerTransform.position.y, minY + 1.0f, maxY - 1.0f);

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        float zAngle = isLeftStart ? 0f : 180f;

        SpawnVine(spawnPos, zAngle, Mathf.Abs(maxX - minX), VineType.Horizontal);
        yield return new WaitForSeconds(vineWarningTime + 0.5f);
    }

    // --- 攻撃: 縦ツタ ---
    IEnumerator VineAttackVertical()
    {
        Debug.Log("ボス：縦ツタ攻撃");
        float spawnX = Mathf.Clamp(playerTransform.position.x, minX + 1.0f, maxX - 1.0f);
        Vector3 spawnPos = new Vector3(spawnX, maxY, 0);
        float zAngle = -90f;
        float length = Mathf.Abs(maxY - minY);

        SpawnVine(spawnPos, zAngle, length, VineType.Vertical);
        yield return new WaitForSeconds(vineWarningTime + 0.5f);
    }

    // --- 溶解液攻撃 (投擲) ---
    IEnumerator AcidSpitAttack()
    {
        Debug.Log("ボス：溶解液攻撃準備");

        if (headRenderer != null)
        {
            // 1. 元の色を保存しておく
            Color originalColor = headRenderer.color;
            float elapsedTime = 0f;

            // 2. 警告時間中、点滅を繰り返すループ
            while (elapsedTime < acidWarningTime)
            {
                // 色を警告色と元の色で交互に切り替える
                headRenderer.color = (headRenderer.color == originalColor) ? acidWarningColor : originalColor;

                // 設定した間隔で待機
                yield return new WaitForSeconds(acidBlinkInterval);
                elapsedTime += acidBlinkInterval;
            }

            // 3. 警告終了後、必ず元の色に戻す
            headRenderer.color = originalColor;
        }
        else
        {
            // headRendererが設定されていない場合は、単純に待つだけ
            yield return new WaitForSeconds(acidWarningTime);
        }

        Debug.Log("ボス：溶解液攻撃");

        // 3発撃つ
        for (int i = 0; i < 3; i++)
        {
            if (isDead) break;

            // 弾生成
            GameObject bullet = Instantiate(acidBulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            // 溶解液発射SEを再生
            if (audioSource != null && acidSpitSound != null)
            {
                audioSource.PlayOneShot(acidSpitSound);
            }

            if (bulletRb != null && playerTransform != null)
            {
                // プレイヤーへの方向計算
                Vector2 direction = (playerTransform.position - firePoint.position);

                Vector2 throwDir = direction.normalized + Vector2.up * 0.5f; // 斜め45度くらい上

                bulletRb.linearVelocity = throwDir.normalized * throwForce;
            }

            // 次の弾までの待機
            yield return new WaitForSeconds(throwInterval);
        }

        yield return new WaitForSeconds(1.0f); // 撃ち終わりの硬直
    }

    // --- 攻撃分岐: 大技 ---
    IEnumerator ExecuteSpecialAttack(BossAttackType attackType)
    {
        Debug.Log($"ボス：[大技] {attackType} を開始");

        // 1. 地中に潜る
        yield return StartCoroutine(AnimateSubmerge(true, submergeAnimTime));

        // 2. 選択された攻撃を実行
        switch (attackType)
        {
            case BossAttackType.DiveBomb:
                yield return StartCoroutine(DiveBombAttack());
                break;
            case BossAttackType.Bite: // 画面奥からの範囲攻撃
                yield return StartCoroutine(BackgroundRangeAttack());
                break;
            case BossAttackType.CeilingAcid:
                yield return StartCoroutine(CeilingAcidAttack());
                break;
        }

        // 3. 次の待機位置に戻る
        UpdateNextStandbyPosition();
        yield return StartCoroutine(AnimateSubmerge(false, submergeAnimTime));
    }

    // --- 攻撃: 急降下爆撃 ---
    IEnumerator DiveBombAttack()
    {
        Debug.Log("ボス：[大技] 急降下爆撃開始");

        // --- 2. 警告エリアを生成し、プレイヤーを追従 ---
        GameObject warning = Instantiate(warningAreaPrefab);
        Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
        foreach (var sr in bodyPartsRenderers) if (sr != null) totalBounds.Encapsulate(sr.bounds);

        float warningWidth = totalBounds.size.x;
        float warningHeight = (maxY - minY) * 3f;
        float warningCenterY = minY + warningHeight / 2f; // 地面を基準に中心を計算
        warning.transform.localScale = new Vector3(warningWidth, warningHeight, 1);

        float elapsedTime = 0f;
        float blinkInterval = 0.2f;
        while (elapsedTime < diveFollowTime)
        {
            // Y座標の中心も更新
            warning.transform.position = new Vector3(playerTransform.position.x, warningCenterY, 0);
            warning.SetActive(!warning.activeSelf);
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        // --- 3. 追従終了、警告の点滅を高速化 ---
        elapsedTime = 0f;
        float fastBlinkInterval = blinkInterval / 2;
        while (elapsedTime < diveFastBlinkTime)
        {
            warning.SetActive(!warning.activeSelf);
            yield return new WaitForSeconds(fastBlinkInterval);
            elapsedTime += fastBlinkInterval;
        }

        float fallXPosition = warning.transform.position.x;
        Destroy(warning);

        // --- 4. 上から高速で落ちてくる ---
        transform.position = new Vector3(fallXPosition, maxY + 5f, 0);
        foreach (var sr in bodyPartsRenderers) sr.enabled = true;

        Vector3 targetFallPosition = new Vector3(fallXPosition, minY + totalBounds.size.y / 2, 0);
        while (transform.position.y > targetFallPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetFallPosition, diveFallSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetFallPosition;

        // 着地SEを再生
        if (audioSource != null && diveLandSound != null)
        {
            audioSource.PlayOneShot(diveLandSound);
        }

        // 着地した瞬間にカメラを揺らす
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }

        // --- 5. 着地後、一定時間行動不能 ---
        Debug.Log("ボス：着地、行動不能");
        yield return new WaitForSeconds(diveStunTime);

        // --- 6. 再び潜る ---
        // 攻撃後の復帰は SpecialAttackBranch で行うので、ここでは潜るだけ
        float endY = transform.position.y - totalBounds.size.y;
        float submergeDuration = submergeAnimTime * 0.5f; // 短めに
        elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, endY, startPos.z);
        while (elapsedTime < submergeDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / submergeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        foreach (var sr in bodyPartsRenderers) sr.enabled = false;
    }

    // --- 攻撃: 画面奥からの範囲攻撃 ---
    IEnumerator BackgroundRangeAttack()
    {
        // 念のため、第一形態ではこのコルーチンが呼ばれないようにガード処理を追加
        if (currentForm == BossForm.Form1)
        {
            Debug.LogWarning("第一形態で BackgroundRangeAttack が誤って呼び出されたため、キャンセルします。");
            yield break; // 攻撃を中止して即座にコルーチンを終了
        }

        Debug.Log("ボス：[大技] 画面奥からの範囲攻撃");

        // --- 1. ボス本体（全パーツ）を非表示にする ---
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null) sr.enabled = false;
        }

        // プレイヤーがいない場合は攻撃を中断する
        if (playerTransform == null) yield break;

        // --- 2. プレイヤーの位置に専用の顔を生成 ---
        Vector3 attackCenterPosition = playerTransform.position; // 初期位置はプレイヤーの現在地
        GameObject faceInstance = Instantiate(backgroundAttackFacePrefab, attackCenterPosition, Quaternion.identity);
        Collider2D faceCollider = faceInstance.GetComponent<Collider2D>();

        // 生成直後は当たり判定を無効化
        if (faceCollider != null)
        {
            faceCollider.enabled = false;
        }

        // --- 3. プレイヤーを追従しながら拡大し、その後位置を固定 ---
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = faceInstance.transform.localScale * 2f; // プレハブのスケールを基準に拡大
        float elapsedTime = 0f;
        float followDuration = zoomInDuration / 2f; // 拡大時間nの半分(n/2)を追従時間とする

        while (elapsedTime < zoomInDuration)
        {
            // 拡大時間の前半はプレイヤーを追従する
            if (elapsedTime < followDuration && playerTransform != null)
            {
                attackCenterPosition = playerTransform.position;
                faceInstance.transform.position = attackCenterPosition;
            }
            // 後半は位置を動かさない

            // 拡大処理
            float t = elapsedTime / zoomInDuration;
            faceInstance.transform.localScale = Vector3.Lerp(startScale, targetScale, t * t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        faceInstance.transform.localScale = targetScale;

        // 拡大完了時の位置を最終的な攻撃中心点とする
        attackCenterPosition = faceInstance.transform.position;

        // 拡大しきったら当たり判定を有効化
        if (faceCollider != null)
        {
            faceCollider.enabled = true;
        }


        // --- 4. 共通の警告プレハブを使って放射状の警告線を生成＆点滅 ---
        List<GameObject> warningInstances = new List<GameObject>();
        for (int i = 0; i < numberOfAttacks; i++)
        {
            if (warningAreaPrefab == null) break;
            float angle = 360f / numberOfAttacks * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 direction = rotation * Vector3.right;
            Vector3 warningScale = new Vector3(attackRadius, 0.3f, 1f);
            Vector3 warningPosition = attackCenterPosition + direction * (attackRadius / 2f);
            GameObject warning = Instantiate(warningAreaPrefab, warningPosition, rotation);
            warning.transform.localScale = warningScale;
            warningInstances.Add(warning);
        }
        float warningElapsedTime = 0f;
        while (warningElapsedTime < rangeWarningTime)
        {
            foreach (var warning in warningInstances) warning.SetActive(!warning.activeSelf);
            yield return new WaitForSeconds(blinkInterval);
            warningElapsedTime += blinkInterval;
        }
        foreach (var warning in warningInstances) if (warning != null) warning.SetActive(true);


        // --- 5. 警告線の先端から中心へ攻撃判定を走らせる ---
        List<GameObject> hitboxes = new List<GameObject>();

        // 攻撃開始と同時に警告線を破棄 
        foreach (var warning in warningInstances)
        {
            // 警告線の角度や位置情報を利用してから破棄する
            if (rangeAttackHitboxPrefab != null && warning != null)
            {
                // 角度（-180～180）を取得
                float startAngle = warning.transform.eulerAngles.z;

                // 開始位置を計算
                Vector3 direction = warning.transform.right;
                Vector3 startPos = attackCenterPosition + direction * attackRadius;

                // 当たり判定を生成
                GameObject hitbox = Instantiate(rangeAttackHitboxPrefab, startPos, warning.transform.rotation);
                hitboxes.Add(hitbox);

                // 新しい移動コルーチンを呼び出す
                StartCoroutine(MoveHitboxInSpiral(hitbox.transform, attackCenterPosition, attackRadius, attackTravelTime, startAngle));
            }
            // 使い終わった警告線はすぐに破棄
            Destroy(warning);
        }
        // warningInstancesリストをクリアしておく
        warningInstances.Clear();


        yield return new WaitForSeconds(attackTravelTime + 0.5f);

        // --- 6. クリーンアップ ---

        // 生成した顔オブジェクトを破棄
        if (faceInstance != null)
        {
            Destroy(faceInstance);
        }
    }

    private IEnumerator MoveHitboxInSpiral(Transform hitbox, Vector3 center, float startRadius, float duration, float startAngle)
    {
        float elapsedTime = 0f;
        // 時計回りに360度回転させる
        float angleToTravel = 180f;

        while (elapsedTime < duration)
        {
            if (hitbox == null) yield break; // 移動中に破壊された場合に備える

            float progress = elapsedTime / duration; // 0.0 (開始) ～ 1.0 (終了)

            // 1. 半径を中心に向かって徐々に小さくする (Spiral)
            float currentRadius = Mathf.Lerp(startRadius, 0f, progress);

            // 2. 角度を時計回りに変化させる (Rotation)
            float currentAngleDegrees = startAngle - (angleToTravel * progress);

            // 3. 角度をラジアンに変換 (Mathf.Cos/Sinはラジアンを使用するため)
            float currentAngleRadians = currentAngleDegrees * Mathf.Deg2Rad;

            // 4. 新しい座標を計算
            float x = center.x + currentRadius * Mathf.Cos(currentAngleRadians);
            float y = center.y + currentRadius * Mathf.Sin(currentAngleRadians);

            hitbox.position = new Vector3(x, y, center.z);

            // 当たり判定自体も進行方向に向ける（お好みで）
            Vector3 directionToCenter = (center - hitbox.position).normalized;
            if (directionToCenter != Vector3.zero)
            {
                float angle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg;
                hitbox.rotation = Quaternion.Euler(0, 0, angle + 90); // 進行方向に対して垂直にするなど調整
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 終了したら当たり判定を破棄
        if (hitbox != null) Destroy(hitbox.gameObject);
    }

    // --- 攻撃: 天井からの毒液 ---
    private IEnumerator CeilingAcidAttack()
    {
        Debug.Log("ボス：[大技] 天井からの毒液攻撃");

        // --- 1. 準備 ---
        canFacePlayer = false; // 攻撃中はボスの向きを固定
        if (cameraPriorityController != null)
        {
            cameraPriorityController.ForceHighAltitudeView(true);
        }

        // ボスの体の高さを計算
        Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
        foreach (var sr in bodyPartsRenderers) if (sr != null) totalBounds.Encapsulate(sr.bounds);
        float bossHeight = totalBounds.size.y;

        // --- 2. 天井の中央から出現 ---
        // X座標をステージの中央（0）に設定
        Vector3 startPos = new Vector3(0, maxY + bossHeight, 0);
        Vector3 endPos = new Vector3(0, maxY, 0);

        // 見えない位置に移動し、上下反転させてから表示
        transform.position = startPos;
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null)
            {
                // 形態に関わらず、表示されている全てのパーツの画像を上下反転させる
                sr.flipY = true;
                sr.enabled = true;
            }
        }

        // 頭にも上下反転したことを伝える
        if (bossHead != null)
        {
            bossHead.SetVerticalFlip(true);
        }

        // ゆっくり降りてくるアニメーション
        float elapsedTime = 0f;
        while (elapsedTime < ceilingAnimDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / ceilingAnimDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        yield return new WaitForSeconds(0.5f); // 攻撃前のタメ

        // --- 3. 溶解液をばらまく (滞在時間ベースに変更) ---
        float attackTimer = 0f;
        float nextShotTime = 0f;
        // 弾の発射間隔を2倍にして、よりゆっくりにする
        float slowerInterval = acidRainInterval * 2f;

        while (attackTimer < ceilingStayDuration)
        {
            if (isDead) break;

            // 次の弾を発射する時間になったら
            if (attackTimer >= nextShotTime)
            {
                if (acidBulletPrefab != null && firePoint != null)
                {
                    GameObject bullet = Instantiate(acidBulletPrefab, firePoint.position, Quaternion.identity);
                    Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                    // 溶解液発射SEを再生
                    if (audioSource != null && acidSpitSound != null)
                    {
                        audioSource.PlayOneShot(acidSpitSound);
                    }

                    if (bulletRb != null)
                    {
                        // 左右ランダムな方向に力を加える
                        float randomX = Random.Range(-1f, 1f);
                        Vector2 force = new Vector2(randomX * acidRainSpreadForce, -5f);
                        bulletRb.AddForce(force, ForceMode2D.Impulse);
                    }
                }
                // 次の弾の発射時刻を更新
                nextShotTime += slowerInterval;
            }

            attackTimer += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(1.0f); // 攻撃後の硬直

        // --- 4. 天井に消える ---
        startPos = transform.position; // 現在地から
        endPos = new Vector3(transform.position.x, maxY + bossHeight, 0); // 天井の上へ

        elapsedTime = 0f;
        while (elapsedTime < ceilingAnimDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / ceilingAnimDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // --- 5. 後片付け ---
        // 見えなくなったら非表示にし、上下反転を元に戻す
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null)
            {
                sr.enabled = false;
                sr.flipY = false;
            }
        }

        // 頭の上下反転も元に戻す
        if (bossHead != null)
        {
            bossHead.SetVerticalFlip(false);
        }

        if (cameraPriorityController != null)
        {
            cameraPriorityController.ForceHighAltitudeView(false);
        }
        canFacePlayer = true; // 向き固定を解除
    }


    // --- 共通処理 ---
    void SpawnVine(Vector3 pos, float angle, float length, VineType type)
    {
        GameObject vineObj = Instantiate(vinePrefab, pos, Quaternion.Euler(0, 0, angle));
        BossVine vineScript = vineObj.GetComponent<BossVine>();
        if (vineScript != null)
        {
            vineScript.ownerBoss = this;

            vineScript.StartAttack(length, vineWarningTime, type);
        }
    }

    void FacePlayer()
    {
        if (playerTransform == null) return;
        float direction = playerTransform.position.x - transform.position.x;
        // プレイヤーが右側にいるべきか（反転すべきか）を判断
        bool shouldFlip = direction > 0;

        // 状態管理のif文を削除し、毎フレームFlipBodyを呼び出すように戻します
        FlipBody(shouldFlip);
    }

    void FlipBody(bool flipX)
    {
        // --- 全パーツのスプライト反転 ---
        // bodyPartsRenderers配列に含まれる全てのスプライトの向きを更新
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null)
            {
                sr.flipX = flipX;
            }
        }

        // --- 頭の位置制御スクリプトに反転状態を伝える ---
        // BossHeadスクリプトに「今どっちを向いているか」を教える
        if (bossHead != null)
        {
            bossHead.SetFlip(flipX);
        }
    }

    // --- 次の待機位置を決定する ---
    void UpdateNextStandbyPosition()
    {
        // 50%の確率で左端か右端かを決める
        float targetX = (Random.value < 0.5f)
            ? minX + standbyPositionOffset.x  // 左端
            : maxX - standbyPositionOffset.x; // 右端

        // 新しい待機位置を更新 (YとZは初期位置のものを流用)
        currentTargetPosition = new Vector3(targetX, initialPosition.y, initialPosition.z);
    }

    private IEnumerator StartOpeningEvent()
    {
        // --- イベント開始時に足場を無効化 ---
        if (temporaryPlatforms != null)
        {
            foreach (var platformCollider in temporaryPlatforms)
            {
                if (platformCollider != null)
                {
                    platformCollider.enabled = false;
                }
            }
        }

        // --- ① プレイヤー落下準備 ---
        if (playerAction != null)
        {
            // プレイヤーの入力を無効化
            playerAction.SetInputActive(false);

            // プレイヤーのRigidbodyを有効にし、落下開始地点へ移動
            Rigidbody2D playerRb = playerAction.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerFallStartPoint != null)
            {
                playerRb.bodyType = RigidbodyType2D.Dynamic; // 物理挙動を有効に
                playerRb.linearVelocity = Vector2.zero; // 落下前の速度をリセット 
                playerAction.transform.position = playerFallStartPoint.position;
            }
        }

        // ボスは初期スプライトで待機
        if (initialFormSprite != null && bodyPartsRenderers.Length > 0 && bodyPartsRenderers[0] != null)
        {
            //  元のスプライトを一時的に保存
            originalBodySprite = bodyPartsRenderers[0].sprite;

            // 全てのパーツを非表示にし、ボディ(0番目)に初期スプライトを設定
            foreach (var sr in bodyPartsRenderers) sr.enabled = false;
            bodyPartsRenderers[0].enabled = true;
            bodyPartsRenderers[0].sprite = initialFormSprite;
        }

        // --- ② プレイヤーの着地を待つ ---
        Debug.Log("イベント：プレイヤーの着地を待機中...");
        yield return new WaitUntil(() => playerAction != null && playerAction.IsGrounded());
        Debug.Log("イベント：プレイヤー着地！");

        // 着地後、プレイヤーが滑らないように速度をゼロにする
        playerAction.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1.0f); // 着地後の短いタメ

        // --- ③ セリフパート ---
        Transform speechTarget;
        if (bossHead != null)
        {
            // 形態2, 3: 頭が存在する場合は、頭を追従対象にする
            speechTarget = bossHead.transform;
        }
        else
        {
            // 形態1: 専用アンカーが設定されていればそれを使い、なければ体自体を追従する
            speechTarget = (speechBubbleAnchorForm1 != null) ? speechBubbleAnchorForm1 : bodyForm1.transform;
        }

        if (speechBubbleInstance != null)
        {
            speechBubbleInstance.ShowMessage("よくぞここまで来たな", speechTarget);
            yield return new WaitForSeconds(3.0f);

            string formSpecificMessage = "";
            switch (currentForm)
            {
                case BossForm.Form1:
                    formSpecificMessage = "なぜもっと魔法を使わなかった...こんなものじゃ完全に<color=red>成長</color>しきれないじゃないか！";
                    break;
                case BossForm.Form2:
                    formSpecificMessage = "お前の魔法は心地よいな。だが...その力、あまりに微弱。我を完全な<color=red>成長</color>へと導くには程遠い。";
                    break;
                case BossForm.Form3:
                    formSpecificMessage = "感謝するぞ、魔法使い。お前の魔法が、我をここまで<color=red>成長</color>させてくれたのだからな！";
                    break;
            }

            speechBubbleInstance.ShowMessage(formSpecificMessage, speechTarget);
            yield return new WaitForSeconds(6.0f);

            speechBubbleInstance.Hide();
        }

        // --- ④ 変身パート ---
        yield return StartCoroutine(TransformSequence());

        // --- ⑤ 戦闘開始 ---
        Debug.Log("イベント終了、戦闘開始！");
        // --- イベント終了時に足場を再有効化 ---
        if (temporaryPlatforms != null)
        {
            foreach (var platformCollider in temporaryPlatforms)
            {
                if (platformCollider != null)
                {
                    platformCollider.enabled = true;
                }
            }
        }

        isEventRunning = false; // イベント終了フラグを立てる
        if (playerAction != null)
        {
            playerAction.SetInputActive(true); // プレイヤーの入力を有効化
        }
    }

    private IEnumerator TransformSequence()
    {
        // 最初に、イベント用に書き換えたスプライトを元に戻しておく
        if (originalBodySprite != null && bodyPartsRenderers.Length > 0 && bodyPartsRenderers[0] != null)
        {
            bodyPartsRenderers[0].sprite = originalBodySprite;
        }

        // 全てのパーツを一度非表示にする
        bodyForm1.SetActive(false);
        bodyForm2.SetActive(false);
        headForm2.SetActive(false);
        bodyForm3.SetActive(false);
        headForm3.SetActive(false);

        // 最終形態までの変身を順番に実行する
        if ((int)currentForm >= 0) // Form1以上
        {
            // 1. 第一形態への変身
            // 演出対象のレンダラーをリスト化
            var renderersToEffect = new List<SpriteRenderer> { bodyForm1.GetComponent<SpriteRenderer>() };
            yield return StartCoroutine(PlayTransformEffect(0.8f, renderersToEffect));
            // 見た目を確定
            bodyForm1.SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }

        if ((int)currentForm >= 1) // Form2以上
        {
            // 2. 第二形態への変身
            // 事前に第一形態を非表示に
            bodyForm1.SetActive(false);
            // 演出対象のレンダラーをリスト化 (体と頭)
            var renderersToEffect = new List<SpriteRenderer> {
            bodyForm2.GetComponent<SpriteRenderer>(),
            headForm2.GetComponent<SpriteRenderer>()
        };
            yield return StartCoroutine(PlayTransformEffect(0.8f, renderersToEffect));
            // 見た目を確定
            bodyForm2.SetActive(true);
            headForm2.SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }

        if ((int)currentForm >= 2) // Form3 (最終形態)
        {
            // 3. 第三形態への変身
            // 事前に第二形態を非表示に
            bodyForm2.SetActive(false);
            headForm2.SetActive(false);
            // 演出対象のレンダラーをリスト化 (体と頭)
            var renderersToEffect = new List<SpriteRenderer> {
            bodyForm3.GetComponent<SpriteRenderer>(),
            headForm3.GetComponent<SpriteRenderer>()
        };
            yield return StartCoroutine(PlayTransformEffect(0.8f, renderersToEffect));
            // 見た目を確定
            bodyForm3.SetActive(true);
            headForm3.SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }

        // 最後に、戦闘用の完全な状態に再初期化する
        InitializeBossForm();
        Debug.Log("変身完了！最終形態: " + currentForm.ToString());
    }

    // PlayTransformEffectメソッドの引数をリストに変更します
    private IEnumerator PlayTransformEffect(float duration, List<SpriteRenderer> targetRenderers)
    {
        // 事前に全てのレンダラーを有効化しておく
        foreach (var sr in targetRenderers)
        {
            if (sr != null) sr.gameObject.SetActive(true);
        }

        // スケールを保存
        Dictionary<SpriteRenderer, Vector3> originalScales = new Dictionary<SpriteRenderer, Vector3>();
        foreach (var sr in targetRenderers)
        {
            if (sr != null) originalScales[sr] = sr.transform.localScale;
        }

        float elapsedTime = 0f;
        float blinkInterval = 0.1f;

        while (elapsedTime < duration)
        {
            foreach (var sr in targetRenderers)
            {
                if (sr == null) continue;
                // 点滅
                sr.enabled = !sr.enabled;
                // わずかに拡大・縮小
                float scaleMultiplier = 1.0f + Mathf.Sin(elapsedTime * 20f) * 0.1f;
                sr.transform.localScale = originalScales[sr] * scaleMultiplier;
            }

            elapsedTime += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        // 演出後は必ず表示状態に戻し、スケールも元に戻す
        foreach (var sr in targetRenderers)
        {
            if (sr == null) continue;
            sr.enabled = true;
            sr.transform.localScale = originalScales[sr];
        }
    }

    private IEnumerator PlayTransformEffect(float duration, SpriteRenderer targetRenderer)
    {
        Vector3 originalScale = targetRenderer.transform.localScale;
        float elapsedTime = 0f;
        float blinkInterval = 0.1f;

        while (elapsedTime < duration)
        {
            // 点滅
            targetRenderer.enabled = !targetRenderer.enabled;
            // わずかに拡大・縮小
            float scaleMultiplier = 1.0f + Mathf.Sin(elapsedTime * 20f) * 0.1f; // サイン波でプルプルさせる
            targetRenderer.transform.localScale = originalScale * scaleMultiplier;

            elapsedTime += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        // 演出後は必ず表示状態に戻し、スケールも元に戻す
        targetRenderer.enabled = true;
        targetRenderer.transform.localScale = originalScale;
    }

    // --- ダメージ・死亡 ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // SEが設定されていれば再生する
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        currentHp -= damage;
        StartCoroutine(FlashDamageEffect());

        // UIにHPの変更を通知
        if (OnHPChanged != null)
        {
            OnHPChanged.Invoke(currentHp, maxHp);
        }

        if (currentHp <= 0) Die();
    }

    IEnumerator FlashDamageEffect()
    {
        Color originalColor = Color.white;
        Color damageColor = new Color(0.5f, 0f, 0.5f, 1f);
        foreach (var sr in bodyPartsRenderers) if (sr != null) sr.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        foreach (var sr in bodyPartsRenderers) if (sr != null) sr.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("ボス撃破！");

        // オーディオソースと撃破SEが設定されていれば再生する
        if (audioSource != null && deathSound != null)
        {
            // 他の音を止めずに再生する
            audioSource.PlayOneShot(deathSound);
        }

        // 1. 全てのコルーチン（行動AI、攻撃ループ、移動アニメーション等）を即座に停止する
        StopAllCoroutines();

        // 2. 物理的な移動を止める
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // その場で停止
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算の影響を受けないようにする
        }

        // 3. 当たり判定を消す
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (var col in cols)
        {
            col.enabled = false;
        }

        // 4. ゆらゆら動くアニメーション(BossBody)や頭の追従(BossHead)を止める
        // パーツのルートにあるスクリプトを無効化する
        BossBody[] bodyScripts = GetComponentsInChildren<BossBody>();
        foreach (var script in bodyScripts) script.enabled = false;

        BossHead[] headScripts = GetComponentsInChildren<BossHead>();
        foreach (var script in headScripts) script.enabled = false;

        // 5. 死亡演出（透明化）を開始する
        StartCoroutine(DeathSequence());
    }

    // 死亡演出：4秒かけて徐々に透明になり、完了後にシーン遷移する
    private IEnumerator DeathSequence()
    {
        float duration = 4.0f; // 4秒間
        float elapsedTime = 0f;

        // Lerpで計算

        while (elapsedTime < duration)
        {
            // 1.0(不透明) -> 0.0(透明) へ変化
            float alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / duration);

            // 現在アクティブな全パーツ（体、頭）の色を更新
            foreach (var sr in bodyPartsRenderers)
            {
                if (sr != null)
                {
                    Color color = sr.color;
                    color.a = alpha;
                    sr.color = color;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 念のため完全に透明にする
        foreach (var sr in bodyPartsRenderers)
        {
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 0.0f;
                sr.color = color;
            }
        }

        // 少し待ってからシーン遷移
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("GameClear");
    }

    // --- 地面に潜る/戻るアニメーション ---
    private IEnumerator AnimateSubmerge(bool isSubmerging, float duration)
    {
        // ボスの高さを計算
        Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
        foreach (var sr in bodyPartsRenderers) if (sr != null) totalBounds.Encapsulate(sr.bounds);
        float bossHeight = totalBounds.size.y;

        Vector3 startPos, endPos;

        if (isSubmerging) // 潜る場合
        {
            startPos = transform.position;
            endPos = new Vector3(startPos.x, startPos.y - bossHeight, startPos.z);
        }
        else // 戻ってくる（浮上する）場合
        {
            // 予告パーティクルを生成
            if (returnParticlePrefab != null)
            {
                // 帰還SEを再生
                if (audioSource != null && returnSound != null)
                {
                    audioSource.PlayOneShot(returnSound);
                }

                // Y座標は地面（minY）に合わせる
                Vector3 particlePos = new Vector3(currentTargetPosition.x, minY, currentTargetPosition.z);
                Instantiate(returnParticlePrefab, particlePos, Quaternion.identity);
                // パーティクルが少し再生されるのを待つ
                yield return new WaitForSeconds(1.0f);
            }

            // 戻り先の座標計算に、固定のinitialPositionではなくcurrentTargetPositionを使用する
            startPos = new Vector3(currentTargetPosition.x, currentTargetPosition.y - bossHeight, currentTargetPosition.z);
            endPos = currentTargetPosition;
            transform.position = startPos; // 見えないうちに地下へ移動
            foreach (var sr in bodyPartsRenderers) sr.enabled = true; // 表示を戻す
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos; // ぴったり位置を合わせる

        if (isSubmerging) // 潜り終わったら非表示にする
        {
            foreach (var sr in bodyPartsRenderers) sr.enabled = false;
        }
    }

    // 形態に応じてGameObjectのアクティブ状態と参照を初期化するメソッド
    private void InitializeBossForm()
    {
        // 一旦すべてのパーツを非表示にする
        bodyForm1.SetActive(false);
        bodyForm2.SetActive(false);
        headForm2.SetActive(false);
        bodyForm3.SetActive(false);
        headForm3.SetActive(false);

        // 新しいパーツリストを作成
        var activeRenderers = new List<SpriteRenderer>();

        switch (currentForm)
        {
            case BossForm.Form1:
                Debug.Log("第一形態の見た目を有効化");
                // 表示するパーツを有効化
                bodyForm1.SetActive(true);

                // 有効なレンダラーをリストに追加
                activeRenderers.Add(bodyForm1.GetComponent<SpriteRenderer>());

                // 参照の更新
                headRenderer = bodyForm1.GetComponent<SpriteRenderer>(); // 警告点滅用に体を設定
                bossHead = null; // 頭は存在しない
                break;

            case BossForm.Form2:
                Debug.Log("第二形態の見た目を有効化");
                // 表示するパーツを有効化
                bodyForm2.SetActive(true);
                headForm2.SetActive(true);

                // 有効なレンダラーをリストに追加
                activeRenderers.Add(bodyForm2.GetComponent<SpriteRenderer>());
                activeRenderers.Add(headForm2.GetComponent<SpriteRenderer>());

                // 参照の更新
                headRenderer = headForm2.GetComponent<SpriteRenderer>();
                bossHead = headForm2.GetComponent<BossHead>();
                break;

            case BossForm.Form3:
                Debug.Log("第三形態の見た目を有効化");
                // 表示するパーツを有効化
                bodyForm3.SetActive(true);
                headForm3.SetActive(true);

                // 有効なレンダラーをリストに追加
                activeRenderers.Add(bodyForm3.GetComponent<SpriteRenderer>());
                activeRenderers.Add(headForm3.GetComponent<SpriteRenderer>());

                // 参照の更新: インスペクターの設定に頼らず、アクティブにしたオブジェクトから明示的に取得する
                // これにより、参照設定ミスを防ぎ、動作を確実にする
                headRenderer = headForm3.GetComponent<SpriteRenderer>();
                bossHead = headForm3.GetComponent<BossHead>();
                break;
        }

        // 汎用的に使うパーツ配列を、現在アクティブなパーツで上書きする
        bodyPartsRenderers = activeRenderers.ToArray();
    }

    // 指定した秒数待った後に、GameClearシーンをロードするコルーチンを追加
    private IEnumerator LoadGameClearSceneAfterDelay(float delay)
    {
        // 指定された時間待機する
        yield return new WaitForSeconds(delay);

        // "GameClear" という名前のシーンをロードする
        SceneManager.LoadScene("GameClear");
    }
}