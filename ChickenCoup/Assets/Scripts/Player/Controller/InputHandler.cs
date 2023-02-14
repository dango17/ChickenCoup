//File Name : InputHandler.cs
//Author : Daniel Oldham/s1903729
//Collaborator : N/A
//Created On : 27/01/23
//Last Modified : 08/02/23
//Description : Handles and Holds all relevant logic for Inputs  

using UnityEngine;

namespace DO
{
    public class InputHandler : MonoBehaviour
    {
        public Transform camHolder;

        public ExecutionOrder movementOrder;
        public PlayerController controller;
        public CameraManager cameraManager; 

        Vector3 moveDirection;
        public float wallDetectionDistance;

        float horizontal;
        float vertical;
        float moveAmount;

        bool freeLook;
        public bool isJumping;
        public bool isSprinting; 

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

            isSprinting = Input.GetKey(KeyCode.LeftShift);
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
            if(isSprinting)
            {
                controller.HandleRun(); 
            }
            else
            {
                controller.HandleRunCoolDown(); 
            }

            //First Person
            if(freeLook)
            {
                cameraManager.fpCameraObject.SetActive(true);      
                controller.FPRotation(horizontal, delta);

            }
            else
            {
                cameraManager.fpCameraObject.SetActive(false); 
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

            Debug.DrawRay(origin, moveDirection * wallDetectionDistance);
            if(Physics.SphereCast(origin, 0.25f, moveDirection,out RaycastHit hit, wallDetectionDistance, controller.coverLayer))
            {
                cameraManager.wallCameraObject.SetActive(true);
                cameraManager.mainCameraObject.SetActive(false);

                controller.isOnCover = true; 

                controller.WallMovement(moveDirection,hit.normal, delta); 
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

