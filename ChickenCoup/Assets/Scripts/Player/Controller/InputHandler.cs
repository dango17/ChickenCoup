// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 27/01/2023

using UnityEngine;
using System.Collections; 

namespace DO
{
    /// <summary>
    /// Handles and Holds all relevant logic for Inputs.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        public Transform camHolder;

        public ExecutionOrder movementOrder;
        public PlayerController controller;
        public CameraManager cameraManager; 

        Vector3 moveDirection;
        public float wallDetectionDistance = 0.2f;
        public float wallDetectionDistanceOnWall = 1.2f;

        float horizontal;
        float vertical;
        float moveAmount;

        bool freeLook;
        public float runningTimer = 5f;
        public float staminaTimer = 4f; 

        public bool isJumping;
        public bool isSprinting;
        public bool isTired; 

        public enum ExecutionOrder
        {
            fixedUpdate, update, lateUpdate
        }

        private void FixedUpdate()
        {
            if(movementOrder == ExecutionOrder.fixedUpdate)
            {
                //Encapsulate so I can add additonal data later 
                HandleMovement(moveDirection, Time.fixedDeltaTime);
            }
        }

        private void Start()
        {
            cameraManager.wallCameraObject.SetActive(false);
            cameraManager.mainCameraObject.SetActive(true);
            cameraManager.fpCameraObject.SetActive(false); 
        }

        private void Update()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            freeLook = Input.GetKey(KeyCode.F);

            //isSprinting = Input.GetKeyDown(KeyCode.LeftShift);
            isJumping = Input.GetKeyDown(KeyCode.Space);

            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

            moveDirection = camHolder.forward * vertical;
            moveDirection += camHolder.right * horizontal;
            moveDirection.Normalize();

            float delta = Time.deltaTime; 

            //Jumping
            if(isJumping && controller.isGrounded)
            {
                controller.HandleJump(); 
            }
            else if (controller.isGrounded == false)
            {
                controller.handleFalling(); 
            }

            //Sprinting 
            if(isTired == false && Input.GetKeyDown(KeyCode.LeftShift))
            {
                controller.moveSpeed = 3.5f;
                isSprinting = true;
                isTired = false; 
                StartCoroutine(RunTimer()); 
            }
            IEnumerator RunTimer()
            {
                yield return new WaitForSeconds(runningTimer);
                isSprinting = false;

                controller.moveSpeed = 2f;
                isTired = true; 

                if(isTired == true && (Input.GetKeyDown(KeyCode.LeftShift)))
                {
                    isSprinting = false; 
                }

                StartCoroutine(RunCoolDown());
            } 
            IEnumerator RunCoolDown()
            {
                yield return new WaitForSeconds(staminaTimer);
                isTired = false;

            }

            //First Person
            if (freeLook)
            {
                cameraManager.fpCameraObject.SetActive(true);      
                controller.FPRotation(horizontal, delta);
                controller.isInFreeLook = true;

            }
            else
            {
                cameraManager.fpCameraObject.SetActive(false);
                controller.isInFreeLook = false;
            }

            //Execution Order
            if(movementOrder == ExecutionOrder.update)
            {
                HandleMovement(moveDirection, delta);
            }

            controller.HandleAnimatorStates();
        }

        void HandleMovement(Vector3 moveDirection, float delta)
        {
            if(freeLook)
            {
                return; 
            }

            Vector3 origin = controller.transform.position;
            origin.y += 1;

            bool willStickToWall = false;
            Vector3 wallNormal = Vector3.zero;

            float detectDistance = wallDetectionDistance; 
            if(controller.isOnCover)
            {
                detectDistance = wallDetectionDistanceOnWall; 
            }

            Debug.DrawRay(origin, moveDirection * detectDistance);

            if (Physics.SphereCast(origin, 0.25f, moveDirection, out RaycastHit hit, detectDistance, controller.coverLayer))
            {
                willStickToWall = true;
                wallNormal = hit.normal; 
            }

            if (willStickToWall)
            {
                controller.isOnCover = true; 
                controller.WallMovement(moveDirection,hit.normal, delta);

                cameraManager.wallCameraObject.SetActive(true);
                cameraManager.mainCameraObject.SetActive(false);
            }
            else
            {
                controller.isOnCover = false; 

                cameraManager.wallCameraObject.SetActive(false);
                cameraManager.mainCameraObject.SetActive(true);

                controller.Move(moveDirection, delta);
                controller.HandleMovementAnimations(moveAmount, delta);
            }
        }
    }
}

