using UnityEngine;

public class LeafRotationReset : MonoBehaviour
{
    [SerializeField] private Transform leaf1;
    [SerializeField] private Transform leaf2;
    [SerializeField] private string enemyLayerName = "Enemy";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(enemyLayerName))
        {
            ResetRotation();
        }
    }

    private void ResetRotation()
    {
        if (leaf1 != null) leaf1.rotation = Quaternion.Euler(0f, 0f, 0f);
        if (leaf2 != null) leaf2.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}

