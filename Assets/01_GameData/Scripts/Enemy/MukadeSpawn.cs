using UnityEngine;

public class MukadeSpawn : MonoBehaviour
{
    [SerializeField] GameObject Mukade;

    //  複数回表示しないためのフラグ
    private bool IsActivated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Mukade != null)
        {
            //  デフォ非表示
            Mukade.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  一回表示したなら何もしない
        if (IsActivated) return;

        if (collision.CompareTag("Player"))
        {
            if (Mukade != null)
            {
                Mukade.SetActive(true);
            }
        }
    }
}
