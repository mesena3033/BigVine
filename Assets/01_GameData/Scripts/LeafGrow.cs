using UnityEngine;

public class LeafGrow : MonoBehaviour
{
    [SerializeField] private Transform leafVisual;
    [SerializeField] private Vector3 grownScale = new Vector3(5f, 1f, 1f);


    private bool hasGrown = false;

   /* private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasGrown) return;

        if (other.CompareTag("Magic"))
        {
            hasGrown = true;
            leafVisual.localScale = grownScale;
        }
    }*/

    private void OnMouseDown()
    {
        if (hasGrown) return;
        hasGrown = true;
        leafVisual.localScale = grownScale;

    }

}
