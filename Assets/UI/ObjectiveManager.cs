// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 22/05/2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DO
{
    public class ObjectiveManager : MonoBehaviour
    {
        [Header("Has Escaped Coop?")]
        public bool hasEscapedCoop = false; 
        public GameObject EscapeCoopTick;

        [Header("Has found Fuse?")]
        public bool hasFoundFuse = false;
        public GameObject HasFoundFuseTick;

        [Header("Has Inserted Fuse?")]
        public bool HasInsertedFuse = false;
        public GameObject hasInsertedFuseTick;

        [Header("Has Stole Keycard?")]
        public bool HasStoleKeycard = false;
        public GameObject HasStoleKeycardTick;

        public void Update()
        {
            if(hasEscapedCoop)
            {
                EscapeCoopTick.SetActive(true); 
            }

            if(hasFoundFuse)
            {
                HasFoundFuseTick.SetActive(true); 
            }

            if(HasInsertedFuse)
            {
                hasInsertedFuseTick.SetActive(true); 
            }

            if(HasStoleKeycard)
            {
                HasStoleKeycardTick.SetActive(true); 
            }
        }

    }
}
