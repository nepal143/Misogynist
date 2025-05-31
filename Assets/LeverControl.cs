using System.Collections;
using UnityEngine;
using TMPro;

public class LeverControl : MonoBehaviour
{
    public FuseBox fuseBoxScript;          // Reference to your FuseBox script
    public Transform lever;                // The lever bone to rotate
    public Transform garageStart;          // Garage closed position
    public Transform garageEnd;            // Garage open position
    public Transform garageDoor;           // The actual garage object to move

    public float leverRotateTime = 1.0f;   // Time to rotate lever
    public float garageMoveTime = 2.0f;    // Time to open garage

    public TextMeshProUGUI infoText;       // Assign in Inspector for messages

    private bool leverPulled = false; 
    private bool isInteracting = false;
    private Coroutine hideTextCoroutine;

    void OnMouseDown()
    {
        if (!isInteracting)
        {
            StartCoroutine(HandleLever());
        }
    }

    IEnumerator HandleLever()
    {
        isInteracting = true;

        if (fuseBoxScript != null && fuseBoxScript.AllFusesInserted())
        {
            // ✅ LIGHTS ON – Pull lever and open garage
            yield return StartCoroutine(RotateLever(0, 163));

            yield return new WaitForSeconds(0.5f); // Optional delay
            yield return StartCoroutine(MoveGarageDoor(garageStart.position, garageEnd.position));
            leverPulled = true;

            // Clear any message
            if (infoText != null)
                infoText.gameObject.SetActive(false);
        }
        else
        {
            // ❌ LIGHTS OFF – Show message and fake pull
            if (infoText != null)
            {
                infoText.text = "⚠️ No electricity! Please install all fuses on the fuse board.";
                infoText.gameObject.SetActive(true);

                if (hideTextCoroutine != null)
                    StopCoroutine(hideTextCoroutine);

                hideTextCoroutine = StartCoroutine(HideInfoTextAfterDelay(3f));
            }

            yield return StartCoroutine(RotateLever(0, 163));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(RotateLever(163, 0));
        }

        isInteracting = false;
    }

    IEnumerator RotateLever(float fromX, float toX)
    {
        float elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(fromX, 0, 0);
        Quaternion endRot = Quaternion.Euler(toX, 0, 0);

        while (elapsed < leverRotateTime)
        {
            elapsed += Time.deltaTime;
            lever.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / leverRotateTime);
            yield return null;
        }

        lever.localRotation = endRot;
    }

    IEnumerator MoveGarageDoor(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < garageMoveTime)
        {
            elapsed += Time.deltaTime;
            garageDoor.position = Vector3.Lerp(from, to, elapsed / garageMoveTime);
            yield return null;
        }

        garageDoor.position = to;
    }

    public bool IsLeverPulled()
    {
        return leverPulled;
    }

    IEnumerator HideInfoTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (infoText != null)
            infoText.gameObject.SetActive(false);
        hideTextCoroutine = null;
    }
}
