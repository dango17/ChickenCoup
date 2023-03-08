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
    }
}