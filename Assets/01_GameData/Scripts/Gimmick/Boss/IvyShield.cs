using UnityEngine;

public class IvyShield : MonoBehaviour
{
    [Header("効果音")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reflectSound; // 跳ね返しSE

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突した相手がボスの弾（例："BossBullet"タグがついている）かチェック
        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            // 跳ね返しSEを再生
            if (audioSource != null && reflectSound != null)
            {
                audioSource.PlayOneShot(reflectSound);
            }
        }
    }
}
