using UnityEngine;
using TMPro;
using System.Collections;

public class FirstEndingTrigger : MonoBehaviour
{
    public GameObject playerHand;
    public string requiredItemName = "CarKey";
    public CarTireInstaller tireInstaller;
    public LeverControl leverControl;
    public GameObject endingTimelineObject;
    public TextMeshProUGUI infoText; // Assign in inspector

    private Coroutine hideTextCoroutine;

    private void OnMouseDown()
    {
        string missingItems = "";

        if (!IsHoldingRequiredItem())
            missingItems += "• Car Key is missing.\n";

        if (!tireInstaller.AreAllTiresInstalled())
            missingItems += "• All car tires are not installed.\n";

        if (!leverControl.IsLeverPulled())
            missingItems += "• Lever has not been pulled.\n";

        if (!string.IsNullOrEmpty(missingItems))
        {
            infoText.text = "<b>You can't escape yet:</b>\n" + missingItems;
            infoText.gameObject.SetActive(true);

            // Restart coroutine to hide after 3 seconds
            if (hideTextCoroutine != null)
                StopCoroutine(hideTextCoroutine);

            hideTextCoroutine = StartCoroutine(HideInfoTextAfterDelay(3f));
            return;
        }

        // All conditions met
        infoText.text = "";
        infoText.gameObject.SetActive(false);
        Debug.Log("✅ All conditions met! Triggering the first ending...");
        endingTimelineObject.SetActive(true);
    }

    bool IsHoldingRequiredItem()
    {
        if (playerHand.transform.childCount == 0)
            return false;

        GameObject heldItem = playerHand.transform.GetChild(0).gameObject;
        return heldItem.name.Contains(requiredItemName);
    }

    IEnumerator HideInfoTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        infoText.gameObject.SetActive(false);
        hideTextCoroutine = null;
    }
}
