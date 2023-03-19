// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 27/01/2023

using UnityEngine;
using System.Collections; 

namespace DO
{
    //Handles and Holds all relevant logic for Inputs.
    public class InputHandler : MonoBehaviour
    {
        [Header("Camera-Holder")]
        [SerializeField] public Transform camHolder;

        [Header("Components")]
        [SerializeField] public ExecutionOrder movementOrder;
        [SerializeField] public PlayerController controller;
        [SerializeField] public CameraManager cameraManager;
        PlayerControls inputActions; 

        [Header("Components")]
        [SerializeField] Vector3 moveDirection;
        [SerializeField] public float wallDetectionDistance = 0.2f;
        [SerializeField] public float wallDetectionDistanceOnWall = 1.2f;

        [Header("Movement")]
        Vector2 moveInputDirection;
        [SerializeField] float moveAmount;

        [Header("Sprint")]
        [SerializeField] public float runningTimer = 5f;
        [SerializeField] public float staminaTimer = 4f;

        [Header("Flags")]
        public bool freeLook;
        public bool isJumping;
        public bool isSprinting;
        public bool isInteracting; 
        public bool isTired;
        public bool isGrabbing;
        public bool isThrowing; 

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
            inputActions = new PlayerControls();

            //Delegates that runs the method anytime the inputs are pressed
            //Movement Inputs
            inputActions.Player.Movement.performed += i => moveInputDirection = i.ReadValue<Vector2>();
            //First Person Inputs
            inputActions.Player.FirstPerson.started += i => freeLook = true;
            inputActions.Player.FirstPerson.canceled += i => freeLook = false;
            //Jump Input (New weird behaviour, jump seems to occasionally multiply)
            inputActions.Player.Jump.performed += i => isJumping = true;
            //Sprint Input 
            inputActions.Player.Sprint.performed += i => isSprinting = true;
            //Pickup Input 
            inputActions.Player.Grab.started += i => isGrabbing = true;
            inputActions.Player.Grab.canceled += i => isGrabbing = false;

            inputActions.Enable(); 

            cameraManager.wallCameraObject.SetActive(false);
            cameraManager.mainCameraObject.SetActive(true);
            cameraManager.fpCameraObject.SetActive(false); 
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            isInteracting = Input.GetKey(KeyCode.E);
            isThrowing = Input.GetKey(KeyCode.Mouse0); 

            moveAmount = moveInputDirection.magnitude;

            moveDirection = camHolder.forward * moveInputDirection.y;
            moveDirection += camHolder.right * moveInputDirection.x;
            moveDirection.Normalize();

            #region Jumping & Running
            //Jumping
            if (isJumping && controller.isInFreeLook == false)
            {
                controller.HandleJump();
            }
            else if (controller.isGrounded == false)
            {
                controller.handleFalling();
            }

            //Sprinting 
            if (isTired == false && isSprinting == true)
            {
                controller.moveSpeed = 4f;
                isSprinting = true;
                isTired = false; 
                StartCoroutine(RunTimer()); 
            }
            IEnumerator RunTimer()
            {
                yield return new WaitForSeconds(runningTimer);
                isSprinting = false;

                controller.moveSpeed = 3f;
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
            #endregion

            #region First Person Camera
            //First Person
            if (freeLook)
            {
                cameraManager.fpCameraObject.SetActive(true);      
                controller.FPRotation(moveInputDirection.x, delta);
                controller.isInFreeLook = true;
            }
            else
            {
                cameraManager.fpCameraObject.SetActive(false);
                controller.isInFreeLook = false;
            }
            #endregion

            //Execution Order
            if (movementOrder == ExecutionOrder.update)
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

