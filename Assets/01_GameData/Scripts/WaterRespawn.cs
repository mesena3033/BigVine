using UnityEngine;
using System.Collections;

public class WaterRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float delayBeforeRespawn = 1.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RespawnAfterDelay(other.gameObject));
        }
;
    }

    private IEnumerator RespawnAfterDelay(GameObject player)
    {
        yield return new WaitForSeconds(delayBeforeRespawn);

        player.transform.position = respawnPoint.position;
    }
}
