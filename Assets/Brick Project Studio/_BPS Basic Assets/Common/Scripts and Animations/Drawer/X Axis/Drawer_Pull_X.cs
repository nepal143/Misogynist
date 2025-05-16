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
        private NoiseSource noiseSource;

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

            noiseSource = gameObject.GetComponent<NoiseSource>();
            if (noiseSource == null)
            {
                noiseSource = gameObject.AddComponent<NoiseSource>();
                noiseSource.noiseMakerTag = "Player";
                noiseSource.noiseRadius = 8f;
                noiseSource.noiseStrength = 0.4f;
            }
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

        IEnumerator opening()
        {
            if (openSound != null)
                audioSource.PlayOneShot(openSound);

            pull_01.Play("openpull_01");
            open = true;

            noiseSource.EmitNoise();

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator closing()
        {
            if (closeSound != null)
                audioSource.PlayOneShot(closeSound);

            pull_01.Play("closepush_01");
            open = false;

            noiseSource.EmitNoise();

            yield return new WaitForSeconds(0.5f);
        }
    }
}
