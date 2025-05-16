using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SojaExiles
{
    public class opencloseDoor : MonoBehaviour
    {
        public Animator openandclose;
        public bool open;
        public Transform Player;
        public NavMeshObstacle navMeshObstacle;

        [Header("Door Sounds")]
        public AudioClip openSound;
        public AudioClip closeSound;

        private AudioSource audioSource;

        void Awake()
        {
            navMeshObstacle = GetComponent<NavMeshObstacle>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // Make sound 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.playOnAwake = false;
        }

        void Start()
        {
            open = false;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found!");
            }

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
                if (dist < 2.3f)
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
            Debug.Log("You are opening the door");

            if (navMeshObstacle != null && navMeshObstacle.enabled)
                navMeshObstacle.enabled = false;

            if (openSound != null)
                audioSource.PlayOneShot(openSound);

            openandclose.Play("Opening");
            open = true;

            yield return new WaitForSeconds(0.5f);
        }

        public IEnumerator closing()
        {
            Debug.Log("You are closing the door");

            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);

            openandclose.Play("Closing");
            open = false;

            if (navMeshObstacle != null)
            {
                yield return new WaitForSeconds(0.5f);
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
