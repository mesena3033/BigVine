using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // リストを使うために必要

public class BossHPUI_Icon : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("ボスのHPアイコン（ハートやドクロ）のプレハブ")]
    [SerializeField] private GameObject hpIconPrefab;

    [Tooltip("アイコンを並べる親オブジェクト（Grid/Horizontal Layout Groupがついている所）")]
    [SerializeField] private Transform iconContainer;

    // 生成したアイコンを管理するリスト
    private List<GameObject> iconList = new List<GameObject>();

    private BossController boss;

    void Start()
    {
        // シーンにいるボスを探して取得
        // ※Unity 6 (2023以降) なら FindFirstObjectByType 推奨
        // ※古いバージョンなら FindObjectOfType<BossController>()
        boss = FindFirstObjectByType<BossController>();

        if (boss != null)
        {
            // ボスのHP変更イベントに登録
            // これにより、ボスのHPが変わるたびに UpdateHPDisplay が自動で呼ばれる
            boss.OnHPChanged.AddListener(UpdateHPDisplay);

            // ※補足：BossControllerのStart()で初期イベントが呼ばれるはずなので、
            // ここで手動で呼ぶ必要はありませんが、もし表示されない場合は
            // ここで初期化処理を入れることも可能です。
        }
        else
        {
            Debug.LogWarning("シーン内に BossController が見つかりません。UIを非表示にします。");
            gameObject.SetActive(false);
        }
    }

    // イベントから呼ばれる更新処理
    // currentHp: 現在のHP, maxHp: 最大HP
    private void UpdateHPDisplay(int currentHp, int maxHp)
    {
        // アイコンの数が合っていない場合（初回や最大HP変化時）、作り直す
        if (iconList.Count != maxHp)
        {
            InitializeIcons(maxHp);
        }

        // 現在のHPに合わせて表示/非表示を切り替える
        for (int i = 0; i < iconList.Count; i++)
        {
            // i番目のアイコンは、HPが(i+1)以上なら表示
            // 例: HP3なら、0,1,2番目は表示(Active)、3番目以降は非表示(Inactive)
            bool shouldShow = i < currentHp;

            // 既に状態が同じならSetActiveを呼ばない（負荷軽減）
            if (iconList[i].activeSelf != shouldShow)
            {
                iconList[i].SetActive(shouldShow);
            }
        }

        // HPが0以下ならパネルごと消すなどの処理があればここに追加
    }

    // アイコンを生成して並べる関数
    private void InitializeIcons(int maxHp)
    {
        // 既存のアイコンを全部消す（リセット）
        foreach (GameObject icon in iconList)
        {
            Destroy(icon);
        }
        iconList.Clear();

        // 最大HPの数だけ生成
        for (int i = 0; i < maxHp; i++)
        {
            if (hpIconPrefab != null && iconContainer != null)
            {
                GameObject newIcon = Instantiate(hpIconPrefab, iconContainer);
                iconList.Add(newIcon);
            }
        }
    }

    // オブジェクト破棄時の後始末
    void OnDestroy()
    {
        if (boss != null)
        {
            boss.OnHPChanged.RemoveListener(UpdateHPDisplay);
        }
    }
}