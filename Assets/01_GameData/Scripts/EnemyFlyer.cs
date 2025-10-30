using UnityEngine;

public class EnemyFlyer : MonoBehaviour
{
    public float moveSpeed = 2f;           // �ړ����x
    public float switchInterval = 2f;      // �ړ�������؂�ւ���Ԋu�i�b�j
    public Vector3[] directions;           // �ړ������̔z��

    private int currentDirectionIndex = 0;
    private float timer = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // �����ʒu���L�^
        if (directions == null || directions.Length == 0)
        {
            // �f�t�H���g�̕����i���E�j
            directions = new Vector3[] { Vector3.right, Vector3.left };
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // ��莞�Ԃ��Ƃɕ�����؂�ւ���
        if (timer >= switchInterval)
        {
            currentDirectionIndex = (currentDirectionIndex + 1) % directions.Length;
            timer = 0f;
        }

        // �󒆂ɌŒ�iY���̈ʒu���ێ��j
        Vector3 move = directions[currentDirectionIndex] * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x + move.x,
            startPosition.y, // Y���͌Œ�
            transform.position.z + move.z
        );
    }
}
