using System.Collections;
using UnityEngine;

public class LeverControl : MonoBehaviour
{
    public FuseBox fuseBoxScript;          // Reference to your FuseBox script
    public Transform lever;                // The lever bone to rotate
    public Transform garageStart;          // Garage closed position
    public Transform garageEnd;            // Garage open position
    public Transform garageDoor;           // The actual garage object to move

    public float leverRotateTime = 1.0f;   // Time to rotate lever
    public float garageMoveTime = 2.0f;    // Time to open garage

    private bool isInteracting = false;

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
        }
        else
        {
            // ❌ LIGHTS OFF – Fake pull
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
}
