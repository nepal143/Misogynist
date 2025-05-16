using UnityEngine;

public class FirstEndingTrigger : MonoBehaviour
{
    public GameObject playerHand; // The hand or inventory where the key would be held
    public string requiredItemName = "CarKey"; // The name of the key object
    public CarTireInstaller tireInstaller; // Reference to tire installer script
    public LeverControl leverControl; // Reference to lever control script
    public GameObject endingTimelineObject; // Object that triggers the timeline

    private void OnMouseDown()
    {
        // 1. Check if player has the car key in hand
        if (!IsHoldingRequiredItem())
        {
            Debug.Log("Player doesn't have the car key.");
            return;
        }

        // 2. Check if all tires are installed
        if (!tireInstaller.AreAllTiresInstalled())
        {
            Debug.Log("All tires are not installed.");
            return;
        }

        // 3. Check if the lever has been pulled
        if (!leverControl.IsLeverPulled())
        {
            Debug.Log("Lever has not been pulled.");
            return;
        }

        // ✅ All conditions met - trigger the ending
        Debug.Log("All conditions met! Triggering the first ending...");
        endingTimelineObject.SetActive(true);
    }

    bool IsHoldingRequiredItem()
    {
        if (playerHand.transform.childCount == 0)
            return false;

        GameObject heldItem = playerHand.transform.GetChild(0).gameObject;
        return heldItem.name.Contains(requiredItemName);
    }
}
