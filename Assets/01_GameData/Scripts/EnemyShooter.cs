using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("発射する弾のプレハブ（EnemyBullet をアサイン）")]
    public GameObject bulletPrefab;

    [Header("発射間隔（秒）")]
    public float shootInterval = 3f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= shootInterval)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null)
        {
            // このオブジェクトの中心から発射
            Instantiate(bulletPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("bulletPrefab が設定されていません。");
        }
    }
}
