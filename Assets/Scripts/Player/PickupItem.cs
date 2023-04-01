// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 19/03/2023

using UnityEngine;

namespace DO
{
    //Allows for the player to pick up items into their pickUpPoint.
    //More user firendly than the last 'pickupable.cs' script
    public class PickupItem : MonoBehaviour
    {
        [SerializeField] public GameObject currentObject;
        [SerializeField] public Transform pickupPoint;
        [SerializeField] public InputHandler inputHandler;

        public bool isHoldingItem = false; 

        #region Pickup Items
        //OTE && OTS Functions are identically to eachother 
        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && inputHandler.isGrabbing == true)
            {
                currentObject.GetComponent<Rigidbody>().isKinematic = true;
                currentObject.GetComponent<BoxCollider>().enabled = false;
                currentObject.transform.parent = pickupPoint.transform;
                currentObject.transform.localPosition = Vector3.zero;
                isHoldingItem = true;
                
                if(isHoldingItem)
                {
                    //Do nothing
                }
            }
            else if (inputHandler.isGrabbing == false)
            {
                DropItem();
                isHoldingItem = false; 
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.tag == "Player" && inputHandler.isGrabbing == true)
            {
                currentObject.GetComponent<Rigidbody>().isKinematic = true;
                currentObject.GetComponent<BoxCollider>().enabled = false;
                currentObject.transform.parent = pickupPoint.transform;
                currentObject.transform.localPosition = Vector3.zero;
                isHoldingItem = true; 
            }
            else if (inputHandler.isGrabbing == false) 
            {
                DropItem(); 
            }
        }
        #endregion

        public void DropItem()
        {
            currentObject.transform.parent = null;
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<BoxCollider>().enabled = true;
        }
    }
}

