using UnityEngine;

public class SecondEndingTrigger : MonoBehaviour
{
    public GameObject playerHand1;
    public GameObject playerHand2;

    public GameObject engineTempObject;
    public GameObject helmateTempObject;
    public GameObject gpsTempObject;
    public GameObject manualTempObject;

    public GameObject timelineTriggerObject;

    private bool engineSubmitted = false;
    private bool helmateSubmitted = false;
    private bool gpsSubmitted = false;
    private bool manualSubmitted = false;

    private void OnMouseDown()
    {
        CheckAndSubmitItem(playerHand1);
        CheckAndSubmitItem(playerHand2);

        if (engineSubmitted && helmateSubmitted && gpsSubmitted && manualSubmitted)
        {
            if (timelineTriggerObject != null)
            {
                timelineTriggerObject.SetActive(true);
                Debug.Log("All items submitted. Timeline triggered.");
            }
        }
    }

    void CheckAndSubmitItem(GameObject hand)
    {
        if (hand.transform.childCount == 0) return;

        Transform heldItem = hand.transform.GetChild(0);
        string tag = heldItem.tag;

        switch (tag)
        {
            case "Engine":
                if (!engineSubmitted)
                {
                    EnableTempObject(engineTempObject);
                    Destroy(heldItem.gameObject);
                    engineSubmitted = true;
                    Debug.Log("Engine submitted.");
                }
                break;

            case "Helmate":
                if (!helmateSubmitted)
                {
                    EnableTempObject(helmateTempObject);
                    Destroy(heldItem.gameObject);
                    helmateSubmitted = true;
                    Debug.Log("Helmate submitted.");
                }
                break;

            case "GPS":
                if (!gpsSubmitted)
                {
                    EnableTempObject(gpsTempObject);
                    Destroy(heldItem.gameObject);
                    gpsSubmitted = true;
                    Debug.Log("GPS submitted.");
                }
                break;

            case "Manual":
                if (!manualSubmitted)
                {
                    EnableTempObject(manualTempObject);
                    Destroy(heldItem.gameObject);
                    manualSubmitted = true;
                    Debug.Log("Manual submitted.");
                }
                break;
        }
    }

    void EnableTempObject(GameObject tempObject)
    {
        if (tempObject != null)
        {
            tempObject.SetActive(true);
        }
    }
}
