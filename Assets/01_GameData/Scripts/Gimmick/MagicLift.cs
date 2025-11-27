using UnityEngine;

public class MagicLift : MonoBehaviour
{
    [SerializeField] private Rigidbody2D platformRB;
    [SerializeField] private float liftAmount = 3f;
    [SerializeField] private float stayTime = 2f;
    [SerializeField] private float upSpeed = 20f;     // ★上昇スピード
    [SerializeField] private float downSpeed = 14f;   // ★下降スピード

    public bool playerOnPlatform = false;

    private Vector3 originalPos;
    private Vector3 liftedPos;
    private bool isMoving = false;

    private void Start()
    {
        originalPos = platformRB.position;
        liftedPos = originalPos + Vector3.up * liftAmount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isMoving) return;
        if (!other.CompareTag("MagicBullet")) return;

        StopAllCoroutines();
        StartCoroutine(LiftThenReturn());
    }

    private System.Collections.IEnumerator LiftThenReturn()
    {
        isMoving = true;

        // ★ 上向きにスムーズに移動（吹き飛び防止）
        while (Vector3.Distance(platformRB.position, liftedPos) > 0.01f)
        {
            Vector3 target = Vector3.MoveTowards(platformRB.position, liftedPos, upSpeed * Time.deltaTime);
            platformRB.MovePosition(target);
            yield return null;
        }
        platformRB.MovePosition(liftedPos);

        yield return new WaitForSeconds(stayTime);

        // ★ 下向きにスムーズに戻る
        while (Vector3.Distance(platformRB.position, originalPos) > 0.01f)
        {
            Vector3 target = Vector3.MoveTowards(platformRB.position, originalPos, downSpeed * Time.deltaTime);
            platformRB.MovePosition(target);
            yield return null;
        }
        platformRB.MovePosition(originalPos);

        isMoving = false;
    }
}
