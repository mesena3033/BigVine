using UnityEngine;

public class LeafGrow : MonoBehaviour
{
    [SerializeField] private Transform leafVisual;
    [SerializeField] private Vector3 midScale = new Vector3(4f, 0.8f, 1f); 
    [SerializeField] private Vector3 grownScale = new Vector3(5f, 1f, 1f);



    private int growthStage = 0;

    private void Update()
    {
        if (growthStage >= 2) return;

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
        growthStage++;

        if (growthStage == 1)
        {
            leafVisual.localScale = midScale;
        }
        else if (growthStage == 2)
        {
            leafVisual.localScale = grownScale;
        }
    }
}

