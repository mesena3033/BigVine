using UnityEngine;

public class MagicClickTrigger : MonoBehaviour
{
    [SerializeField] private GameObject vineObject;

    private void Start()
    {
        if (vineObject != null)
        {
            vineObject.SetActive(false);
        }
    }
    private void OnMouseDown()
    {
        if (vineObject != null)
        {
            vineObject.SetActive(true);
        }
    }

}
