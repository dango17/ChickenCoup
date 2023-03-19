using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class PickupItem : MonoBehaviour
    {
        [SerializeField] public GameObject currentObject;
        [SerializeField] public Transform pickupPoint;
        [SerializeField] public InputHandler inputHandler;

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
            }
            else if (inputHandler.isGrabbing == false)
            {
                DropItem();
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

