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
        [SerializeField] public Transform concealmentPoint;
        [SerializeField] public MeshRenderer meshRenderer;
        [Header("ItemType")]
        [SerializeField] public bool canMove;
        [SerializeField] public bool canConceal;
        [SerializeField] public bool isFood;
        [SerializeField] public bool isSugary;
        [SerializeField] public bool isAlcohol;
        [Header("Eaten ParticleEffect")]
        public GameObject EatEffectPrefab;

        InputHandler inputHandler;
        PlayerController controller;
        private Material[] originalMaterials;

        //0.05f is equal to 0.15 in the inspector on moveSpeed?!
        private float speedBoost = 0.025f; //(0.15f)

        public void Start()
        {
            controller = FindObjectOfType<PlayerController>();
            inputHandler = FindObjectOfType<InputHandler>();

            //Save the original materials array
            originalMaterials = meshRenderer.materials;

            //Disable the second material on the mesh renderer
            Material[] materials = meshRenderer.materials;
            materials[1] = originalMaterials[0];
            meshRenderer.materials = materials;
        }

        public void Update()
        {
       
        }

        //OTE && OTS Functions are identically to eachother 
        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                //Turn on outline shader here
                Material[] materials = meshRenderer.materials;
                materials[1] = originalMaterials[1];
                meshRenderer.materials = materials;

                #region Pick Up Items 
                //Player can pick up item as normal if canMove == true
                if (inputHandler.isGrabbing == true && canMove == true)
                {
                    PickUpItem(); 
                }
                #endregion

                #region Normal Consumables
                //Player will temp hold food, held long enough == Eaten
                if (inputHandler.isGrabbing == true && isFood == true)
                {
                    EatNormalFood(); 
                }
                #endregion

                #region Sugary Consumables
                //Sugary Foods Here (speed boost player)
                if (inputHandler.isGrabbing == true && isSugary == true)
                {
                    EatSugaryFood(); 
                }
                #endregion

                #region Concealment Items
                else if (inputHandler.isGrabbing == true && canConceal == true)
                {
                    ConcealPlayer(); 
                }
                #endregion

                //Do Alcohol Foods Here (Add chromatic abbrasion)
            }
            else if (inputHandler.isGrabbing == false)
            {
                DropItem();
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.tag == "Player")
            {
                //Turn on outline shader here
                Material[] materials = meshRenderer.materials;
                materials[1] = originalMaterials[1];
                meshRenderer.materials = materials;

                if (other.tag == "Player" && inputHandler.isGrabbing == true)
                {
                    PickUpItem();
                }
                if (other.tag == "Player" && inputHandler.isGrabbing && canConceal == true)
                {
                    ConcealPlayer();
                }

                else if (inputHandler.isGrabbing == false)
                {
                    DropItem();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //Disable the outline material 
            Material[] materials = meshRenderer.materials;
            materials[1] = originalMaterials[0];
            meshRenderer.materials = materials;
        }

        public void PickUpItem()
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<BoxCollider>().enabled = false;
            currentObject.transform.parent = pickupPoint.transform;
            currentObject.transform.localPosition = Vector3.zero;

            //Disable the outline material 
            Material[] materials = meshRenderer.materials;
            materials[1] = originalMaterials[0];
            meshRenderer.materials = materials;

            controller.objectInHand = true;          
        }

        public void EatNormalFood()
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<BoxCollider>().enabled = false;
            currentObject.transform.parent = pickupPoint.transform;
            currentObject.transform.localPosition = Vector3.zero;

            //Temp spawn more eggs here

            Invoke("DestroyCurrentObject", 0.75f);
        }

        public void EatSugaryFood()
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<BoxCollider>().enabled = false;
            currentObject.transform.parent = pickupPoint.transform;
            currentObject.transform.localPosition = Vector3.zero;

            controller.moveSpeed += speedBoost;

            Invoke("DestroyCurrentObject", 0.75f);
        }

        public void ConcealPlayer()
        {
            currentObject.GetComponent<Rigidbody>().isKinematic = true;
            currentObject.GetComponent<BoxCollider>().enabled = false;
            currentObject.transform.parent = concealmentPoint.transform;

            //Rotate the current object 180 degrees on the chicken
            currentObject.transform.rotation = Quaternion.Euler(180f, 0f, 0f);

            //Disable the outline material 
            Material[] materials = meshRenderer.materials;
            materials[1] = originalMaterials[0];
            meshRenderer.materials = materials;

            inputHandler.isConcealed = true; 

            currentObject.transform.localPosition = Vector3.zero;
        }

        public void DropItem()
        {
            currentObject.transform.parent = null;
            currentObject.GetComponent<Rigidbody>().isKinematic = false;
            currentObject.GetComponent<BoxCollider>().enabled = true;

            inputHandler.isConcealed = false; 
        }

        public void DestroyCurrentObject()
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

        public void DoNothing(){}
    }
} 