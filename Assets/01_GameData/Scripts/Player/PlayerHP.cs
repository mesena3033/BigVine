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

    void Start()
    {
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
            Debug.LogWarning("ReSpawnPoint ‚ªÝ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñI");
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
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (point.normal.y > 0.5f)
                {
                    Debug.Log("Enemy stomped!");
                    return;
                }
            }
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
