using UnityEngine;

public class BomPoint : MonoBehaviour
{
    [SerializeField] private GameObject targetPlant;     
    [SerializeField] private GameObject explosionPrefab; // 爆発エフェクト
    [SerializeField] private GameObject hideObject;
    [SerializeField] private float fallTime = 1.5f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private Fallitem fallItem;


    private bool isFalling = false;
    private float timer = 0f;

    private Vector3 plantOriginalPos;
    private bool canTrigger = true;   

    private void Start()
    {
        if (targetPlant != null)
        {
            plantOriginalPos = targetPlant.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTrigger) return;
        if (!other.CompareTag("MagicBullet")) return;

        if (hideObject != null)
        {
            hideObject.SetActive(false);
        }

        if (fallItem != null)
            fallItem.EnableFall();

        isFalling = true;
        timer = 0f;
        canTrigger = false;
    }

    private void Update()
    {
        if (isFalling && targetPlant != null)
        {
            timer += Time.deltaTime;

            targetPlant.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            if (timer >= fallTime)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        if (targetPlant == null) return;

        Vector3 pos = targetPlant.transform.position;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, pos, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, explosionRadius);

        foreach (var h in hits)
        {
            ExplosionMover mover = h.GetComponent<ExplosionMover>();
            if (mover != null)
            {
                mover.StartMoving();
            }
        }

        if (hideObject != null)
        {
            hideObject.SetActive(true);
        }

        if (fallItem != null)
            fallItem.ResetFall();

        targetPlant.transform.position = plantOriginalPos;

        isFalling = false;
        canTrigger = true;
    }
}