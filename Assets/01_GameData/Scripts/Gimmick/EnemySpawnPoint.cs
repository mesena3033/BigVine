
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("スポーンする敵のPrefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("スポーン位置のオフセット")]
    [SerializeField] private Vector2 spawnOffset = Vector2.zero;

    [Header("一度だけスポーンするか")]
    [SerializeField] private bool spawnOnce = true;

    private bool hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawnOnce && hasSpawned) return;

            Vector2 spawnPos = (Vector2)transform.position + spawnOffset;
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            hasSpawned = true;
        }
    }
}
