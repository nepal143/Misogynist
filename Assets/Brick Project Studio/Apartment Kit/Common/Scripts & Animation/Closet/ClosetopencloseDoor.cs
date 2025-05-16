using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SojaExiles
{
    public class ClosetopencloseDoor : MonoBehaviour
    {
        public Animator Closetopenandclose;
        public bool open;
        public Transform Player;
        public NavMeshObstacle navMeshObstacle; // Add this in Inspector or get via code


        void Awake()
{
    navMeshObstacle = GetComponent<NavMeshObstacle>();
}
        void Start()
        {
            open = false;

            // Automatically find the player by tag
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found!");
            }

            // Try to find the NavMeshObstacle if not set
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
                if (dist < 15f)
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

    Closetopenandclose.Play("Opening"); // or Closetopenandclose
    open = true;

    yield return new WaitForSeconds(0.5f);
}

        public IEnumerator closing()
        {
            Debug.Log("You are closing the closet");

            Closetopenandclose.Play("ClosetClosing");
            open = false;

            yield return new WaitForSeconds(0.5f);

            if (navMeshObstacle != null)
            {
                navMeshObstacle.enabled = true;
                Debug.Log("NavMeshObstacle re-enabled");
            }
        }
    }
}
