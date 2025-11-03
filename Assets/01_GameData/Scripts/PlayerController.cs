using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rbody;
    Collider2D col;

    float axisH = 0.0f;
    public float Speed = 3.0f;
    public float JumpPw = 9.0f;
    public LayerMask groundLayer;
    bool goJump = false;

    [Header("Physics Materials")]
    [SerializeField] PhysicsMaterial2D normalMat;     
    [SerializeField] PhysicsMaterial2D noFrictionMat; 

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        axisH = Input.GetAxisRaw("Horizontal");

        if (axisH > 0.0f)
        {
            transform.localScale = new Vector2(1, 1);
        }
        else if (axisH < 0.0f)
        {
            transform.localScale = new Vector2(-1, 1);
        }

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        bool onGround = Physics2D.CircleCast(
            transform.position,
            0.6f,
            Vector2.down,
            0.5f,
            groundLayer
        );

        rbody.linearVelocity = new Vector2(Speed * axisH, rbody.linearVelocity.y);

        if (onGround && goJump)
        {
            rbody.AddForce(Vector2.up * JumpPw, ForceMode2D.Impulse);
            goJump = false;
        }
    }

    public void Jump()
    {
        goJump = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector2.Angle(contact.normal, Vector2.up) < 30f)
            {
                col.sharedMaterial = normalMat;
                return;
            }
            else if (Mathf.Abs(Vector2.Angle(contact.normal, Vector2.left)) < 30f ||
                     Mathf.Abs(Vector2.Angle(contact.normal, Vector2.right)) < 30f)
            {
                col.sharedMaterial = noFrictionMat;
                return;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        col.sharedMaterial = normalMat;
    }
}