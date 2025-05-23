using UnityEngine;
using TMPro;

public class GrabbableRoot : MonoBehaviour
{
    [Header("Name to Display")]
    public string objectDisplayName = "Grabbable Object";

    private static TextMeshProUGUI objectNameText;
    private static Camera playerCamera;
    private const float maxDistance = 2f;

    void Start()
    {
        // Only initialize once
        if (objectNameText == null)
        {
            GameObject textObject = GameObject.FindGameObjectWithTag("objectName");
            if (textObject != null)
            {
                objectNameText = textObject.GetComponent<TextMeshProUGUI>();
                objectNameText.text = "";
            }
            else
            {
                Debug.LogError("No TextMeshProUGUI with tag 'objectName' found in scene.");
            }
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Make object immovable at start
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (playerCamera == null || objectNameText == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            GrabbableRoot grabbable = hit.collider.GetComponent<GrabbableRoot>();
            if (grabbable != null)
            {
                objectNameText.text = grabbable.objectDisplayName;
                return;
            }
        }

        objectNameText.text = "";
    }
}
