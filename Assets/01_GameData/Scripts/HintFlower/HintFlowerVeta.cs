using UnityEngine;

public class HintFlowerVeta : MonoBehaviour
{
    [SerializeField] GameObject HintFlower;

    //  複数回表示しないためのフラグ
    private bool IsActivated = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(HintFlower!=null)
        {
            //  デフォ非表示
            HintFlower.SetActive(false);
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

        if(collision.CompareTag("Player"))
        {
            if(HintFlower!=null)
            {
                HintFlower.SetActive(true);
            }
        }
    }
}
