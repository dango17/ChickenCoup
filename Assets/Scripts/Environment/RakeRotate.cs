// Author: Daniel Oldham/s1903729
// Collaborator: Liam Bansal
// Created On: ?

using UnityEngine;

namespace DO
{
    public class RakeRotate : MonoBehaviour
    {
        public bool IsTriggered {
            get;
            private set;
        }

        public float rotationSpeed = 5.0f;
        private Quaternion initialRotation;
        private Quaternion targetRotation;

        /// <summary>
        /// The agent that triggered the trap.
        /// </summary>
        private GameObject triggeredAgent = null;

        void Start()
        {
            initialRotation = transform.rotation;
            targetRotation = initialRotation * Quaternion.Euler(-90, 0, 0);
        }

        void Update()
        {
            Quaternion target = IsTriggered ? targetRotation : initialRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * rotationSpeed);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsTriggered && other.gameObject.CompareTag("Player"))
            {
                IsTriggered = true;
                triggeredAgent = other.gameObject;
            }

			if (!IsTriggered && other.gameObject.CompareTag("Farmer")) {
				IsTriggered = true;
				triggeredAgent = other.gameObject;
				float stunTime = 2.5f;
                other.GetComponent<Farmer>().StunFarmer(stunTime);
			}
		}

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == triggeredAgent) {
                IsTriggered = false;
            }
        }
    }
}