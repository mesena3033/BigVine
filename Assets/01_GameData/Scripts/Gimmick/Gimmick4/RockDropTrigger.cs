using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class RockDropTrigger : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rockRb;
    [SerializeField] private float gravityScale = 2f;

    [SerializeField] private GameObject targetImage;

    //カメラ
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private int highPriority = 20;
    [SerializeField] private float priorityDuration = 2f;

    private bool hasTriggered = false;
    private GameObject lastBullet = null;

    private int originalPriority;

    private void Awake()
    {
        // 実行開始時に、設定された仮想カメラの元のプライオリティを保存しておく
        if (virtualCamera != null)
        {
            originalPriority = virtualCamera.Priority.Value;
        }
        else
        {
            Debug.LogError("仮想カメラが設定されていません！", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("MagicBullet") && other.gameObject != lastBullet)
        {
            lastBullet = other.gameObject;
            hasTriggered = true;

            if (rockRb != null)
            {
                rockRb.bodyType = RigidbodyType2D.Dynamic;
                rockRb.gravityScale = gravityScale;
            }

            if (targetImage != null) targetImage.SetActive(false);

            if (virtualCamera != null)
            {
                StartCoroutine(BoostCameraPriorityCoroutine());
            }
        }
    }

    private IEnumerator BoostCameraPriorityCoroutine()
    {
        // 1. プライオリティを指定した値に変更する
        Debug.Log($"{virtualCamera.name} のプライオリティを {highPriority} に変更します。");
        // Priority構造体の.Valueプロパティに値を設定する
        virtualCamera.Priority.Value = highPriority;

        // 2. 指定した時間だけ待機する
        yield return new WaitForSeconds(priorityDuration);

        // 3. プライオリティを元の値に戻す
        Debug.Log($"{virtualCamera.name} のプライオリティを元の値 {originalPriority} に戻します。");
        // こちらも同様に.Valueプロパティに元の値を設定する
        virtualCamera.Priority.Value = originalPriority;
    }
}
