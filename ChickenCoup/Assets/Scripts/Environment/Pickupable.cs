// Author: Daniel Oldham/s1903729d
// Collaborator: N/A
// Created On: 28/02/2023

using UnityEngine;

/// <summary>
/// Slides open specified GameObjects to resemble a sliding door.
/// </summary>
namespace DO
{
    public class Pickupable : MonoBehaviour
    {
        float throwForce;
        Vector3 objectPosition;
        float distance;

        public bool canHold = true;
        public GameObject item;
        public GameObject pickupPoint;

        public InputHandler inputHandler;

        private void Start()
        {
            //inputHandler = GetComponent<InputHandler>();
        }

        private void Update()
        {
            distance = Vector3.Distance(item.transform.position, pickupPoint.transform.position); 
            if(distance >= 1)
            {
                inputHandler.isHolding = false;
                LetGo();
            }

            if (inputHandler.isHolding == true)
            {
                PickUp(); 
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

        public void PickUp()
        {
            if(distance <= 1f)
            {
                inputHandler.isHolding = true;
                item.GetComponent<Rigidbody>().useGravity = false;
                item.GetComponent<Rigidbody>().detectCollisions = true;
            }    
        }

        public void LetGo()
        {
            inputHandler.isHolding = false;
        }
    }
}