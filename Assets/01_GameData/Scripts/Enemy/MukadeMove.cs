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

    float yPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rb2d = GetComponent<Rigidbody2D>();

        yPos = transform.position.y;
    }

    private void FixedUpdate()
    {
        yPos = transform.position.y;

        if (yPos < DestroyYMax)
        {
            Vector2 delta = Vector2.up * MoveHoriSpeed * Time.fixedDeltaTime;
            Rb2d.MovePosition(Rb2d.position + delta);
        }
        else
        {
            //Debug.Log("maxを超えた");
        }

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Update呼ばれた");
        //Debug.Log(yPos);

        //  範囲外に出たら消す
        float py = transform.position.y;
        if (py < DestroyYMin /*|| py > DestroyYMax*/)
        {
            Destroy(gameObject);
        }
    }
}
