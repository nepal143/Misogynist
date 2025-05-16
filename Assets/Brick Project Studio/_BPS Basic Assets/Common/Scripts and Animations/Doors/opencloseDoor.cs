using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Required for NavMeshObstacle

namespace SojaExiles
{
    public class opencloseDoor : MonoBehaviour
    {
        public Animator openandclose;
        public bool open;
        public Transform Player;
        public NavMeshObstacle navMeshObstacle; // Reference to obstacle

        void Awake()
{
    navMeshObstacle = GetComponent<NavMeshObstacle>();
}
        void Start()
        {
            open = false;

            // Auto-assign player transform
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found!");
            }

            // Auto-assign NavMeshObstacle if not manually set
            if (navMeshObstacle == null)
            {
                navMeshObstacle = GetComponent<NavMeshObstacle>();
            }
        }

        void OnMouseOver()
        {
            if (Player)
            {
                float dist = Vector3.Distance(Player.position, transform.position);
                if (dist <  2.3f)
                {
                    if (!open && Input.GetMouseButtonDown(0))
                    {
                        StartCoroutine(opening());
                    }
                    else if (open && Input.GetMouseButtonDown(0))
                    {
                        StartCoroutine(closing());
                    }
                }
            }
        }

public IEnumerator opening()
{
    print("you are opening the door");

    if (navMeshObstacle != null && navMeshObstacle.enabled)
        navMeshObstacle.enabled = false;

    openandclose.Play("Opening"); // or Closetopenandclose
    open = true;

    yield return new WaitForSeconds(0.5f);
}

        public IEnumerator closing()
        {
            Debug.Log("You are closing the door");

            openandclose.Play("Closing");
            open = false;

            // Enable obstacle after closing
            if (navMeshObstacle != null)
            {
                yield return new WaitForSeconds(0.5f); // Wait until animation is done
                navMeshObstacle.enabled = true;
                Debug.Log("NavMeshObstacle re-enabled");
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
