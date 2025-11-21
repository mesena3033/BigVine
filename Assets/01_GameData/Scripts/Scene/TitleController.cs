using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction submitAction;
    private InputAction toggleBgmAction;
    private AudioSource audioSource;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        submitAction = playerInput.actions["Submit"];
        toggleBgmAction = playerInput.actions["ToggleBGM"];
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        submitAction.performed += OnSubmit;
        if (toggleBgmAction != null)
            toggleBgmAction.performed += OnToggleBgm;
    }

    void OnDisable()
    {
        submitAction.performed -= OnSubmit;
        if (toggleBgmAction != null)
            toggleBgmAction.performed -= OnToggleBgm;
    }
    private void OnSubmit(InputAction.CallbackContext context)
    {
        // Spaceキー or コントローラーのSouthボタンでMainSceneへ
        SceneManager.LoadScene("MainGame");
    }
    private void OnToggleBgm(InputAction.CallbackContext context)
    {
        // AudioSource が存在しなければ何もしない
        if (audioSource == null)
            return;

        // 再生中なら停止、停止中なら再生
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        else
        {
            audioSource.Play();
        }
    }

}
