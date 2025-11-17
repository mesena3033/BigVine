using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerHP : MonoBehaviour
{
    public int MaxHP = 3;
    public int NowHP;
    public float InvincibleTime = 2f;
    private bool isInvincible = false;

    [Header("UI Settings")]
    public GameObject HeartPrefab;
    public Transform HeartParent;
    public SpriteRenderer sr;

    private List<GameObject> hearts = new List<GameObject>();

    public Transform RespawnPoint;

    //  レイヤーID(Player, Enemy)
    private int PlayerLayer;
    private int EnemyLayer;

    void Start()
    {
        //  レイヤーID取得
        PlayerLayer = gameObject.layer;
        EnemyLayer = LayerMask.NameToLayer("Enemy");

        sr = GetComponentInChildren<SpriteRenderer>();
        NowHP = MaxHP;

        for (int i = 0; i < MaxHP; i++)
        {
            GameObject h = Instantiate(HeartPrefab, HeartParent);
            hearts.Add(h);
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        NowHP -= damage;
        if (NowHP <= 0)
        {
            NowHP = 0;
            Die();
        }

        UpdateUI();
        StartCoroutine(InvincibleCoroutine());
    }
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        // ダメージ後に一定時間、Enemy / EnemyBullet との衝突を無効化して「すり抜け」仕様にする
        if (PlayerLayer >= 0)
        {
            if (EnemyLayer >= 0) Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, true);
        }

        float elapsed = 0f;

        while (elapsed < InvincibleTime)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(0.2f);

            sr.enabled = true;
            yield return new WaitForSeconds(0.2f);

            elapsed += 0.4f;
        }

        sr.enabled = true;
        isInvincible = false;

        // 一定時間経過後、衝突を元に戻す
        if (PlayerLayer >= 0)
        {
            if (EnemyLayer >= 0) Physics2D.IgnoreLayerCollision(PlayerLayer, EnemyLayer, false);
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
        NowHP = MaxHP;
        UpdateUI();

        if (RespawnPoint != null)
        {
            transform.position = RespawnPoint.position;
        }
        else
        {
            Debug.LogWarning("ReSpawnPoint が設定されていません！");
        }
        SceneManager.LoadScene("GameOver");
    }

    private void UpdateUI()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].SetActive(i < NowHP);
        }
    }
    public void HealFull()
    {
        NowHP = MaxHP;
        UpdateUI();
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            //foreach (ContactPoint2D point in collision.contacts)
            //{
            //    if (point.normal.y > 0.5f)
            //    {
            //        Debug.Log("Enemy stomped!");
            //        return;
            //    }
            //}
            TakeDamage(1);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
        {
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }
}
