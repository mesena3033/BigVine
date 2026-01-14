using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class GrowthPointGimmick : MonoBehaviour
{
    public enum GimmickType { Turret, IvyShield, FallingRock, StickyBombLauncher }
    public GimmickType type;

    [Header("共通設定")]
    public int damageAmount = 1;

    [Header("ツタシールド (IvyShield) 用設定")]
    [SerializeField] private GameObject shieldObject; // 子オブジェクトのシールド(IvyShield)
    [SerializeField] private float shieldDuration = 7.0f; // シールド展開時間

    [Header("砲台 (Turret) 用設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireForce = 25f;
    [SerializeField] private float cooldown = 1.0f;

    [Header("落石 (FallingRock) 用設定")]
    [SerializeField] private GameObject fallingRockObject;
    [SerializeField] private CinemachineCamera rockFollowCamera;
    [SerializeField] private float fallingRockCooldown = 10f;

    [Header("ひっつき爆弾 (StickyBombLauncher) 用設定")]
    [SerializeField] private GameObject stickyBombPrefab;
    [SerializeField] private Transform stickyBombFirePoint;
    [SerializeField] private float stickyBombThrowForce = 12f;
    [SerializeField] private float stickyBombCooldown = 5f;
    private bool isCooldown = false;

    [Header("効果音 (SE)")]
    [SerializeField] private AudioSource audioSource; // このギミックから出る音を再生
    [SerializeField] private AudioClip turretFireSound; // 砲台の発射音
    [SerializeField] private AudioClip shieldOpenSound; // ツタシールドの展開音
    [SerializeField] private AudioClip rockReleaseSound; // 落石が解放される音

    // --- 魔法が当たった時の処理 ---
    public void OnMagicHit()
    {
        Debug.Log("★★★ OnMagicHit 呼ばれました！★★★ オブジェクト名: " + this.gameObject.name);

        if (isCooldown) return;

        Debug.Log("成長点活性化: " + type);

        switch (type)
        {
            case GimmickType.Turret:
                StartCoroutine(FireTurret());
                break;

            case GimmickType.IvyShield:
                StartCoroutine(ActivateShield());
                break;

            case GimmickType.FallingRock:
                // 落石処理
                StartCoroutine(ActivateFallingRock());
                break;

            case GimmickType.StickyBombLauncher:
                StartCoroutine(FireStickyBomb());
                break;
        }
    }

    // --- ツタシールド展開コルーチン ---
    private IEnumerator ActivateShield()
    {
        isCooldown = true;

        if (shieldObject != null)
        {
            // シールド展開SEを再生
            if (audioSource != null && shieldOpenSound != null)
            {
                audioSource.PlayOneShot(shieldOpenSound);
            }

            shieldObject.SetActive(true); // シールド出現

            yield return new WaitForSeconds(shieldDuration); // 一定時間待機

            shieldObject.SetActive(false); // シールド消滅
        }
        else
        {
            Debug.LogWarning("Shield Objectが設定されていません！");
            yield return new WaitForSeconds(1.0f);
        }

        isCooldown = false;
    }

    // --- 砲台処理 ---
    private IEnumerator FireTurret()
    {
        isCooldown = true;

        float duration = 3.0f;      // 3秒間発射し続ける
        float fireInterval = 0.5f;  // 0.5秒ごとに発射
        float elapsedTime = 0f;

        // 指定された時間、弾を発射し続けるループ
        while (elapsedTime < duration)
        {
            // 弾のプレハブ(bulletPrefab)と発射位置(firePoint)が設定されているか確認
            if (bulletPrefab != null && firePoint != null)
            {
                // 1. 弾を生成する
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

                // 砲台の発射SEを再生
                if (audioSource != null && turretFireSound != null)
                {
                    audioSource.PlayOneShot(turretFireSound);
                }

                // 2. 生成した弾のRigidbody2Dを取得する
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

                // 3. 弾に力を加えて発射する
                if (rb != null)
                {
                    rb.AddForce(Vector2.right * fireForce, ForceMode2D.Impulse);
                }
            }
            else
            {
                // どちらかが設定されていない場合、警告を出す
                Debug.LogWarning("Bullet Prefab または Fire Point が設定されていません！");
                break; // ループを中断
            }

            // 次の発射まで待機
            yield return new WaitForSeconds(fireInterval);
            elapsedTime += fireInterval; // 経過時間を加算
        }

        // クールダウン
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }

    // --- 落石ギミック処理 ---
    private IEnumerator ActivateFallingRock()
    {
        isCooldown = true;

        if (fallingRockObject != null)
        {
            Debug.Log("落石ギミック開始: 岩オブジェクトをチェックします。");

            // 岩オブジェクトが非アクティブならアクティブにする
            if (!fallingRockObject.activeSelf)
            {
                fallingRockObject.SetActive(true);
                Debug.Log("岩オブジェクトをアクティブにしました。");
            }

            Rigidbody2D rockRb = fallingRockObject.GetComponent<Rigidbody2D>();
            if (rockRb != null)
            {
                // bodyTypeをDynamicに変更して重力を有効化
                rockRb.bodyType = RigidbodyType2D.Dynamic;
                Debug.Log("岩のRigidbody2DをDynamicに変更し、落下を開始させます。");

                // 落石解放SEを再生
                if (audioSource != null && rockReleaseSound != null)
                {
                    audioSource.PlayOneShot(rockReleaseSound);
                }
            }
            else
            {
                Debug.LogError("落石オブジェクトにRigidbody2Dがアタッチされていません！");
            }

            FallingRock rockScript = fallingRockObject.GetComponent<FallingRock>();
            if (rockScript != null)
            {
                // 岩のスクリプトにカメラ情報を渡して落下シーケンスを開始
                rockScript.StartFall(rockFollowCamera);
                Debug.Log("FallingRockスクリプトのStartFallを呼び出しました。");
            }
            else
            {
                Debug.LogError("落石オブジェクトにFallingRockスクリプトがアタッチされていません！");
            }
        }
        else
        {
            Debug.LogWarning("Falling Rock Objectがインスペクターで設定されていません！");
        }

        yield return new WaitForSeconds(fallingRockCooldown);
        isCooldown = false;
    }

    // --- ひっつき爆弾処理 ---
    private IEnumerator FireStickyBomb()
    {
        isCooldown = true;

        if (stickyBombPrefab != null && stickyBombFirePoint != null)
        {
            GameObject bomb = Instantiate(stickyBombPrefab, stickyBombFirePoint.position, stickyBombFirePoint.rotation);
            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(Vector2.down * stickyBombThrowForce, ForceMode2D.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("ひっつき爆弾のPrefabまたはFirePointが設定されていません！");
        }

        yield return new WaitForSeconds(stickyBombCooldown);
        isCooldown = false;
    }


    // --- ボス突進へのカウンター判定 ---
}