using UnityEngine;

public class MagicClickTrigger : MonoBehaviour
{
    [SerializeField] private GameObject vineObject1;
    [SerializeField] private GameObject vineObject2;

    private int triggerCount = 0;

    private void OnMouseDown()
    {
        triggerCount++;

        if (triggerCount == 1 && vineObject1 != null)
        {
            vineObject1.SetActive(true);
        }
        else if (triggerCount == 2 && vineObject2 != null)
        {
            Vector3 basePos = vineObject1.transform.position;
            vineObject2.transform.position = basePos + new Vector3(0, -6.5f, 0);
            vineObject2.SetActive(true);
        }
    }


}
