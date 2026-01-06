using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    //  弾の移動スピード
    [SerializeField] private float BulletSpeed;

    //  弾が何秒残るか
    [Tooltip("弾が残る秒数")]
    [SerializeField] private float LifeTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //  一定時間で消える
        Destroy(gameObject, LifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        //  左(前)に移動する
        this.transform.Translate(BulletSpeed * Time.deltaTime, 0, 0);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // 他のオブジェクトに当たったら消える
        Destroy(gameObject);
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Default") &&
            other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            // 他のオブジェクトに当たったら消える
            Destroy(gameObject);
        }
    }
}
