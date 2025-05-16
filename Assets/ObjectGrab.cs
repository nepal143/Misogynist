using UnityEngine;
using System.Collections.Generic;

public class ObjectGrabber : MonoBehaviour
{
    public Transform handHold;                     // For normal items
    public Transform distantHoldPoint;             // For big items
    public float grabDistance = 1.3f;
    public List<string> bigObjectTags;             // Tags to consider as big objects

    private GameObject heldObject = null;
    private Rigidbody heldRigidbody = null;
    private Collider[] heldColliders = null;

void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            GrabbableRoot grabbable = hit.collider.GetComponentInParent<GrabbableRoot>();
            if (grabbable != null)
            {
                // Auto-drop if already holding something
                if (heldObject != null)
                {
                    DropObject();
                }

                GrabObject(grabbable.gameObject);
            }
        }
    }

    if (Input.GetKeyDown(KeyCode.Q) && heldObject != null)
    {
        DropObject();
    }
}

    void GrabObject(GameObject obj)
    {
        heldObject = obj;
        heldRigidbody = obj.GetComponent<Rigidbody>();
        heldColliders = obj.GetComponentsInChildren<Collider>();

        // Disable all colliders
        foreach (var col in heldColliders)
        {
            col.enabled = false;
        }

        // Disable physics
        if (heldRigidbody != null)
        {
            heldRigidbody.useGravity = false;
            heldRigidbody.isKinematic = true;
        }

        // Determine which hold point to use based on tag
        Transform grabPoint = bigObjectTags.Contains(obj.tag) ? distantHoldPoint : handHold;

        // Parent and position the object
        obj.transform.SetParent(grabPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void DropObject()
    {
        // Re-enable colliders
        foreach (var col in heldColliders)
        {
            col.enabled = true;
        }

        // Enable physics
        if (heldRigidbody != null)
        {
            heldRigidbody.useGravity = true;
            heldRigidbody.isKinematic = false;
        }

        // Detach object
        heldObject.transform.SetParent(null);

        heldObject = null;
        heldRigidbody = null;
        heldColliders = null;
    }
}
