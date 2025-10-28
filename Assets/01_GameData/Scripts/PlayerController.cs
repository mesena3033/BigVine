using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rbody;
    float axisH = 0.0f;
    public float Speed = 3.0f;
    public float JumpPw = 9.0f;
    public LayerMask groundLayer;
    bool goJump = false;

    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
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
            rbody.AddForce(Vector2.up * JumpPw, ForceMode2D.Impulse);//transform‚Å’²®
            goJump = false;
        }
    }

    public void Jump()
    {
        goJump = true;
    }
}