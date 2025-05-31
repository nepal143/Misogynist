using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("References")]
    public GameObject hidingCameraObject;
    public GameObject pressFToHideCanvas; // This is now unique per hiding spot

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

        if (hidingCameraObject != null)
            hidingCameraObject.SetActive(false);

        if (pressFToHideCanvas != null)
            pressFToHideCanvas.SetActive(false); // Make sure it's hidden at start
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        bool canHide = isHovering && distance <= hideDistance && !isHiding;

        // Show/hide panel
        if (pressFToHideCanvas != null)
            pressFToHideCanvas.SetActive(canHide);

        // Handle input
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isHiding)
                ExitHiding();
            else if (canHide)
                EnterHiding();
        }
    }

    void OnMouseOver() => isHovering = true;
    void OnMouseExit() => isHovering = false;

    void EnterHiding()
    {
        if (playerObject != null)
            playerObject.SetActive(false);

        if (hidingCameraObject != null)
            hidingCameraObject.SetActive(true);

        if (pressFToHideCanvas != null)
            pressFToHideCanvas.SetActive(false);

        isHiding = true;
    }

    void ExitHiding()
    {
        if (playerObject != null)
            playerObject.SetActive(true);

        if (hidingCameraObject != null)
            hidingCameraObject.SetActive(false);

        isHiding = false;
    }
}
