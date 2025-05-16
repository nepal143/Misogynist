using System.Collections;
using UnityEngine;

namespace SojaExiles
{
    public class opencloseDoor1 : MonoBehaviour
    {
        public Animator openandclose1;
        public bool open;
        public Transform Player;

        [Header("Door Sounds")]
        public AudioClip openSound;
        public AudioClip closeSound;

        private AudioSource audioSource;

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

            // Add and configure 3D AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // Fully 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.playOnAwake = false;
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

        IEnumerator opening()
        {
            Debug.Log("You are opening the door");
            
            if (openSound != null)
                audioSource.PlayOneShot(openSound);

            openandclose1.Play("Opening 1");
            open = true;

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator closing()
        {
            Debug.Log("You are closing the door");

            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);

            openandclose1.Play("Closing 1");
            open = false;

            yield return new WaitForSeconds(0.5f);
        }
    }
}
