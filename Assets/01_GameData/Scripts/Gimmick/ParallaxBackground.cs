using UnityEngine;

public class PlayerCenteredLoop : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("背景4枚（左→右の順）")]
    [SerializeField] private Transform[] backgrounds;

    private float width;
    private bool canMove = true;

    private void Start()
    {
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

        // 左→右順に並べ替え
        System.Array.Sort(backgrounds, (a, b) => a.position.x.CompareTo(b.position.x));

        Transform bgLeft = backgrounds[0];
        Transform bgRight = backgrounds[backgrounds.Length - 1]; // 4枚目が右端

        float centerX = (bgLeft.position.x + bgRight.position.x) * 0.5f;

        // ---- 右へ進んだとき ----
        if (player.position.x > centerX)
        {
            float newX = bgRight.position.x + width;
            bgLeft.position = new Vector3(newX, bgLeft.position.y, bgLeft.position.z);
            canMove = false;
            return;
        }

        // ---- 左へ進んだとき ----
        if (player.position.x < centerX)
        {
            float newX = bgLeft.position.x - width;
            bgRight.position = new Vector3(newX, bgRight.position.y, bgRight.position.z);
            canMove = false;
            return;
        }
    }

    private bool IsPlayerInCenterArea()
    {
        System.Array.Sort(backgrounds, (a, b) => a.position.x.CompareTo(b.position.x));

        Transform left = backgrounds[0];
        Transform right = backgrounds[backgrounds.Length - 1];

        float centerX = (left.position.x + right.position.x) * 0.5f;
        float p = player.position.x;

        return (p > centerX - width * 0.3f && p < centerX + width * 0.3f);
    }
}