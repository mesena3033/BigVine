using UnityEngine;

public class CameraFollowDummy : MonoBehaviour
{
    public Transform player; 
    public float fixedY = 0f; 

    void LateUpdate()
    {
        if (player == null) return;

        Vector2 newPos = new Vector2(player.position.x, fixedY);
        transform.position = newPos;
    }

}
