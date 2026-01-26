//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.SceneManagement;

//public class GameClearController : MonoBehaviour
//{
//    private PlayerInput playerInput;
//    private InputAction submitAction;

//    void Awake()
//    {
//        playerInput = GetComponent<PlayerInput>();
//        submitAction = playerInput.actions["Submit"];
//    }

//    void OnEnable()
//    {
//        submitAction.performed += OnSubmit;
//    }

//    void OnDisable()
//    {
//        submitAction.performed -= OnSubmit;
//    }

//    private void OnSubmit(InputAction.CallbackContext context)
//    {
//        GameManager.Instance.ResetStage1Count();
//        GameManager.Instance.ResetStage2Count();
//        GameManager.Instance.ResetStageCount();

//        // Spaceキー or コントローラーのSouthボタンでMainSceneへ
//        SceneManager.LoadScene("Title");
//    }
//}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameClearController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction submitAction;

    // ▼▼▼ 画像切り替え用の変数を追加 ▼▼▼
    [Header("形態ごとのクリア画像（Sprite Renderer または Image）")]
    [SerializeField] private GameObject clearImageForm1; // 第一形態エンド用 (0～10)
    [SerializeField] private GameObject clearImageForm2; // 第二形態エンド用 (11～18)
    [SerializeField] private GameObject clearImageForm3; // 最終形態エンド用 (19～)
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            submitAction = playerInput.actions["Submit"];
        }
    }

    // ▼▼▼ Startメソッドを追加して画像を切り替える ▼▼▼
    void Start()
    {
        // 1. まず全部非表示にする（リセット）
        if (clearImageForm1) clearImageForm1.SetActive(false);
        if (clearImageForm2) clearImageForm2.SetActive(false);
        if (clearImageForm3) clearImageForm3.SetActive(false);

        // 2. GameManagerから魔法の回数を取得
        if (GameManager.Instance != null)
        {
            int count = GameManager.Instance.growthMagicCount;
            Debug.Log("クリア時の魔法回数: " + count);

            // 3. ボスの形態条件と同じ数字で分岐して表示
            if (count <= 10)
            {
                // 第一形態エンド
                if (clearImageForm1) clearImageForm1.SetActive(true);
            }
            else if (count <= 18)
            {
                // 第二形態エンド
                if (clearImageForm2) clearImageForm2.SetActive(true);
            }
            else
            {
                // 最終形態エンド
                if (clearImageForm3) clearImageForm3.SetActive(true);
            }
        }
        else
        {
            // エラー対策：GameManagerがなければとりあえずForm3を出す
            if (clearImageForm3) clearImageForm3.SetActive(true);
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    void OnEnable()
    {
        if (submitAction != null) submitAction.performed += OnSubmit;
    }

    void OnDisable()
    {
        if (submitAction != null) submitAction.performed -= OnSubmit;
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        // GameManagerのリセット処理
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetStage1Count();
            GameManager.Instance.ResetStage2Count();
            // GameManager.Instance.ResetStageCount(); // ※もしこのメソッドが無ければ消してください
        }

        // Titleシーンへ戻る
        SceneManager.LoadScene("Title");
    }
}