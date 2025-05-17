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
                    // Increase activity only when player interacts
                    if (PlayerActivityTracker.Instance != null)
                    {
                        PlayerActivityTracker.Instance.IncreaseActivity(10f); // Adjust value if needed
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

            pull_01.Play("openpull_01");
            open = true;

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator closing()
        {
            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);

            pull_01.Play("closepush_01");
            open = false;

            yield return new WaitForSeconds(0.5f);
        }
    }
}
