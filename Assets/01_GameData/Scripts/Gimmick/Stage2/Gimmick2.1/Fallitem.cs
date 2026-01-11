using UnityEngine;

public class Fallitem : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void EnableFall()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void ResetFall()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}