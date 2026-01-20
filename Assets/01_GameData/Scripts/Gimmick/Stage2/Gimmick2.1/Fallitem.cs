using UnityEngine;

public class Fallitem : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 startPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void EnableFall()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void ResetFall()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = startPos;
    }
}
