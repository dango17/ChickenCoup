// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 29/03/2023

using UnityEngine;

namespace DO
{
    public class PushPull : MonoBehaviour
    {
        public float pushForce = 10f;
        public float pullForce = 5f;
        public float distance = 1.5f;

        private bool canPush = false;
        private bool canPull = false;
        private bool isPushing = false;
        private bool isPulling = false;
        private GameObject currentObject;

        void Update()
        {
            // check for push or pull input
            if (Input.GetKeyDown(KeyCode.E) && canPush == true)
            {
                isPushing = true;
                isPulling = false;
                if (currentObject != null) currentObject.GetComponent<Rigidbody>().isKinematic = true;
            }
            else if (Input.GetKeyDown(KeyCode.E) && canPull)
            {
                isPulling = true;
                isPushing = false;
                if (currentObject != null) currentObject.GetComponent<Rigidbody>().isKinematic = false;
            }

            // release object if no longer pushing or pulling
            if (!isPushing && !isPulling && currentObject != null)
            {
                if (currentObject != null) currentObject.GetComponent<Rigidbody>().isKinematic = false;
                currentObject = null;
            }

            // move object if pushing or pulling and currentObject is not null
            if ((isPushing || isPulling) && currentObject != null)
            {
                if (isPushing)
                {
                    Vector3 pushDirection = transform.forward * distance;
                    if (currentObject != null) currentObject.GetComponent<Rigidbody>().AddForce(pushDirection * pushForce);
                }
                else if (isPulling)
                {
                    Vector3 pullDirection = (transform.position - currentObject.transform.position).normalized * distance;
                    if (currentObject != null) currentObject.GetComponent<Rigidbody>().AddForce(pullDirection * pullForce);
                }
            }
        }

        // check for objects within range
        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("Pushable"))
            {
                canPush = true;
                currentObject = other.gameObject;
            }
            else if (other.gameObject.CompareTag("Pullable"))
            {
                canPull = true;
                currentObject = other.gameObject;
            }
        }

        // reset variables when leaving object range
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Pushable"))
            {
                canPush = false;
                isPushing = false;
                if (currentObject != null) currentObject.GetComponent<Rigidbody>().isKinematic = false;
                currentObject = null;
            }
            else if (other.gameObject.CompareTag("Pullable"))
            {
                canPull = false;
                isPulling = false;
                if (currentObject != null) currentObject.GetComponent<Rigidbody>().isKinematic = false;
                currentObject = null;
            }
        }
    }
}

