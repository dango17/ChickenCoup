// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 22/05/2023

using UnityEngine;

namespace DO
{
    public class hasEscapedCheck : MonoBehaviour
    {
        ObjectiveManager objectiveManager;

        public void Start()
        {
            objectiveManager = FindObjectOfType<ObjectiveManager>(); 
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == ("Player"))
            {
                objectiveManager.EscapeCoopTick.SetActive(true); 
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.tag == ("Player"))
            {
                objectiveManager.hasEscapedCoop = true;
            }
        }
    }

}