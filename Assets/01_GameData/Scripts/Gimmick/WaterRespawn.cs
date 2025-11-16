using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WaterRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float delayBeforeRespawn = 2.5f;

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

        // HPリセット処理はそのまま
        PlayerHP hp = player.GetComponent<PlayerHP>();
        if (hp != null)
        {
            hp.NowHP = hp.MaxHP;
            hp.HealFull(); // UI更新も兼ねているならこれで十分
        }

        // ここでシーン遷移
        SceneManager.LoadScene("GameOver");
    }

}
