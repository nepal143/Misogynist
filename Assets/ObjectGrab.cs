using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    public Transform handHold;
    public float grabDistance = 1.3f;

    private GameObject heldObject = null;
    private Rigidbody heldRigidbody = null;
    private Collider[] heldColliders = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && heldObject == null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
            {
                GrabbableRoot grabbable = hit.collider.GetComponentInParent<GrabbableRoot>();
                if (grabbable != null)
                {
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

        // Disable colliders
        foreach (var col in heldColliders)
        {
            col.enabled = false;
        }

        // Disable rigidbody physics
        if (heldRigidbody != null)
        {
            heldRigidbody.useGravity = false;
            heldRigidbody.isKinematic = true;
        }

        // Attach to hand
        obj.transform.SetParent(handHold);
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

        // Enable physics again
        if (heldRigidbody != null)
        {
            heldRigidbody.useGravity = true;
            heldRigidbody.isKinematic = false;
        }

        // Detach from hand
        heldObject.transform.SetParent(null);

        heldObject = null;
        heldRigidbody = null;
        heldColliders = null;
    }
}
