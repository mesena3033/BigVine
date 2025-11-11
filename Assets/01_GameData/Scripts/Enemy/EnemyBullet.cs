using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("弾の速度（単位：m/s）")]
    public float speed = 10f;

    [Header("弾の寿命（秒）")]
    public float lifeTime = 5f;

    void Start()
    {
        // 一定時間後に自動で削除
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 弾を前方（ローカルZ軸）に移動
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

   
    void OnTriggerEnter(Collider other)
    {
        // 例：プレイヤーや壁に当たったら消える
        Destroy(gameObject);
    }
    
}

