using UnityEngine;
using TMPro;
using System.Collections;

public class SpeechBubbleController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Vector3 offset = new Vector3(-5, 3f, 0); // 表示位置のオフセット

    private Transform targetToFollow;

    void Update()
    {
        // 追従対象が設定されていれば、その位置に追従する
        if (targetToFollow != null)
        {
            transform.position = targetToFollow.position + offset;
        }
    }

    // 外部からメッセージ表示を指示するためのメソッド
    public void ShowMessage(string message, Transform target)
    {
        messageText.text = message;
        targetToFollow = target;
        gameObject.SetActive(true);
    }

    // 吹き出しを非表示にするメソッド
    public void Hide()
    {
        targetToFollow = null;
        gameObject.SetActive(false);
    }
}