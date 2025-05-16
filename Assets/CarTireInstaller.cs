using UnityEngine;

public class CarTireInstaller : MonoBehaviour
{
    public Transform playerHand;              // Where the tire is held
    public Transform tireInstallPosition;     // Position + rotation for tire
    public Transform objectToRotate;          // The other object to rotate (e.g. wheel mount)

    void OnMouseDown()
    {
        if (playerHand.childCount > 0)
        {
            Transform heldItem = playerHand.GetChild(0);

            if (heldItem.CompareTag("Tire"))
            {
                InstallTire(heldItem);
            }
        }
        else
        {
            Debug.Log("You need to hold a tire to install it.");
        }
    }

    void InstallTire(Transform tire)
    {
        // Set position and rotation of the tire
        tire.position = tireInstallPosition.position;
        tire.rotation = tireInstallPosition.rotation;

        // Parent it so it stays fixed
        tire.SetParent(tireInstallPosition);

        // Set the assigned object's rotation to (0, 90, 0)
        if (objectToRotate != null)
        {
            objectToRotate.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        Debug.Log("Tire installed. Assigned object rotated.");
    }
}
