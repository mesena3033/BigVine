using UnityEngine;
using UnityEngine.UI; 
using TMPro;     

public class BossHPUIController : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private Slider hpSlider; // HPバーのスライダー
    [SerializeField] private TextMeshProUGUI hpText; // HP数値のテキスト

    private BossController boss;

    void Start()
    {
        // シーンにいるボスを探して取得
        boss = FindFirstObjectByType<BossController>();

        if (boss != null)
        {
            // ボスのOnHPChangedイベント（合図）が来たら、
            // このスクリプトのUpdateHPDisplayメソッドが実行されるように予約する
            boss.OnHPChanged.AddListener(UpdateHPDisplay);
        }
        else
        {
            // ボスがいないならUI自体を非表示にする
            Debug.LogWarning("ボスが見つからないため、HP UIを非表示にします。");
            gameObject.SetActive(false);
        }
    }

    // HP表示を更新するためのメソッド
    private void UpdateHPDisplay(int currentHp, int maxHp)
    {
        // スライダーの値を更新 (例: 50/100HPなら0.5になる)
        if (hpSlider != null)
        {
            hpSlider.value = (maxHp > 0) ? (float)currentHp / maxHp : 0;
        }

        // テキストの表示を更新 (例: "50 / 100")
        if (hpText != null)
        {
            hpText.text = $"{currentHp} / {maxHp}";
        }

        // もしボスのHPが0以下になったら、UIを非表示にする
        if (currentHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    // ゲームオブジェクトが破棄されるときに自動で呼ばれる
    void OnDestroy()
    {
        // メモリリークを防ぐため、イベントの予約を解除しておく
        if (boss != null)
        {
            boss.OnHPChanged.RemoveListener(UpdateHPDisplay);
        }
    }
}