using UnityEngine;

public class MukadeMove : MonoBehaviour
{
    //  移動スピード
    [SerializeField] private float MoveHoriSpeed;

    [Header("消えるライン")]

    //  これ以下／以上で消す
    [SerializeField] private float DestroyYMin;
    [SerializeField] private float DestroyYMax;

    private Rigidbody2D Rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rb2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 delta = Vector2.up * MoveHoriSpeed * Time.deltaTime;
        Rb2d.MovePosition(Rb2d.position + delta);
    }

    // Update is called once per frame
    void Update()
    {
        //  範囲外に出たら消す
        float py = transform.position.y;
        if (py < DestroyYMin || py > DestroyYMax)
        {
            Destroy(gameObject);
        }
    }
}
