using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class CageController : MonoBehaviour
    {
  
        public Transform playerTransformPoint;
        public float lockTime = 10f;
        public float lockCooldown = 2f;

        private bool isLocked = false;
        private float lockStartTime;

        private Transform playerOriginalTransform;

        //The time the player was last unlocked from the cage
        private float lastUnlockTime = Mathf.NegativeInfinity;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isLocked && Time.time - lastUnlockTime >= lockCooldown)
            {
                LockPlayer(other.transform);
            }
        }

        private void Update()
        {
            if (isLocked && Time.time - lockStartTime >= lockTime)
            {
                UnlockPlayer();
            }
        }

        private void LockPlayer(Transform playerTransform)
        {
            
            playerOriginalTransform = playerTransform;
            playerTransform.position = playerTransformPoint.position;

            isLocked = true;
            //Record the time the player was locked
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
            // Move the player back to their original transform
            playerOriginalTransform.position = playerTransformPoint.position;

            // Unlock the player
            isLocked = false;

            // Re-enable player movement and physics
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

            // Record the time the player was last unlocked
            lastUnlockTime = Time.time;
        }
    }
}
