using UnityEngine;

public class AimCursor : MonoBehaviour
{
    public Camera mainCamera;

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        Vector2 pos2D = new Vector2(worldPos.x, worldPos.y);
        transform.position = pos2D;
    }
}
