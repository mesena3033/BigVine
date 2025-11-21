using UnityEngine;

public class PlayerCenteredLoop : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("背景3枚（左→右の順）")]
    [SerializeField] private Transform[] backgrounds;

    [Header("天井3枚（左→右の順）")]
    [SerializeField] private Transform[] ceilings;

    private float width;
    private bool canMove = true;

    private void Start()
    {
        // 背景の横幅（3枚とも同じ前提）
        width = backgrounds[0].GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        if (!canMove)
        {
            if (!IsPlayerInCenterArea())
                canMove = true;

            return;
        }

        // 背景を左→右順にソート
        System.Array.Sort(backgrounds, (a, b) => a.position.x.CompareTo(b.position.x));
        System.Array.Sort(ceilings, (a, b) => a.position.x.CompareTo(b.position.x));

        Transform bgLeft = backgrounds[0];
        Transform bgRight = backgrounds[2];

        Transform ceilLeft = ceilings[0];
        Transform ceilRight = ceilings[2];

        float centerX = (bgLeft.position.x + bgRight.position.x) * 0.5f;

        // ---- 右へ進んだとき（左の背景＆天井を右へ回す） ----
        if (player.position.x > centerX)
        {
            float newX = bgRight.position.x + width;

            bgLeft.position = new Vector3(newX, bgLeft.position.y, bgLeft.position.z);
            ceilLeft.position = new Vector3(newX, ceilLeft.position.y, ceilLeft.position.z);

            canMove = false;
            return;
        }

        // ---- 左へ進んだとき（右の背景＆天井を左へ回す） ----
        if (player.position.x < centerX)
        {
            float newX = bgLeft.position.x - width;

            bgRight.position = new Vector3(newX, bgRight.position.y, bgRight.position.z);
            ceilRight.position = new Vector3(newX, ceilRight.position.y, ceilRight.position.z);

            canMove = false;
            return;
        }
    }

    // プレイヤーが中央ゾーンにいるか判定
    private bool IsPlayerInCenterArea()
    {
        System.Array.Sort(backgrounds, (a, b) => a.position.x.CompareTo(b.position.x));

        Transform left = backgrounds[0];
        Transform right = backgrounds[2];

        float centerX = (left.position.x + right.position.x) * 0.5f;

        float p = player.position.x;

        return (p > centerX - width * 0.3f && p < centerX + width * 0.3f);
    }
}
