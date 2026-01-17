using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleButton : MonoBehaviour
{

    [SerializeField] private string nextSceneName;


    public void OnClick()
    {
        Debug.Log("BackToTitle �Ă΂ꂽ");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetStage1Count();
            GameManager.Instance.ResetStage2Count();
        }

        SceneManager.LoadScene(nextSceneName);
    }
}