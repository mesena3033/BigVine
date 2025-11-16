using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ThornRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float delayBeforeRespawn = 0.001f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RespawnAfterDelay(other.gameObject));
        }
    }

    private IEnumerator RespawnAfterDelay(GameObject player)
    {

        yield return new WaitForSeconds(delayBeforeRespawn);

        SceneManager.LoadScene("GameOver");
    }
}
