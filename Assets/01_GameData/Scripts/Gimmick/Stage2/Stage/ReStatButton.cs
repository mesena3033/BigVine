using UnityEngine;
using UnityEngine.SceneManagement;

public class ReStatButton : MonoBehaviour
{
    public void Retry()
    {
        if (SceneMemory.currentStage == "Stage1")
        {
            Debug.Log("ステージ1から来た！");
            SceneManager.LoadScene("Stage1");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetStage1Count();
                GameManager.Instance.ResetStage2Count();
            }
        }
        else if (SceneMemory.currentStage == "Stage2")
        {
            Debug.Log("ステージ2から来た！");
            SceneManager.LoadScene("Stage2");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetStage2Count();
            }
        }
        else if (SceneMemory.currentStage == "LastBoss")
        {
            Debug.Log("ボスステージから来た");
            SceneManager.LoadScene("LastBoss");
        }
    }
}
