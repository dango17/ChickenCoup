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
        [SerializeField] public GameObject playerModel; 
        PlayerControls inputActions; 

        [Header("Components")]
        [SerializeField] Vector3 moveDirection;
        [SerializeField] Vector3 lookInputDirection;
        [SerializeField] public float wallDetectionDistance = 0.2f;
        [SerializeField] public float wallDetectionDistanceOnWall = 1.2f;

        [Header("Movement")]
        Vector2 moveInputDirection;
        [SerializeField] float moveAmount;

        [Header("Sprint")]
        [SerializeField] public float runningTimer = 5f;
        [SerializeField] public float staminaTimer = 4f;

        [Header("Flags")]
        public bool isFP;
        public bool isJumping;
        public bool isSprinting;
        public bool isInteracting;
        public bool isClucking; 
        public bool isTired;
        public bool isGrabbing;
        public bool isThrowing;
        public bool FPSModeInit; 

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
            //First Person CameraDir Input
            inputActions.Player.FPCameraDirection.performed += i => lookInputDirection = i.ReadValue<Vector2>();
            //First Person Inputs
            inputActions.Player.FirstPerson.started += i => isFP = true; cameraManager.tiltAngle = 0;
            inputActions.Player.FirstPerson.canceled += i => isFP = false;
            //Jump Input (New weird behaviour, jump seems to occasionally multiply)
            inputActions.Player.Jump.performed += i => isJumping = true;
            //Sprint Input 
            inputActions.Player.Sprint.performed += i => isSprinting = true;
            //Grabbing Input 
            inputActions.Player.Grab.started += i => isGrabbing = true;
            inputActions.Player.Grab.canceled += i => isGrabbing = false;
            //Clucking Input 
            inputActions.Player.Cluck.performed += i => isClucking = true;
            //inputActions.Player.Cluck.canceled += i => isClucking = false; 

            inputActions.Enable(); 

            cameraManager.wallCameraObject.SetActive(false);
            cameraManager.mainCameraObject.SetActive(true);
            cameraManager.fpCameraObject.SetActive(false); 
        }

        private void OnEnable() {
			inputActions.Enable();
		}

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            #region FP Automatic Mode
            //FPMode = Automatically move to first person state
            //for things like vents, under tables, etc. 
            if (controller.isFPMode)
            {
                if(!FPSModeInit)
                {
                    cameraManager.tiltAngle = 0;
                    cameraManager.fpCameraObject.SetActive(true);
                    playerModel.SetActive(false);
                    FPSModeInit = true;
                    controller.rotateSpeed = 1.2f; 
                }

                moveDirection = controller.mTransform.forward * moveInputDirection.y;
                moveDirection += controller.mTransform.right * moveInputDirection.x;
                moveDirection.Normalize();

                controller.FPRotation(moveInputDirection.x, delta);
                cameraManager.HandleFPSTile(lookInputDirection.y, delta);
                controller.Move(moveDirection, delta);

                return;
            }
            if(FPSModeInit)
            {
                cameraManager.tiltAngle = 0;
                cameraManager.fpCameraObject.SetActive(false);
                playerModel.SetActive(true);
                FPSModeInit = false;
                controller.rotateSpeed = 0.01f; 
            }
            #endregion

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
            if (isFP)
            {
                cameraManager.fpCameraObject.SetActive(true);
                playerModel.SetActive(false);
                controller.FPRotation(lookInputDirection.x, delta);
                cameraManager.HandleFPSTile(lookInputDirection.y, delta); 
                controller.isInFreeLook = true;
            }
            else
            {
                cameraManager.fpCameraObject.SetActive(false);
                playerModel.SetActive(true);
                controller.isInFreeLook = false;
                cameraManager.tiltAngle = 0;
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
            if(isFP)
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

