using UnityEngine;

public class FlowerToFruit : MonoBehaviour
{
    [SerializeField] private GameObject flowerImage; // ‰Ô‚Ì‰æ‘œ
    [SerializeField] private GameObject fruitImage;  // ŽÀ‚Ì‰æ‘œ

    private bool isChanged = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("MagicBullet")) return;

        flowerImage.SetActive(false);
        fruitImage.SetActive(true);
    }
}