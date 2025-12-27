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
    [SerializeField] private float shieldDuration = 3.0f; // シールド展開時間

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
            Rigidbody2D rockRb = fallingRockObject.GetComponent<Rigidbody2D>();
            if (rockRb != null)
            {
                rockRb.bodyType = RigidbodyType2D.Dynamic;
            }
            else
            {
                Debug.LogError("落石オブジェクトにRigidbody2Dがアタッチされていません！");
            }

            FallingRock rockScript = fallingRockObject.GetComponent<FallingRock>();
            if (rockScript != null)
            {
                rockScript.StartFall(rockFollowCamera);
            }
            else
            {
                Debug.LogError("落石オブジェクトにFallingRockスクリプトがアタッチされていません！");
            }
        }
        else
        {
            Debug.LogWarning("Falling Rock Objectが設定されていません！");
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