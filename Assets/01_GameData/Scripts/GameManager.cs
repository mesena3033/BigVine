using UnityEngine;

public class GameManager : MonoBehaviour
{
    // シングルトンインスタンスを保持する静的変数
    public static GameManager Instance { get; private set; }

    // 成長魔法を植物に当てた回数を保持する変数
    public int growthMagicCount = 0;

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでもこのオブジェクトを破棄しない
        }
        else
        {
            Destroy(gameObject); // 既にインスタンスが存在する場合は自身を破棄
        }
    }

    // カウントを1増やすための公開メソッド
    public void IncrementGrowthMagicCount()
    {
        growthMagicCount++;
        Debug.Log("成長魔法カウント: " + growthMagicCount); // デバッグ用にログを出力
    }
}