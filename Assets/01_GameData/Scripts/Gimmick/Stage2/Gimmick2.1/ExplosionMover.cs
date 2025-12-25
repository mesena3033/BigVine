using UnityEngine;

public class ExplosionMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;   // 上に飛ぶスピード
    [SerializeField] private float moveTime = 1f;    // 上に飛んでいる時間
    [SerializeField] private float waitTime = 0.5f;  // ★上で止まる時間
    [SerializeField] private float returnSpeed = 3f; // 戻るスピード

    private bool isMovingUp = false;
    private bool isWaiting = false;   // ★滞空中
    private bool isReturning = false;

    private float timer = 0f;
    private Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.position;
    }

    public void StartMoving()
    {
        isMovingUp = true;
        isWaiting = false;
        isReturning = false;
        timer = 0f;
    }

    private void Update()
    {
        // ★ 上に飛ぶ
        if (isMovingUp)
        {
            timer += Time.deltaTime;
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            if (timer >= moveTime)
            {
                isMovingUp = false;
                isWaiting = true;   // ★滞空開始
                timer = 0f;
            }
        }

        // ★ 上で止まる（滞空）
        if (isWaiting)
        {
            timer += Time.deltaTime;

            if (timer >= waitTime)
            {
                isWaiting = false;
                isReturning = true; // ★戻り開始
            }
        }

        // ★ 元の位置に戻る
        if (isReturning)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                originalPos,
                returnSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, originalPos) < 0.01f)
            {
                isReturning = false;
            }
        }
    }
}