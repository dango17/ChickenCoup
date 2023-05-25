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

        public bool isHoldingItem;
        [SerializeField] public bool canMove;
        [SerializeField] public bool canConceal;
        [SerializeField] public bool isFood;
        [SerializeField] public bool isSugary;
        [SerializeField] public bool isFuse;
        [Header("Eaten ParticleEffect")]
        public GameObject EatEffectPrefab;

        InputHandler inputHandler;
        PlayerController controller;
        ObjectiveManager objectiveManager; 
        private Material[] originalMaterials;

        //0.05f is equal to 0.15 in the inspector on moveSpeed?!
        private float speedBoost = 0.025f; //(0.15f)

        public void Start()
        {
            controller = FindObjectOfType<PlayerController>();
            inputHandler = FindObjectOfType<InputHandler>();
            objectiveManager = FindObjectOfType<ObjectiveManager>();

            //Save the original materials array
            originalMaterials = meshRenderer.materials;

            //Disable the second material on the mesh renderer
            Material[] materials = meshRenderer.materials;
            materials[1] = originalMaterials[0];
            meshRenderer.materials = materials;
        }

        public void Update()
        {
            if(isHoldingItem == true)
            {
                //Get all instances of this script in the scene
                PickupItem[] pickupItems = FindObjectsOfType<PickupItem>();

                //Disable the script on each of the gameObjects
                foreach (PickupItem pickupItem in pickupItems)
                {
                    pickupItem.enabled = false;
                    this.enabled = true; 
                }
            }
            else if (!isHoldingItem == false)
            {
                // Get all instances of this script in the scene
                PickupItem[] pickupItems = FindObjectsOfType<PickupItem>();

                // Disable the script on each of the gameObjects
                foreach (PickupItem pickupItem in pickupItems)
                {
                    pickupItem.enabled = true;
                }
            }
        }

        //OTE && OTS Functions are identically to eachother 
        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && this.enabled)
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

                if (inputHandler.isGrabbing == true && canMove == true && isFuse)
                {
                    PickUpItem();
                    objectiveManager.hasFoundFuse = true; 
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
            if (other.tag == "Player" && this.enabled)
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

            isHoldingItem = true;          
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
            currentObject.GetComponent<Rigidbody>().useGravity = true;
            currentObject.GetComponent<BoxCollider>().enabled = true;

            isHoldingItem = false;

            //Get all instances of this script in the scene
            PickupItem[] pickupItems = FindObjectsOfType<PickupItem>();

            //Disable the script on each of the gameObjects
            foreach (PickupItem pickupItem in pickupItems)
            {
                pickupItem.enabled = true;
            }
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