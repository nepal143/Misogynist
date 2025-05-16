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
        private NoiseSource noiseSource;

        void Awake()
        {
            navMeshObstacle = GetComponent<NavMeshObstacle>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.playOnAwake = false;

            // Add or get NoiseSource
            noiseSource = gameObject.GetComponent<NoiseSource>();
            if (noiseSource == null)
            {
                noiseSource = gameObject.AddComponent<NoiseSource>();
                noiseSource.noiseMakerTag = "Player";
                noiseSource.noiseRadius = 10f;
                noiseSource.noiseStrength = 0.7f;
            }
        }

        void Start()
        {
            open = false;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                Player = playerObject.transform;
        }

        void OnMouseOver()
        {
            if (Player && Vector3.Distance(Player.position, transform.position) < 2.3f)
            {
                if (!open && Input.GetMouseButtonDown(0))
                    StartCoroutine(opening());
                else if (open && Input.GetMouseButtonDown(0))
                    StartCoroutine(closing());
            }
        }

        public IEnumerator opening()
        {
            if (navMeshObstacle != null && navMeshObstacle.enabled)
                navMeshObstacle.enabled = false;

            if (openSound != null)
                audioSource.PlayOneShot(openSound);

            openandclose.Play("Opening");
            open = true;

            noiseSource.EmitNoise();

            yield return new WaitForSeconds(0.5f);
        }

        public IEnumerator closing()
        {
            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);

            openandclose.Play("Closing");
            open = false;

            noiseSource.EmitNoise();

            if (navMeshObstacle != null)
            {
                yield return new WaitForSeconds(0.5f);
                navMeshObstacle.enabled = true;
            }
        }
    }
}
