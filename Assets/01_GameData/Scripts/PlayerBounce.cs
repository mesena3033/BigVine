using UnityEngine;

public class PlayerBounce : MonoBehaviour
{
    [SerializeField] private float bounceForce = 6f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    var v = rb.linearVelocity;
                    v.y = bounceForce;
                    rb.linearVelocity = v;
                    break;
                }
            }
        }
    }

}
