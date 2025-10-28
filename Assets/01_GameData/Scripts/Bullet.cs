using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject bulletPrefab;  
    public float bulletSpeed = 10f;  
    public Camera mainCamera;

    private GameObject currentBullet; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentBullet == null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 targetPos = new Vector2(mousePos.x, mousePos.y);

            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

            currentBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = currentBullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = direction * bulletSpeed;

            Destroy(currentBullet, 3f);

            StartCoroutine(ResetBulletAfterDelay(3f));
        }
    }

    private System.Collections.IEnumerator ResetBulletAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentBullet = null;
    }

}
