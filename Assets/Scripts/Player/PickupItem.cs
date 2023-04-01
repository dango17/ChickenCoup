// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 19/03/2023

using UnityEngine;
using System.Collections; 

namespace DO
{
    //Allows for the player to pick up items into their pickUpPoint.
    //More user firendly than the last 'pickupable.cs' script
    public class PickupItem : MonoBehaviour
    {
        [Header("Current GameObject")]
        [SerializeField] public GameObject currentObject;
        [Header("PickupPoint Transform")]
        [SerializeField] public Transform pickupPoint;
        [Header("Input-Handler")]
        [SerializeField] public InputHandler inputHandler;
        [Header("What Item Is This?")]
        [SerializeField] public bool canMove;
        [SerializeField] public bool isFood;
        [SerializeField] public bool isSugary;
        [SerializeField] public bool isAlcohol;
        [Header("Eaten ParticleEffect")]
        public GameObject EatEffectPrefab;

        #region Pickup Items
        //OTE && OTS Functions are identically to eachother 
        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                //Player can pick up item as normal if canMove == true
                if (inputHandler.isGrabbing == true && canMove == true)
                {
                    currentObject.GetComponent<Rigidbody>().isKinematic = true;
                    currentObject.GetComponent<BoxCollider>().enabled = false;
                    currentObject.transform.parent = pickupPoint.transform;
                    currentObject.transform.localPosition = Vector3.zero;
 
                }
                //Player will temp hold food, held long enough == Eaten
                if (inputHandler.isGrabbing == true && isFood == true)
                {
                    currentObject.GetComponent<Rigidbody>().isKinematic = true;
                    currentObject.GetComponent<BoxCollider>().enabled = false;
                    currentObject.transform.parent = pickupPoint.transform;
                    currentObject.transform.localPosition = Vector3.zero;

                    //Temp spawn more eggs here

                    Invoke("DestoryCurrentObject", 0.75f); 
                }
                //Do Sugary Foods Here (speed boost player)

                //Do Alcohol Foods Here (Add chromatic abbrasion)
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

        public void DestoryCurrentObject()
        {
            GameObject particleEffect = Instantiate(EatEffectPrefab, currentObject.transform.position, Quaternion.identity);
            Destroy(currentObject);
        }

        private IEnumerator DestroyParticleEffect(GameObject EatEffectPrefab, ParticleSystem particleSystem)
        {
            while (particleSystem.IsAlive())
            {
                yield return null;
            }
            Destroy(EatEffectPrefab);
        }


        public void DropItem()
        {
            currentObject.transform.parent = null;
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<BoxCollider>().enabled = true;
        }
    }
}

