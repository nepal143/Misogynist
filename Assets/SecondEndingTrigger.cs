using UnityEngine;
using TMPro;
using System.Collections;

public class SecondEndingTrigger : MonoBehaviour
{
    public GameObject playerHand1;
    public GameObject playerHand2;

    public GameObject engineTempObject;
    public GameObject helmateTempObject;
    public GameObject gpsTempObject;
    public GameObject manualTempObject;

    public GameObject timelineTriggerObject;
    public TextMeshProUGUI infoText; // Assign in Inspector

    private bool engineSubmitted = false;
    private bool helmateSubmitted = false;
    private bool gpsSubmitted = false;
    private bool manualSubmitted = false;

    private Coroutine hideTextCoroutine;

    private void OnMouseDown()
    {
        CheckAndSubmitItem(playerHand1);
        CheckAndSubmitItem(playerHand2);

        if (engineSubmitted && helmateSubmitted && gpsSubmitted && manualSubmitted)
        {
            infoText.text = "";
            infoText.gameObject.SetActive(false);

            if (timelineTriggerObject != null)
            {
                timelineTriggerObject.SetActive(true);
                Debug.Log("✅ All items submitted. Timeline triggered.");
            }
        }
        else
        {
            ShowMissingItems();
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
                }
                break;

            case "Helmate":
                if (!helmateSubmitted)
                {
                    EnableTempObject(helmateTempObject);
                    Destroy(heldItem.gameObject);
                    helmateSubmitted = true;
                }
                break;

            case "GPS":
                if (!gpsSubmitted)
                {
                    EnableTempObject(gpsTempObject);
                    Destroy(heldItem.gameObject);
                    gpsSubmitted = true;
                }
                break;

            case "Manual":
                if (!manualSubmitted)
                {
                    EnableTempObject(manualTempObject);
                    Destroy(heldItem.gameObject);
                    manualSubmitted = true;
                }
                break;
        }
    }

    void ShowMissingItems()
    {
        string message = "<b>Still missing:</b>\n";

        if (!engineSubmitted) message += "• Engine\n";
        if (!helmateSubmitted) message += "• Helmate\n";
        if (!gpsSubmitted) message += "• GPS\n";
        if (!manualSubmitted) message += "• Manual\n";

        infoText.text = message;
        infoText.gameObject.SetActive(true);

        // Start or restart coroutine to hide text after 3 seconds
        if (hideTextCoroutine != null)
            StopCoroutine(hideTextCoroutine);

        hideTextCoroutine = StartCoroutine(HideInfoTextAfterDelay(3f));
    }

    IEnumerator HideInfoTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        infoText.gameObject.SetActive(false);
        hideTextCoroutine = null;
    }

    void EnableTempObject(GameObject tempObject)
    {
        if (tempObject != null)
        {
            tempObject.SetActive(true);
        }
    }
}
