using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // シングルトンインスタンスを保持する静的変数
    public static GameManager Instance { get; private set; }

    // 成長魔法を植物に当てた回数を保持する変数
    public int growthMagicCount = 0;

    // ステージごとのカウント保持
    public int stage1Count = 0;
    public int stage2Count = 0;

    //解放されたボスの形態（0:第一, 1:第二, 2:第三）
    // false = 未解放, true = 解放済み
    public bool[] sessionUnlockedBosses = new bool[3];

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
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Stage1")
        {
            stage1Count++;
            Debug.Log("ステージ1カウント:" + stage1Count);
        }
        else if (sceneName == "Stage2")
        {
            stage2Count++;
            Debug.Log("ステージ2カウント:" + stage2Count);
        }

        growthMagicCount = stage1Count + stage2Count;

           // growthMagicCount++;
        Debug.Log("成長魔法カウント: " + growthMagicCount); // デバッグ用にログを出力
    }

    // ステージ2のカウントリセット
    public void ResetStage2Count()
    {
        stage2Count = 0;
    }

    // ステージ1のカウントリセット
    public void ResetStage1Count()
    {
        stage1Count = 0;
    }

    public void ResetStageCount()
    {
        growthMagicCount = 0;
    }
}