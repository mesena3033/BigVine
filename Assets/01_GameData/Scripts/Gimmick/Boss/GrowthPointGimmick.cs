using UnityEngine;
using System.Collections;

public class GrowthPointGimmick : MonoBehaviour
{
    public enum GimmickType { Turret, IvyShield, FallingRock }
    public GimmickType type;

    [Header("共通設定")]
    public int damageAmount = 1;

    [Header("ツタシールド (IvyShield) 用設定")]
    [SerializeField] private GameObject shieldObject; // 子オブジェクトのシールド(IvyShield)
    [SerializeField] private float shieldDuration = 3.0f; // シールド展開時間

    [Header("砲台 (Turret) 用設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireForce = 15f;
    [SerializeField] private float cooldown = 1.0f;
    private bool isCooldown = false;

    // --- 魔法が当たった時の処理 ---
    public void OnMagicHit()
    {
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

    // --- 砲台処理 (前回と同じ) ---
    private IEnumerator FireTurret()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }

    // --- ボス突進へのカウンター判定 ---
}