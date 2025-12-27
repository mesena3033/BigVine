using Unity.Cinemachine;
using System.Collections;
using UnityEngine;

public class FallingRock : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private int damageAmount = 5;
    [SerializeField] private float shakeMagnitude = 0.4f;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float cameraFollowDuration = 2.0f;

    [Header("エフェクト")]
    [Tooltip("衝撃を発生させるためのImpulse Sourceコンポーネント")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private Rigidbody2D rb;
    private bool hasHit = false;

    // カメラ優先度コントローラーを操作するための変数
    private CameraPriorityController cameraPriorityController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        // ゲーム開始時に、メインカメラからCameraPriorityControllerを探して保持しておく
        if (Camera.main != null)
        {
            cameraPriorityController = Camera.main.GetComponent<CameraPriorityController>();
        }
    }

    public void StartFall(CinemachineCamera vcam)
    {
        Debug.Log("StartFallが呼び出されました。");
        if (vcam != null)
        {
            Debug.Log("追従カメラ " + vcam.name + " の制御を開始します。");
            StartCoroutine(FollowRockWithCamera(vcam));
        }
        else
        {
            Debug.LogWarning("引数で渡された追従カメラ(vcam)がnullです。インスペクターの設定を確認してください。");
        }
    }

    private IEnumerator FollowRockWithCamera(CinemachineCamera vcam)
    {
        if (cameraPriorityController == null)
        {
            Debug.LogError("致命的エラー：FallingRockがCameraPriorityControllerを見つけられていません！");
            yield break; // コルーチンを中断
        }

        // 元のプライオリティを記憶
        int originalPriority = vcam.Priority.Value;

        // プライオリティの自動更新を一時停止させる
        cameraPriorityController.IsPaused = true;
        Debug.Log("CameraPriorityControllerを一時停止しました。");

        // このカメラのプライオリティを一時的に引き上げて、主導権を奪う
        vcam.Follow = transform;
        // 他のカメラ(10, 11)より確実に高い値に設定
        vcam.Priority.Value = 20;
        Debug.Log(vcam.name + " のPriorityを20に設定し、カメラをジャックします。");


        yield return new WaitForSeconds(cameraFollowDuration);

        Debug.Log(cameraFollowDuration + "秒経過したので、カメラ制御を返却します。");
        // 仕事が終わったら、プライオリティを元に戻して主導権を返す
        vcam.Follow = null;
        vcam.Priority.Value = originalPriority;

        // プライオリティの自動更新を再開させる
        cameraPriorityController.IsPaused = false;
        Debug.Log("CameraPriorityControllerを再開しました。");
    }

    // OnCollisionEnter2D と DestroyAfterDelay は変更なし
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        BossController boss = collision.gameObject.GetComponentInParent<BossController>();
        if (boss != null)
        {
            hasHit = true;
            boss.TakeDamage(damageAmount);
            if (CameraShaker.Instance != null)
            {
                CameraShaker.Instance.Shake(shakeDuration, shakeDuration);
            }
            StartCoroutine(DestroyAfterDelay());
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
            StartCoroutine(DestroyAfterDelay());
        }
    }

    // ヒット時の共通処理をまとめる
    private void HandleHit(GameObject hitObject)
    {
        hasHit = true;

        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}