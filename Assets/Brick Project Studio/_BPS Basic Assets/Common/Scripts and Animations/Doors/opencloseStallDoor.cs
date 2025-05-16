using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SojaExiles
{
    public class opencloseStallDoor : MonoBehaviour
    {
        public Animator openandclose;
        public bool open;
        public Transform Player;

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
        }

        void OnMouseOver()
        {
            if (Player)
            {
                float dist = Vector3.Distance(Player.position, transform.position);
                if (dist <  2.3)
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
            print("you are opening the door");
            openandclose.Play("OpeningStall");
            open = true;
            yield return new WaitForSeconds(.5f);
        }

        IEnumerator closing()
        {
            print("you are closing the door");
            openandclose.Play("ClosingStall");
            open = false;
            yield return new WaitForSeconds(.5f);
        }
    }
}
