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
        [SerializeField] public CageController cageController; 
        [SerializeField] public CameraManager cameraManager;
        [SerializeField] public Farmer farmer; 
        [SerializeField] public GameObject playerModel;
        [SerializeField] public GameObject playerLeftEye;
        [SerializeField] public GameObject playerRightEye;
        [SerializeField] public GameObject ojectiveMenu;
        [SerializeField] public Animator UIAnims;
        PlayerControls inputActions; 

        [Header("Components")]
        [SerializeField] public Vector3 moveDirection;
        [SerializeField] public Vector3 lookInputDirection;
        [SerializeField] public float wallDetectionDistance = 0.2f;
        [SerializeField] public float wallDetectionDistanceOnWall = 1.2f;

        [Header("Movement")]
        Vector2 moveInputDirection;
        [SerializeField] float moveAmount;

        [Header("Sprint")]
        [SerializeField] public float runningTimer = 5f;
        [SerializeField] public float staminaTimer = 4f;
        [SerializeField] public float sprintIncrease = 1f;

       [Header("Flags")]
        public bool isFP;
        public bool isJumping;
        public bool isSprinting;
        public bool isInteracting;
        public bool isClucking; 
        public bool isTired;
        public bool isGrabbing;
        public bool isThrowing;
        public bool isLayingEgg; 
        public bool FPSModeInit;
        public bool isConcealed;
        public bool isToggledMenu; 

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

            //Jump Input
            inputActions.Player.Jump.performed += i => isJumping = true;

            //Sprint Input 
            inputActions.Player.Sprint.started += i => isSprinting = true;
            inputActions.Player.Sprint.canceled += i => isSprinting = false;

            //Grabbing Input 
            inputActions.Player.Grab.started += i => isGrabbing = true;
            inputActions.Player.Grab.canceled += i => isGrabbing = false;

            //Clucking Input
            inputActions.Player.Cluck.started += i => isClucking = true;
            inputActions.Player.Cluck.canceled += i => isClucking = false; 

            //LayEgg Input
            inputActions.Player.LayEgg.started += i => isLayingEgg = true;
            inputActions.Player.LayEgg.canceled += i => isLayingEgg = false;

            //Open Objective Menu 
            inputActions.Player.ToggleMenu.started += i => isToggledMenu = true;
            inputActions.Player.ToggleMenu.canceled += i => isToggledMenu = false;

            inputActions.Enable(); 

            cameraManager.wallCameraObject.SetActive(false);
            cameraManager.mainCameraObject.SetActive(true);
            cameraManager.fpCameraObject.SetActive(false); 

            cageController = FindObjectOfType<CageController>();
            farmer = FindObjectOfType<Farmer>(); 
        }

        private void OnEnable() {
            if (inputActions != null) {
                inputActions.Enable();
            }
		}

        private void OnDisable()
        {
            if (inputActions != null) {
                inputActions.Disable();
            }
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

                    moveDirection = Vector3.zero; 
                }

                moveDirection = controller.mTransform.forward * moveInputDirection.y;
                moveDirection += controller.mTransform.right * moveInputDirection.x;
                moveDirection.Normalize();

                controller.FPRotation(moveInputDirection.x, delta);
                cameraManager.HandleFPSTilt(lookInputDirection.y, delta);
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
            else
            {
                controller.rotateSpeed = 0.1f;
            }
            #endregion

            moveAmount = moveInputDirection.magnitude;

            moveDirection = camHolder.forward * moveInputDirection.y;
            moveDirection += camHolder.right * moveInputDirection.x;
            moveDirection.Normalize();

            #region Jumping & Running
            //Jumping
            if (isJumping && controller.isInFreeLook == false && controller.isGrounded == true 
                && cageController.isLocked == false && farmer.HasCaughtPlayer == false)
            {
                controller.animator.Play("Jump");
                controller.Jump();
            }
            else if (controller.isGrounded == false)
            {
                controller.handleFalling();
            }

            //Sprinting 
            if (isTired == false && isSprinting == true)
            {
                controller.moveSpeed += 0.01f;

                if (controller.moveSpeed > controller.MaximumMovementSpeed) {
                    controller.moveSpeed = controller.MaximumMovementSpeed;
				}

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
                playerLeftEye.SetActive(false);
                playerRightEye.SetActive(false); 

                controller.FPRotation(lookInputDirection.x, delta);
                cameraManager.HandleFPSTilt(lookInputDirection.y, delta); 
                controller.isInFreeLook = true;
            }
            else
            {
                cameraManager.fpCameraObject.SetActive(false);
                playerModel.SetActive(true);
                playerLeftEye.SetActive(true);
                playerRightEye.SetActive(true); 

                controller.isInFreeLook = false;
                cameraManager.tiltAngle = 0;
            }
            #endregion

            #region EggLaying

            if(isLayingEgg && controller.timeSinceLastSpawn >= controller.cooldownTime)
            {
                controller.HandleEggSpawning(); 
            }
            else
            {
                controller.HandleEggCoolDown(); 
            }

            #endregion

            #region Clucking
            if(isClucking == true)
            {
                controller.HandleClucking();

                //Play UI cluck element 
                UIAnims.SetBool("isClucking", true);
            }
            if(isClucking == false)
            {
                //Play UI cluck element 
                UIAnims.SetBool("isClucking", false);
            }

            #endregion

            #region Toggle Objective Menu
            if(isToggledMenu)
            {
                ojectiveMenu.SetActive(true); 
            }
            if(!isToggledMenu)
            {
                ojectiveMenu.SetActive(false);
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

