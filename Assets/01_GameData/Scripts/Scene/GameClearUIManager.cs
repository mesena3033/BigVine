using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameClearUIManager : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TextMeshProUGUI magicCountText; // 回数表示テキスト
    [SerializeField] private Image[] bossImages; // 左から順に第1, 第2, 第3形態のImage

    [Header("スプライト設定")]
    [Tooltip("未解放時のシルエット画像")]
    [SerializeField] private Sprite[] silhouetteSprites;
    [Tooltip("解放時のカラーイラスト画像")]
    [SerializeField] private Sprite[] illustrationSprites;

    // BossControllerと同じ閾値
    private const int FORM1_THRESHOLD = 10;
    private const int FORM2_THRESHOLD = 18;

    void Start()
    {
        // GameManagerがない場合は処理中断
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManagerが存在しません。");
            return;
        }

        // 1. 魔法使用回数の取得と表示
        int count = GameManager.Instance.growthMagicCount;
        magicCountText.text = $"魔法使用回数:{count}回";

        // 2. 今回の結果をGameManagerに記録（アプリ終了まで保持）
        RegisterCurrentResult(count);

        // 3. 画像の更新
        UpdateBossImages();
    }

    /// <summary>
    /// 今回のクリア結果をGameManagerの一時変数に記録する
    /// </summary>
    private void RegisterCurrentResult(int count)
    {
        int clearedIndex = -1;

        if (count <= FORM1_THRESHOLD)
        {
            clearedIndex = 0; // 第一形態
        }
        else if (count <= FORM2_THRESHOLD)
        {
            clearedIndex = 1; // 第二形態
        }
        else
        {
            clearedIndex = 2; // 第三形態
        }

        // 該当する形態のフラグをtrueにする
        // これで、ゲームを再起動するまでは「解放済み」として扱われる
        if (clearedIndex >= 0 && clearedIndex < GameManager.Instance.sessionUnlockedBosses.Length)
        {
            GameManager.Instance.sessionUnlockedBosses[clearedIndex] = true;
            Debug.Log($"今回クリアした形態: 第{clearedIndex + 1}形態 (セッション実績解除)");
        }
    }

    /// <summary>
    /// GameManagerの記録を見て、シルエットかイラストか切り替える
    /// </summary>
    private void UpdateBossImages()
    {
        // 設定チェック
        if (bossImages.Length < 3 || silhouetteSprites.Length < 3 || illustrationSprites.Length < 3) return;

        for (int i = 0; i < 3; i++)
        {
            // GameManagerにある「今回の起動中に解放したか？」フラグをチェック
            bool isUnlocked = GameManager.Instance.sessionUnlockedBosses[i];

            if (isUnlocked)
            {
                bossImages[i].sprite = illustrationSprites[i];
                bossImages[i].color = Color.white;
            }
            else
            {
                bossImages[i].sprite = silhouetteSprites[i];
            }
        }
    }
}