using UnityEngine;

namespace DO
{
    public class RakeRotate : MonoBehaviour
    {
        public float rotationSpeed = 5.0f;
        private Quaternion initialRotation;
        private Quaternion targetRotation;
        private bool isTriggered = false;

        void Start()
        {
            initialRotation = transform.rotation;
            targetRotation = initialRotation * Quaternion.Euler(-90, 0, 0);
        }

        void Update()
        {
            Quaternion target = isTriggered ? targetRotation : initialRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * rotationSpeed);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                isTriggered = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                isTriggered = false;
            }
        }
    }
}