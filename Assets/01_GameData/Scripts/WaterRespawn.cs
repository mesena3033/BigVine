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

        PlayerHP hp = player.GetComponent<PlayerHP>();
        if (hp != null)
        {
            hp.NowHP = hp.MaxHP; 
            var updateUIMethod = player.GetComponent<PlayerHP>();
            if (updateUIMethod != null)
            {
                player.GetComponent<PlayerHP>().HealFull();
            }
        }
        player.transform.position = respawnPoint.position;

    }
}
