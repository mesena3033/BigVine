
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 5f;
    public float rushSpeed = 10f;
    public float rushDuration = 1f;
    public float cooldownTime = 2f;

    private Vector3 targetPosition;
    private bool isRushing = false;
    private bool isCooldown = false;

    private SpriteRenderer sr;
    private Color OriginalColor;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            OriginalColor = sr.color;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player not found! Assign in Inspector or set Player tag.");
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log($"距離: {distance}");

        if (!isRushing && !isCooldown && distance <= detectionRange)
        {
            Debug.Log("プレイヤー感知 → 突進開始");
            targetPosition = player.position;
            //StartCoroutine(Rush());

            //Debug.Log($"参照しているPlayer: {player.name}, 座標: {player.position}");
            // 突進前の点滅 → その後 Rush 開始
            StartCoroutine(FlashBeforeRush());
        }
    }

    // 突進前に一瞬だけ赤く点滅する
    private System.Collections.IEnumerator FlashBeforeRush()
    {
        isRushing = true;

        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.3f); // 点滅時間
            sr.color = OriginalColor;
        }

        // 点滅後に突進開始
        StartCoroutine(Rush());
    }



    private System.Collections.IEnumerator Rush()
    {
        isRushing = true;
        float elapsed = 0f;

        while (elapsed < rushDuration)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, rushSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isRushing = false;
        StartCoroutine(Cooldown());
    }

    private System.Collections.IEnumerator Cooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
