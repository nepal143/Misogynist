using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

namespace SojaExiles
{
    public class opencloseDoor : MonoBehaviour
    {
        public Animator openandclose;
        public bool open;
        public Transform Player;
        public NavMeshObstacle navMeshObstacle;

        [Header("Lock Settings")]
        public bool isLocked = false;
        public string requiredKeyName = "DoorKey";
        public GameObject playerHand;

        [Header("Door Sounds")]
        public AudioClip openSound;
        public AudioClip closeSound;
        public AudioClip lockedSound; // 🔐 NEW: assign this in Inspector

        [Header("UI Feedback")]
        public TextMeshProUGUI keyHintText; // 🔠 Assign a TMP text object in Inspector
        public float hintDisplayDuration = 2f;

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
                if (Input.GetMouseButtonDown(0))
                {
                    if (!open)
                        TryToOpen();
                    else
                        StartCoroutine(closing());
                }
            }
        }

        void TryToOpen()
        {
            if (isLocked)
            {
                if (HasCorrectKeyInHand())
                {
                    Debug.Log("Correct key found. Unlocking door.");
                    isLocked = false;

                    // ✅ DO NOT delete the key anymore
                    // DestroyKeyFromHand();

                    StartCoroutine(opening());
                }
                else
                {
                    Debug.Log("Door is locked. Requires key: " + requiredKeyName);
                    PlayLockedSound();
                    ShowKeyHint();
                }
            }
            else
            {
                StartCoroutine(opening());
            }
        }

        bool HasCorrectKeyInHand()
        {
            if (playerHand == null || playerHand.transform.childCount == 0)
                return false;

            GameObject heldItem = playerHand.transform.GetChild(0).gameObject;
            return heldItem.name == requiredKeyName;
        }

        void PlayLockedSound()
        {
            if (lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
        }

        void ShowKeyHint()
        {
            if (keyHintText != null)
            {
                keyHintText.text = "Need the key: " + requiredKeyName;
                CancelInvoke(nameof(ClearKeyHint)); // Cancel previous invokes
                Invoke(nameof(ClearKeyHint), hintDisplayDuration);
            }
        }

        void ClearKeyHint()
        {
            if (keyHintText != null)
            {
                keyHintText.text = "";
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
