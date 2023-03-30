// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 31/01/2023

using UnityEngine;

namespace DO
{
    /// <summary>
    /// Holds all Camera gameObjects for several perspectives.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public GameObject mainCameraObject;
        public GameObject wallCameraObject;
        public GameObject fpCameraObject;
        public Transform camTransform;

        public float tiltAngle;
        public float tileRotation = 5f; 

        public void HandleFPSTilt(float vertical, float delta)
        {
            tiltAngle -= vertical * tileRotation;

            tiltAngle = Mathf.Clamp(tiltAngle, -35, 35);
            fpCameraObject.transform.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
            camTransform.rotation *= fpCameraObject.transform.rotation; 
        }
    }
}