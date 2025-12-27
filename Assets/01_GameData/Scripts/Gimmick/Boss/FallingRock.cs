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
        if (vcam != null)
        {
            StartCoroutine(FollowRockWithCamera(vcam));
        }
    }

    private IEnumerator FollowRockWithCamera(CinemachineCamera vcam)
    {
        if (cameraPriorityController == null)
        {
            Debug.LogError("致命的エラー：FallingRockがCameraPriorityControllerを見つけられていません！");
        }
        else
        {
            Debug.Log("FallingRockがCameraPriorityControllerを正常に参照しています。");
        }

        // 元のプライオリティを記憶
        int originalPriority = vcam.Priority.Value;

        // プライオリティの自動更新を一時停止させる
        if (cameraPriorityController != null)
        {
            cameraPriorityController.IsPaused = true;
        }

        // このカメラのプライオリティを一時的に引き上げて、主導権を奪う
        vcam.Follow = transform;
        vcam.Priority.Value = 11; // プレイヤーカメラ(10)より高くする

        yield return new WaitForSeconds(cameraFollowDuration);

        // 仕事が終わったら、プライオリティを元に戻して主導権を返す
        vcam.Follow = null;
        vcam.Priority.Value = originalPriority;

        // プライオリティの自動更新を再開させる
        if (cameraPriorityController != null)
        {
            cameraPriorityController.IsPaused = false;
        }
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