// Author: Daniel Oldham/s1903729
// Collaborator: Liam Bansal.
// Created On: 20/03/2023

using UnityEngine;

namespace DO
{
    public class FPCamTrigger : MonoBehaviour
    {
        private PlayerController playerController = null;

        private void Start() {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
		}

        private void OnTriggerEnter(Collider other)
        {
            if(other.transform.parent.tag == "Player")
            {
				playerController.EnableFirstPerson(true);
			}
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.transform.parent.tag == "Player")
            {
				playerController.EnableFirstPerson(true);
			}
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.parent.tag == "Player")
            {
				playerController.EnableFirstPerson(false);
			}
        }
    }
}