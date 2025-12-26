using UnityEngine;
using System.Collections;

public class SkyEnemyBoss : MonoBehaviour
{
    [Header("ボスへの帰還設定")]
    [Tooltip("この時間(秒)が経過すると、ボスへ帰還する")]
    [SerializeField] private float lifetime = 15f;
    [Tooltip("ボスへ戻る時の速度")]
    [SerializeField] private float returnSpeed = 20f;

    // 自分を生成したボスの情報を格納する変数
    public BossController ownerBoss { get; set; }

    [Header("追従・突進AI")]
    [Tooltip("感知範囲")]
    [SerializeField] private float detectionRange = 10f;
    [Tooltip("突進の速度")]
    [SerializeField] private float rushSpeed = 15f;
    [Tooltip("突進が継続する時間")]
    [SerializeField] private float rushDuration = 0.8f;
    [Tooltip("突進後のクールダウン時間")]
    [SerializeField] private float cooldownTime = 2f;

    private Transform player;
    private Vector3 targetPosition;
    private bool isRushing = false;
    private bool isCooldown = false;

    // AIが起動しているかどうかを管理するフラグ
    private bool isAiActive = false;

    // 外部から命令された移動ルーチンを保持するための変数
    private Coroutine commandedMoveCoroutine;

    void Start()
    {
        // プレイヤーを探して参照を保持しておく
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            // プレイヤーが見つからない場合はエラーメッセージを出す
            Debug.LogError("Player not found! Assign in Inspector or set Player tag.", this.gameObject);
        }

        // 生成されたら、自身の寿命タイマーを開始する
        StartCoroutine(LifetimeRoutine());
    }

    void Update()
    {
        // AIが非アクティブ、またはプレイヤーが見つからない場合は、何もしない
        if (!isAiActive || player == null)
        {
            return;
        }

        // プレイヤーとの距離を計算
        float distance = Vector3.Distance(transform.position, player.position);

        // 突進中でもクールダウン中でもなく、プレイヤーが感知範囲内にいれば突進を開始
        if (!isRushing && !isCooldown && distance <= detectionRange)
        {
            // 突進する目標地点を、その瞬間のプレイヤーの位置に設定
            targetPosition = player.position;
            StartCoroutine(Rush());
        }
    }

    /// <summary>
    /// ボスから呼び出される命令。指定された目標地点へ移動し、到着後に自律AIを起動する。
    /// </summary>
    /// <param name="targetPosition">目標地点の座標</param>
    /// <param name="speed">移動速度</param>
    public void GoToTargetAndActivateAI(Vector3 targetPosition, float speed)
    {
        // 念のため、実行中のコルーチンを停止
        if (commandedMoveCoroutine != null)
        {
            StopCoroutine(commandedMoveCoroutine);
        }
        // 指定地点への移動を開始
        commandedMoveCoroutine = StartCoroutine(MoveToTargetRoutine(targetPosition, speed));
    }

    /// <summary>
    /// 目標地点まで移動し、到着したら自律移動を開始するコルーチン
    /// </summary>
    private IEnumerator MoveToTargetRoutine(Vector3 target, float speed)
    {
        // AIはまだ動かさない
        isAiActive = false;

        // ターゲットの方向に向きを変える
        if (target.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1); // 左向き
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1); // 右向き
        }

        // ターゲットに十分近づくまで、毎フレーム移動処理を行う
        while (this != null && Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null; // 1フレーム待つ
        }

        // --- 到着後の処理 ---
        if (this == null) yield break; // 移動中に破壊された場合は終了

        // 最後に位置をぴったり合わせる
        transform.position = target;

        // ここでAIを起動する！
        isAiActive = true;
        Debug.Log("ハエが目標地点に到着。追従AIを起動します。");
    }

    // 突進処理
    private IEnumerator Rush()
    {
        isRushing = true;
        Debug.Log("プレイヤーを感知 → 突進開始");

        // 突進方向を見て向きを変える
        if (targetPosition.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1); // 左向き
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1); // 右向き
        }

        float elapsed = 0f;
        while (elapsed < rushDuration)
        {
            // 決めた目標地点に向かってまっすぐ突進する
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, rushSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isRushing = false;
        StartCoroutine(Cooldown()); // 突進が終わったらクールダウンへ
    }

    // クールダウン処理
    private IEnumerator Cooldown()
    {
        isCooldown = true;
        Debug.Log("クールダウン開始");
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
        Debug.Log("クールダウン終了");
    }

    // Sceneビューで感知範囲を視覚的に表示する（デバッグ用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    /// <summary>
    /// 自身の寿命を管理し、時間切れになったら帰還命令を出すコルーチン
    /// </summary>
    private IEnumerator LifetimeRoutine()
    {
        // 設定された寿命の時間だけ待機
        yield return new WaitForSeconds(lifetime);

        // 時間が来たら、ボスへの帰還を開始する
        Debug.Log("ハエの活動時間終了。ボスのもとへ帰ります。");
        RecallToBoss();
    }

    /// <summary>
    /// AIを停止し、ボスへ帰還する処理を開始する
    /// </summary>
    private void RecallToBoss()
    {
        // プレイヤー追従AIを停止
        isAiActive = false;

        // 実行中の突進やクールダウンを強制的に停止
        StopAllCoroutines();

        // ボスへ帰るための新しいコルーチンを開始
        StartCoroutine(ReturnToBossRoutine());
    }

    /// <summary>
    /// ボスの位置まで飛んでいき、到着したら自身を破壊するコルーチン
    /// </summary>
    private IEnumerator ReturnToBossRoutine()
    {
        // もしボスが既に倒されているなどで存在しない場合は、その場で消える
        if (ownerBoss == null)
        {
            Destroy(gameObject);
            yield break;
        }

        // ボスに近づくまでループ
        while (this != null && ownerBoss != null)
        {
            // ボスの現在地を取得
            Vector3 bossPosition = ownerBoss.transform.position;

            // ボスの方向へ移動
            transform.position = Vector3.MoveTowards(transform.position, bossPosition, returnSpeed * Time.deltaTime);

            // ボスとの距離を計算
            float distanceToBoss = Vector3.Distance(transform.position, bossPosition);

            // ボスに十分近づいたら（ここでは1.5ユニット以内）、回復処理を呼んで消滅
            if (distanceToBoss < 1.5f)
            {
                ownerBoss.HealByEatingFly(); // ボスに回復命令
                Destroy(gameObject);      // 自身の役目は終わったので消える
                yield break;              // コルーチンを抜ける
            }

            yield return null; // 1フレーム待つ
        }
    }
}
