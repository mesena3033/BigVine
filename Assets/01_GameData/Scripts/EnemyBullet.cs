using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("�e�̑��x�i�P�ʁFm/s�j")]
    public float speed = 10f;

    [Header("�e�̎����i�b�j")]
    public float lifeTime = 5f;

    void Start()
    {
        // ��莞�Ԍ�Ɏ����ō폜
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // �e��O���i���[�J��Z���j�Ɉړ�
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

   
    void OnTriggerEnter(Collider other)
    {
        // ��F�v���C���[��ǂɓ��������������
        Destroy(gameObject);
    }
    
}

