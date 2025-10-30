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
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero);
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                {
                    Grow();
                    break;
                }
            }

        }
    }
    private void Grow()
    {
        hasGrown = true;
        leafVisual.localScale = grownScale;
    }

}
