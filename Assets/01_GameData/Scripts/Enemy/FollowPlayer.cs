
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

    void Start()
    {
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
            StartCoroutine(Rush());

            Debug.Log($"参照しているPlayer: {player.name}, 座標: {player.position}");
        }
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
