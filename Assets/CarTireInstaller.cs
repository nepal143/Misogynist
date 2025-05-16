using UnityEngine;

public class CarTireInstaller : MonoBehaviour
{
    public Transform playerHand;              // Player's hand holding the tire
    public GameObject preInstalledTire;       // The disabled tire object already placed in car
    public Transform objectToRotate;          // Optional object to rotate (e.g. wheel mount)
    private int installedTireCount ; 
    void OnMouseDown()
    {
        if (playerHand.childCount > 0)
        {
            Transform heldItem = playerHand.GetChild(0);

            if (heldItem.CompareTag("Tire"))
            {
                InstallTire(heldItem.gameObject);
                installedTireCount++ ; 
            }
        }
        else
        {
            Debug.Log("You need to hold a tire to install it.");
        }
    }

    void InstallTire(GameObject heldTire)
    {
        // Enable the pre-installed tire on the car
        if (preInstalledTire != null)
        {
            preInstalledTire.SetActive(true);
        }

        // Remove the tire from the player's hand
        heldTire.transform.SetParent(null);
        Destroy(heldTire);

        // Rotate the optional object
        if (objectToRotate != null)
        {
            objectToRotate.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        Debug.Log("Tire installed successfully. Hand tire removed, car tire enabled.");
    }
    public bool AreAllTiresInstalled()
{
    return installedTireCount >= 1;
}
}
