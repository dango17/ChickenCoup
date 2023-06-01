using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class WinTriggerState : MonoBehaviour
    {
        public GameObject StatsMenuElement; 
        StatsMenu statsMenu; 

        public void Start()
        {
            statsMenu = FindObjectOfType<StatsMenu>(); 
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                statsMenu.StopTimer();
                StatsMenuElement.SetActive(true); 
            }
        }
    }
}
