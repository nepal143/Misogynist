using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FuseBox : MonoBehaviour
{
    public List<GameObject> fuseSlots;        // Fuse slots to enable
    private int currentSlotIndex = 0;

    public Transform playerHand;              // Hand where the fuse is held
    public TextMeshProUGUI messageText;       // Just this! No CanvasGroup

    public float messageDuration = 2f;        // How long the message stays

    private Coroutine messageRoutine;

    void OnMouseDown()
    {
        if (playerHand.childCount > 0)
        {
            Transform heldItem = playerHand.GetChild(0);
            if (heldItem.CompareTag("Fuse"))
            {
                InsertFuse(heldItem.gameObject);
                return;
            }
        }

        ShowMessage("You need a fuse to interact with the fuse box.");
    }

    void InsertFuse(GameObject fuse)
    {
        if (currentSlotIndex < fuseSlots.Count)
        {
            fuseSlots[currentSlotIndex].SetActive(true);
            currentSlotIndex++;

            Destroy(fuse);
            Debug.Log("Fuse inserted.");

            if (currentSlotIndex == fuseSlots.Count)
            {
                ShowMessage("Lights are back!");
            }
        }
        else
        {
            ShowMessage("All fuse slots are already filled.");
        }
    }

    void ShowMessage(string msg)
    {
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(DisplayMessage(msg));
    }

    IEnumerator DisplayMessage(string msg)
    {
        messageText.text = msg;
        yield return new WaitForSeconds(messageDuration);
        messageText.text = "";
        messageRoutine = null;
    }
    public bool AllFusesInserted()
{
    return currentSlotIndex >= fuseSlots.Count;
}
}
