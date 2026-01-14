using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使うため
using System.Text;
using UnityEngine.InputSystem; // Input Systemを使うため

public class DebugViewer : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject debugPanel;      // 全体のパネル
    [SerializeField] private TextMeshProUGUI statusText; // 魔法回数などを出すテキスト
    [SerializeField] private TextMeshProUGUI logText;    // ログを出すテキスト
    [SerializeField] private ScrollRect scrollRect;      // スクロールビュー

    [Header("キー設定")]
    [SerializeField] private Key toggleKey = Key.F2;     // 切り替えキー（デフォルトF2）

    // ログを溜めておくためのビルダー
    private StringBuilder logBuilder = new StringBuilder();

    // --------------------------------------------------
    // ログの受信設定
    // --------------------------------------------------
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    // --------------------------------------------------
    // メインループ
    // --------------------------------------------------
    void Update()
    {
        // 1. キー入力を検知して表示切り替え
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            TogglePanel();
        }

        // 2. パネルが開いている時だけ、ステータス情報を更新（負荷対策）
        if (debugPanel.activeSelf)
        {
            UpdateStatus();
        }
    }

    // --------------------------------------------------
    // 機能メソッド
    // --------------------------------------------------

    // 表示・非表示の切り替え
    void TogglePanel()
    {
        bool isActive = !debugPanel.activeSelf;
        debugPanel.SetActive(isActive);

        // 開いた瞬間にスクロールを一番下（最新）に合わせる
        if (isActive)
        {
            // 少し待たないとUI更新が間に合わないことがあるので強制更新
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // GameManagerの数値を画面に反映
    void UpdateStatus()
    {
        if (GameManager.Instance != null)
        {
            statusText.text =
                $"Total Magic: {GameManager.Instance.growthMagicCount}\n" +
                $"Stage 1: {GameManager.Instance.stage1Count}\n" +
                $"Stage 2: {GameManager.Instance.stage2Count}";
        }
        else
        {
            statusText.text = "GameManager not found.";
        }
    }

    // Unityのログを受け取ってテキストに追加する
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 色分け（エラーは赤、警告は黄色）
        string color = "white";
        if (type == LogType.Error || type == LogType.Exception) color = "red";
        else if (type == LogType.Warning) color = "yellow";

        // テキストに追加
        logBuilder.Append($"<color={color}>[{type}] {logString}</color>\n");

        // 文字数が多すぎたら古いものを消す（メモリ対策: 5000文字）
        if (logBuilder.Length > 5000)
        {
            logBuilder.Remove(0, logBuilder.Length - 5000);
        }

        // UIに反映
        if (logText != null)
        {
            logText.text = logBuilder.ToString();

            // パネルが開いているなら自動スクロール
            if (debugPanel.activeSelf)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}