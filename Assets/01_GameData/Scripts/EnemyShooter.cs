using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("���˂���e�̃v���n�u�iEnemyBullet ���A�T�C���j")]
    public GameObject bulletPrefab;

    [Header("���ˊԊu�i�b�j")]
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
            // ���̃I�u�W�F�N�g�̒��S���甭��
            Instantiate(bulletPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("bulletPrefab ���ݒ肳��Ă��܂���B");
        }
    }
}
