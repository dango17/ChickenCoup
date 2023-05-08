// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 04/03/2023

using UnityEngine;

namespace DO
{
    public class FuseDoor : MonoBehaviour
    {
        public GameObject fuseObjects;
        public GameObject generator; 

        public Light generatorIndicator; 
        public Light lightIndicator; 

        void Start()
        {
            lightIndicator.color = Color.red;
            GetComponent<Collider>().isTrigger = true;
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
            }
        }
    }
}