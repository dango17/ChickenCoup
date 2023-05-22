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

        public void Update()
        {
            if(hasEscapedCoop)
            {
                EscapeCoopTick.SetActive(true); 
            }
        }

    }
}
