using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameClearController : MonoBehaviour
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

    private void OnSubmit(InputAction.CallbackContext context)
    {
        GameManager.Instance.ResetStage1Count();
        GameManager.Instance.ResetStage2Count();
        GameManager.Instance.ResetStageCount();

        // Spaceキー or コントローラーのSouthボタンでMainSceneへ
        SceneManager.LoadScene("Title");
    }
}
