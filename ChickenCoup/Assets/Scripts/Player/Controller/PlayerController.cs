// Author: Daniel Oldham/s1903729
// Collaborator: N/A
// Created On: 27/01/2023

using UnityEngine;
using System.Collections;

namespace DO
{
    /// <summary>
    /// playerManager that handles movement types and the relevant logic to execute.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Speeds")]
        [SerializeField] public float moveSpeed = 0.4f;
        [SerializeField] public float sprintSpeed = 0.8f;
        [SerializeField] public float rotateSpeed = 0.2f;
        [SerializeField] public float FPRotationSpeed = 0.2f; 
        [Header("Jumping")]
        [SerializeField] public float jumpForce = 5f;
        [SerializeField] public float fallForce = 8f;
        [SerializeField] public Vector3 jump; 
        [Header("Ground-Check")]
        [SerializeField] public LayerMask groundLayer;
        [SerializeField] public float raycastDistance = 0.2f; 
        [Header("Cover")]
        [SerializeField] public float wallSpeed = 0.2f;
        [SerializeField] public float wallCheckDistance = 0.2f;
        [SerializeField] public LayerMask coverLayer;
        [Header("Peeking")]
        public float wallCamXPosition = 1;
        public Transform wallCameraParent;
        [Header("Flags")]
        [SerializeField] public bool isOnCover;
        [SerializeField] public bool isGrounded;
        [SerializeField] public bool isInFreeLook; 

        [HideInInspector] public Transform mTransform;
        [HideInInspector] public Animator animator;
        [HideInInspector] new Rigidbody rigidbody;
        [HideInInspector] public SkinnedMeshRenderer meshRenderer;
        [HideInInspector] public InputHandler inputHandler;

        private void Start()
        {
            mTransform = this.transform;
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
            inputHandler = GetComponent<InputHandler>(); 
        }

        private void Update()
        {
            //Ground Check the player
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
            {
                isGrounded = true; 
            }
            else
            {
                isGrounded = false; 
            }
        }

        public void WallMovement(Vector3 moveDirection, Vector3 normal, float delta)
        {
            float dot = Vector3.Dot(moveDirection, Vector3.forward);
            Vector3 wallCameraTargetPosition = Vector3.zero; 
            if (dot < 0)
            {
                moveDirection.x *= -1; 
            }
            HandleRotation(normal, delta);
 
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normal);
            Debug.DrawRay(mTransform.position, projectedVelocity, Color.blue);

            Vector3 relativeDir = mTransform.InverseTransformDirection(projectedVelocity);

            Vector3 origin = mTransform.position;
            origin.y += 1;

            if (Mathf.Abs(relativeDir.x) > 0.01f)
            {
                if (relativeDir.x > 0)
                    origin += mTransform.right * wallCheckDistance;

                if (projectedVelocity.x < 0)
                    origin -= mTransform.right * wallCheckDistance; 

                Debug.DrawRay(origin, -normal, Color.red);
                if (Physics.Raycast(origin, -normal, out RaycastHit hit, 2f))
                {
                    //Do nothing
                }
                else
                {
                    projectedVelocity = Vector3.zero;
                    wallCameraTargetPosition.x = wallCamXPosition * ((relativeDir.x < 0) ? -1:1) ; 
                    relativeDir.x = 0; 
                }
            }
            else
            {
                projectedVelocity = Vector3.zero;
                relativeDir.x = 0; 
            }

            rigidbody.velocity = projectedVelocity * wallSpeed;

            float m = 0;
       
            //Looking inward therefore 
            m = relativeDir.x;

            if(m < 0.1f && m > -0.1f)
            {
                m = 0;
            }
            else
            {
                m = (m < 0) ? -1 : 1; 
            }

            animator.SetFloat("movement", m, 0.1f, delta);
            wallCameraParent.localPosition = Vector3.Lerp(wallCameraParent.localPosition, wallCameraTargetPosition, delta / 0.2f); 
        }

        public void Move(Vector3 moveDirection, float delta)
        { 
            rigidbody.velocity = moveDirection * moveSpeed;
            HandleRotation(moveDirection, delta);         
        }

        void HandleRotation(Vector3 lookDir, float delta)
        {
            //Handle Rotation
            //lookDir = currentRotation point      
            if (lookDir == Vector3.zero)
                lookDir = mTransform.forward;

            Quaternion lookRotation = Quaternion.LookRotation(lookDir);
            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
        }

        public void FPRotation(float horizontal, float delta)
        {
            Vector3 targetEuler = mTransform.eulerAngles;
            targetEuler.y += horizontal * delta / FPRotationSpeed; 

            mTransform.eulerAngles = targetEuler; 
        }

        public void HandleAnimatorStates()
        {
            animator.SetBool("isOnCover", isOnCover);
        }

        public void HandleMovementAnimations(float moveAmount, float delta)
        {
            //Set moveAmount Values 
            float m = moveAmount;
            if (m > 0.1f && m < 0.51f)
                m = 0.5f;

            if (m > 0.51f)
                m = 1;

            if (m < 0.1f)
                m = 0f;

            animator.SetFloat("movement", m, 0.1f, delta);
        }


        public void HandleJump()
        {
            inputHandler.isJumping = true;
            rigidbody.AddForce(jump * jumpForce, ForceMode.Force); 
        }

        public void handleFalling()
        {
            inputHandler.isJumping = false; 
            rigidbody.AddForce(Vector3.down * fallForce, ForceMode.Impulse); 
        }
    }
}
