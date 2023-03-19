// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 28/02/2023

using UnityEngine;

/// <summary>
/// Allows the player to pick up, drop, throw and manipulate gameObjects with said script attacted.
/// Based off the first playtesting session, this system didnt work very well with the intended platforming 
/// of the level design. As a result I've removed this script from any obejcts within the scenes and rewrote a new
/// pick up system under the "PickUpUtem.cs" Script. Replaces this for a more user friendly version 
/// </summary>
namespace DO
{
    public class Pickupable : MonoBehaviour
    {
        [Header("Can you Hold this?")]
        public bool canHold = true;
        [Header("Throw-Force")]
        public float throwForce = 30f;
        [Header("Item to Hold")]
        public GameObject item;
        [Header("Players Transform Point")]
        public GameObject pickupPoint;
        [Header("InputHandler Here")]
        public InputHandler inputHandler;

        [HideInInspector] Vector3 objectPosition;
        [HideInInspector] float distance;

        private void Update()
        {
            distance = Vector3.Distance(item.transform.position, pickupPoint.transform.position); 
            if(distance >= 0.7f)
            {
                inputHandler.isGrabbing = false;
                LetGo();
            }

            if (inputHandler.isGrabbing == true)
            {
                inputHandler.isGrabbing = true; 
                PickUp(); 
                item.GetComponent<Rigidbody>().velocity = Vector3.zero;
                item.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                item.transform.SetParent(pickupPoint.transform);

                if (inputHandler.isGrabbing == true && inputHandler.isThrowing == true)
                {
                    //Throw
                    item.GetComponent<Rigidbody>().AddForce(pickupPoint.transform.forward * throwForce);
                }
            }
            else
            {
                objectPosition = item.transform.position;
                item.transform.SetParent(null);
                item.GetComponent<Rigidbody>().useGravity = true;
                item.transform.position = objectPosition;
            }
        }

        public void PickUp()
        {
            if(distance <= 0.7f)
            {
                inputHandler.isGrabbing = true;
                item.GetComponent<Rigidbody>().useGravity = false;
                item.GetComponent<Rigidbody>().detectCollisions = true;
            }    
        }

        public void LetGo()
        {
            inputHandler.isGrabbing = false;
        }
    }
}