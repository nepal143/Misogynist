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
                Player = playerObject.transform;

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.playOnAwake = false;
        }

        void OnMouseOver()
        {
            if (Player && Vector3.Distance(Player.position, transform.position) < 2.3f)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Increase activity only when the player initiates door interaction
                    if (PlayerActivityTracker.Instance != null)
                    {
                        PlayerActivityTracker.Instance.IncreaseActivity(15f); // You can tweak the amount
                    }

                    if (!open)
                        StartCoroutine(opening());
                    else
                        StartCoroutine(closing());
                }
            }
        }

        IEnumerator opening()
        {
            if (openSound != null)
                audioSource.PlayOneShot(openSound);

            openandclose1.Play("Opening 1");
            open = true;

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator closing()
        {
            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);

            openandclose1.Play("Closing 1");
            open = false;

            yield return new WaitForSeconds(0.5f);
        }
    }
}
