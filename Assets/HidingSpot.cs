using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("References")]
    public GameObject hidingCameraObject;  // Parent object with Camera + AudioListener

    [Header("Settings")]
    public float hideDistance = 2f;

    private Transform player;
    private GameObject playerObject;
    private Camera hidingCamera;

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
        {
            hidingCamera = hidingCameraObject.GetComponent<Camera>();

            if (hidingCamera != null)
            {
                hidingCamera.enabled = false;
            }
            else
            {
                Debug.LogWarning("No Camera component found on hidingCameraObject.");
            }

            // Also disable AudioListener if needed
            AudioListener listener = hidingCameraObject.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isHiding)
            {
                ExitHiding();
            }
            else if (isHovering && distance <= hideDistance)
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

        if (hidingCamera != null)
            hidingCamera.enabled = true;

        AudioListener listener = hidingCameraObject.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = true;

        isHiding = true;
        Debug.Log("Player is now hiding.");
    }

    void ExitHiding()
    {
        if (playerObject != null)
            playerObject.SetActive(true);

        if (hidingCamera != null)
            hidingCamera.enabled = false;

        AudioListener listener = hidingCameraObject.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = false;

        isHiding = false;
        Debug.Log("Player exited hiding.");
    }
}
