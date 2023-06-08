// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 04/03/2023

using UnityEngine;

namespace DO
{
    public class FusePuzzle : MonoBehaviour
    {
        public GameObject fuseObjects;

        public Light generatorIndicator; 
        public Light lightIndicator;
        public Light keypadIndicator;

        public KeypadPuzzle keypadPuzzle; 

        ObjectiveManager objectiveManager; 

        void Start()
        {
            lightIndicator.color = Color.red;
            GetComponent<Collider>().isTrigger = true;

            objectiveManager = FindObjectOfType<ObjectiveManager>(); 
        }

        void Update()
        {
            
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fuse"))
            {
                Destroy(GameObject.Find("FuseObject"));
                lightIndicator.color = Color.green; 
                generatorIndicator.color = Color.green;
                keypadIndicator.color = Color.green; 

                objectiveManager.HasInsertedFuse = true;
                keypadPuzzle.enabled = true; 
            }
        }
    }
}