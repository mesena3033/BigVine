using UnityEngine;

public class MoveLeftWithRigidbody : MonoBehaviour
{
    public float moveSpeed = 2f; // �ړ����x�iInspector�Œ����\�j
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(-moveSpeed, 0f); // �������Ɉ�葬�x�ňړ�
    }

    void FixedUpdate()
    {
        // ���t���[�����x���ێ��i���̗͂Ŏ~�܂�Ȃ��悤�Ɂj
        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
    }
}