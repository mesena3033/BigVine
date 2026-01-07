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

    private void OnSubmit(InputAction.CallbackContext context)
    {
        // Spaceキー or コントローラーのSouthボタンでMainSceneへ
        SceneManager.LoadScene("Stage1");
    }
}
