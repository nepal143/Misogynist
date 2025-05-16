using System.Collections;
using UnityEngine;

namespace SojaExiles
{
    public class Drawer_Pull_X : MonoBehaviour
    {
        public Animator pull_01;
        public bool open;
        public Transform Player;

        [Header("Drawer Sounds")]
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

            // Set up 3D audio source
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // fully 3D
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
                    print("object name");

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
            Debug.Log("You are opening the drawer");

            if (openSound != null)
                audioSource.PlayOneShot(openSound);

            pull_01.Play("openpull_01");
            open = true;

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator closing()
        {
            Debug.Log("You are closing the drawer");

            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);

            pull_01.Play("closepush_01");
            open = false;

            yield return new WaitForSeconds(0.5f);
        }
    }
}
