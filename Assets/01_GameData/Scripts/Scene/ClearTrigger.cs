using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class ClearTrigger : MonoBehaviour
{
    // 遷移先のシーン名（Inspectorから変更可能）
    [SerializeField] private string nextSceneName = "GameClear";

    // 2Dの当たり判定（Is TriggerがONのもの）に入った時に呼ばれる
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 触れたのが「Player」というタグがついたオブジェクトなら
        if (collision.CompareTag("Player"))
        {
            //仮
            SceneMemory.currentStage = nextSceneName;

            // シーンをロードする
            SceneManager.LoadScene(nextSceneName);
        }
    }
}