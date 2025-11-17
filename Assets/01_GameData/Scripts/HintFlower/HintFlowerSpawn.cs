using UnityEngine;

public class HintFlowerSpawn : MonoBehaviour
{
    //  ヒント花のオブジェクト
    [SerializeField] private GameObject HintFlower;

    //　ヒント花の出現場所
    [SerializeField] private GameObject HintFlowerSpawnPoint;

    //  プレイヤー座標
    [SerializeField] private GameObject Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //　花が出る位置の座標取得
        Vector3 FlowerPosition = transform.position;

        //　プレイヤーの座標取得
        Vector3 PlayerPosition = Player.transform.position;

        //　座標確認用
        //Debug.Log("Flower.x :" + FlowerPosition.x + " Flower.y :" + FlowerPosition.y);
        //Debug.Log("Player.x :" + PlayerPosition.x + " Player.y :" + PlayerPosition.y);

        //  前提プレイヤーの方が低い座標
        //  プレイヤーが花-10までいったら現れる
        if (FlowerPosition.x - 10 <= PlayerPosition.x)
        {
            Instantiate(HintFlower, new Vector3(FlowerPosition.x, FlowerPosition.y, 0), Quaternion.identity);
            Destroy(HintFlowerSpawnPoint);
        }
    }
}
