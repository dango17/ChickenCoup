// Author: Daniel Oldham/s1903729
// Collaborator: Liam Bansal.
// Created On: 20/03/2023

using UnityEngine;
using System.Collections;

namespace DO
{
    public class FPCamTrigger : MonoBehaviour
    {
        private PlayerController playerController = null;
        public float delay = 0.5f;

        public GameObject ventLight;

        private void Start()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
        }

        private IEnumerator EnableFirstPersonWithDelay(bool enable)
        {
            yield return new WaitForSeconds(delay);
            playerController.EnableFirstPerson(enable);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(EnableFirstPersonWithDelay(true));
                ventLight.SetActive(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(EnableFirstPersonWithDelay(true));
                ventLight.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(EnableFirstPersonWithDelay(false));
                ventLight.SetActive(false);
            }
        }
    }
}