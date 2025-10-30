using UnityEngine;

public class EnemyFlyer : MonoBehaviour
{
    public float moveSpeed = 2f;           // 移動速度
    public float switchInterval = 2f;      // 移動方向を切り替える間隔（秒）
    public Vector3[] directions;           // 移動方向の配列

    private int currentDirectionIndex = 0;
    private float timer = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // 初期位置を記録
        if (directions == null || directions.Length == 0)
        {
            // デフォルトの方向（左右）
            directions = new Vector3[] { Vector3.right, Vector3.left };
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 一定時間ごとに方向を切り替える
        if (timer >= switchInterval)
        {
            currentDirectionIndex = (currentDirectionIndex + 1) % directions.Length;
            timer = 0f;
        }

        // 空中に固定（Y軸の位置を維持）
        Vector3 move = directions[currentDirectionIndex] * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x + move.x,
            startPosition.y, // Y軸は固定
            transform.position.z + move.z
        );
    }
}
