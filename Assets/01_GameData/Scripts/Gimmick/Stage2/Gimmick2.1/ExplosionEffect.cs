using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.5f; // 0.5ïbå„Ç…è¡Ç¶ÇÈ

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

}
