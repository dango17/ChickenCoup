using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DO
{
    public class Pickupable : MonoBehaviour
    {
        float throwForce = 600;
        Vector3 objectPosition;
        float distance;

        public bool canHold = true;
        public GameObject item;
        public GameObject pickupPoint;
        public bool isHolding = false;

        private void Update()
        {
            distance = Vector3.Distance(item.transform.position, pickupPoint.transform.position); 
            if(distance >= 1)
            {
                isHolding = false; 
            }

            if (isHolding == true)
            {
                item.GetComponent<Rigidbody>().velocity = Vector3.zero;
                item.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                item.transform.SetParent(pickupPoint.transform);

                if (Input.GetMouseButtonDown(1))
                {
                    //Throw
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

        private void OnMouseDown()
        {
            if(distance <= 1f)
            {
                isHolding = true;
                item.GetComponent<Rigidbody>().useGravity = false;
                item.GetComponent<Rigidbody>().detectCollisions = true;
            }    
        }

        private void OnMouseUp()
        {
            isHolding = false;
        }
    }
}