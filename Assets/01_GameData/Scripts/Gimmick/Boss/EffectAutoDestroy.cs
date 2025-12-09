using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.0f; // 消えるまでの時間

    void Start()
    {
        // lifetime秒後に、このオブジェクトを破棄する
        Destroy(gameObject, lifetime);
    }
}
