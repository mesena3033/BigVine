using UnityEngine;

public class BreakableFloor : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private string rockLayerName = "Gimmick";

    private bool isFalling = false;
    private float fallTimer = 0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(rockLayerName))
        {
            isFalling = true;
        }
    }

    private void Update()
    {
        if (isFalling)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            fallTimer += Time.deltaTime;

            if (fallTimer >= destroyDelay)
            {
                Destroy(gameObject);
            }
        }
    }

}
