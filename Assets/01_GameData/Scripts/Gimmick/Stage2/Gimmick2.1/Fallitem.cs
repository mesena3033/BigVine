using UnityEngine;

public class Fallitem : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool used = false; // ˆê‰ñ‚¾‚¯—‚¿‚é

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Å‰‚Í“®‚©‚È‚¢
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        // šMagicBullet ‚É“–‚½‚Á‚½H
        if (other.CompareTag("MagicBullet"))
        {
            Fall();
        }
    }

    private void Fall()
    {
        used = true;
        rb.bodyType = RigidbodyType2D.Dynamic; // © ‚±‚ê‚Å—‚¿‚éI
    }
}