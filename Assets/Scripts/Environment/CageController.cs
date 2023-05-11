// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 31/03/2023

using UnityEngine;

namespace DO
{
    /// <summary>
    /// Detects if player is inside the cage, if so lock them in-place inside the cage for a 
    /// specified amount of time, eventually 'unlocked'.
    /// </summary>

    public class CageController : MonoBehaviour
    {
        public Transform playerTransformPoint;
        public float lockTime = 10f;
        public float lockCooldown = 2f;

        public bool isLocked = false;
        private float lockStartTime;
        public Animator animator; 

        private Transform playerOriginalTransform;

        PlayerController controller; 

        private void Update()
        {
            if (isLocked && Time.time - lockStartTime >= lockTime)
            {
                UnlockPlayer();
            }
        }

        public void LockPlayer(Transform playerTransform)
        {
            
            playerOriginalTransform = playerTransform;
            playerTransform.position = playerTransformPoint.position;

            isLocked = true;
            animator.Play("cagedoorClose");

            //Record the time player was locked
            lockStartTime = Time.time;

            //Disable player movement and physics while locked
            Rigidbody playerRigidbody = playerTransform.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = true;
            }
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        private void UnlockPlayer()
        {
            //Move the player back to their original transform
            playerOriginalTransform.position = playerTransformPoint.position;
            isLocked = false;

            animator.Play("cagedoorOpen");

            //Re-enable player movement and physics
            Rigidbody playerRigidbody = playerOriginalTransform.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;
                playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }
            PlayerController playerController = playerOriginalTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
}
