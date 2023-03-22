// Author: Daniel Oldham/s1903729
// Collaborator: /A
// Created On: 20/03/2023

using UnityEngine;

namespace DO
{
    public class FPCamTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            InputHandler inputHandler = other.GetComponentInParent<InputHandler>(); 

            if(other.tag == "Player")
            {
                inputHandler.controller.isFPMode = true; 
            }
        }

        private void OnTriggerStay(Collider other)
        {
            InputHandler inputHandler = other.GetComponentInParent<InputHandler>();

            if (other.tag == "Player")
            {
                inputHandler.controller.isFPMode = true;
            }

        }

        private void OnTriggerExit(Collider other)
        {
            InputHandler inputHandler = other.GetComponentInParent<InputHandler>();

            if (other.tag == "Player")
            {
                inputHandler.controller.isFPMode = false;
            }

        }
    }
}