using UnityEngine;

public class LeafGrow : MonoBehaviour
{
    [SerializeField] private Transform leafVisual;
    [SerializeField] private Vector3 grownScale = new Vector3(5f, 1f, 1f);

    private bool hasGrown = false;

    private void Update()
    {
        if (hasGrown) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (hit != null && hit.gameObject == this.gameObject)
            {
                Grow();
            }
        }
    }

    private void Grow()
    {
        hasGrown = true;
        leafVisual.localScale = grownScale;
    }

}
