using Unity.VisualScripting;
using UnityEngine;

public class HintFlowerTalk : MonoBehaviour
{
    //  ヒント花のオブジェクト花
    [SerializeField] private GameObject HintFlower;

    //  プレイヤー座標
    [SerializeField] private GameObject Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    //  バグってわからん
    // Update is called once per frame
    //void Update()
    //{
    //    //　花の座標取得
    //    Vector3 FlowerPosition = transform.position;

    //    //　プレイヤーの座標取得
    //    Vector3 PlayerPosition = Player.transform.position;

    //    //  前提プレイヤーの方が高い座標
    //    //  プレイヤーが花+10を越えたら消える
    //    if (FlowerPosition.x + 10 >= PlayerPosition.x)
    //    {
    //        Destroy(HintFlower);
    //    }
    //}
}
