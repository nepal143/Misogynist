using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("References")]
    public GameObject hidingCameraObject;      // The object containing the camera + listener
    public GameObject pressFToHideCanvas;      // Canvas GameObject to enable when near

    [Header("Settings")]
    public float hideDistance = 2f;

    private Transform player;
    private GameObject playerObject;

    private bool isHovering = false;
    private bool isHiding = false;

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerObject = playerGO;
            player = playerGO.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure it has the tag 'Player'.");
        }

        if (hidingCameraObject != null)
            hidingCameraObject.SetActive(false);

        if (pressFToHideCanvas != null)
            pressFToHideCanvas.SetActive(false);  // Hide canvas at start
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool canHide = isHovering && distance <= hideDistance;

        // Show/hide the 'Press F to Hide' canvas
        if (pressFToHideCanvas != null)
            pressFToHideCanvas.SetActive(canHide && !isHiding);

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isHiding)
            {
                ExitHiding();
            }
            else if (canHide)
            {
                EnterHiding();
            }
        }
    }

    void OnMouseOver()
    {
        isHovering = true;
    }

    void OnMouseExit()
    {
        isHovering = false;
    }

    void EnterHiding()
    {
        if (playerObject != null)
            playerObject.SetActive(false);

        if (hidingCameraObject != null)
            hidingCameraObject.SetActive(true);

        if (pressFToHideCanvas != null)
            pressFToHideCanvas.SetActive(false); // Hide UI when hiding

        isHiding = true;
        Debug.Log("Player is now hiding.");
    }

    void ExitHiding()
    {
        if (playerObject != null)
            playerObject.SetActive(true);

        if (hidingCameraObject != null)
            hidingCameraObject.SetActive(false);

        isHiding = false;
        Debug.Log("Player exited hiding.");
    }
}
