using UnityEngine;
using UnityEngine.Rendering;

public class SimpleParallax : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 0.1f; // ”wŒi‚Ì“®‚­”ä—¦

    private float startPlayerX;
    private float startBgX;

    private void Start()
    {
        startPlayerX = player.position.x;
        startBgX = transform.position.x;
    }

    private void Update()
    {
        float movedX = player.position.x - startPlayerX;

        // ƒvƒŒƒCƒ„[ˆÚ“®—Ê‚Ì speed Š„‡‚¾‚¯“®‚©‚·
        transform.position = new Vector3(
            startBgX + movedX * speed,
            transform.position.y,
            transform.position.z
        );
    }
}