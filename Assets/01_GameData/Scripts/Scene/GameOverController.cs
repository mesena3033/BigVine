using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction submitAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        submitAction = playerInput.actions["Submit"];
    }

    void OnEnable()
    {
        submitAction.performed += OnSubmit;
    }

    void OnDisable()
    {
        submitAction.performed -= OnSubmit;
    }


    //仮
    void Start()
    {
        if (SceneMemory.currentStage == "Stage1")
        {
            Debug.Log("ステージ1から来た！");
        }
        else if (SceneMemory.currentStage == "Stage2")
        {
            Debug.Log("ステージ2から来た！");
        }
        else if (SceneMemory.currentStage == "LastBoss")
        {
            Debug.Log("ボスステージから来た");
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        // Spaceキー or コントローラーのSouthボタンでMainSceneへ
        SceneManager.LoadScene("Stage1");
    }
}
