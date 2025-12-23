using UnityEngine;

public class ExplosionMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float moveTime = 10f;

    private bool isMoving = false;
    private float timer = 0f;

    public void StartMoving()
    {
        isMoving = true;
        timer = 0f;
    }

    private void Update()
    {
        if (isMoving)
        {
            timer += Time.deltaTime;
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            if (timer >= moveTime)
            {
                isMoving = false;
            }
        }
    }
}